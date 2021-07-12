using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Commands;
using Venflow.Modeling;

namespace Venflow.Dynamic.Inserter
{
    internal class InsertionFactory<TEntity> where TEntity : class, new()
    {
        private readonly Entity<TEntity> _entity;

        private readonly Dictionary<InsertCacheKey, Delegate> _inserterCache;
        private readonly object _insertionLock;

        internal InsertionFactory(Entity<TEntity> entity)
        {
            _entity = entity;

            _inserterCache = new(InsertCacheKeyComparer.Default);
            _insertionLock = new();
        }

        internal Func<NpgsqlConnection, TInsert, CancellationToken, Task<int>> GetOrCreateInserter<TInsert>(RelationBuilderValues relationBuilderValues, bool isSingleInsert, bool isFullInsert) where TInsert : class
        {
            var cacheKey = new InsertCacheKey(relationBuilderValues?.GetFlattenedRelations() ?? Array.Empty<EntityRelation>(), isSingleInsert);

            if (_inserterCache.TryGetValue(cacheKey, out var tempInserter))
            {
                return (tempInserter as Func<NpgsqlConnection, TInsert, CancellationToken, Task<int>>)!;
            }

            lock (_insertionLock)
            {
                if (_inserterCache.TryGetValue(cacheKey, out tempInserter))
                {
                    return (tempInserter as Func<NpgsqlConnection, TInsert, CancellationToken, Task<int>>)!;
                }
                else
                {
                    var sourceCompiler = new InsertionSourceCompiler();

                    if (isFullInsert)
                    {
                        sourceCompiler.CompileFromRoot(_entity);
                    }
                    else
                    {
                        sourceCompiler.CompileFromRelations(_entity, relationBuilderValues);
                    }

                    var inserter = new InsertionFactoryCompiler(_entity).CreateInserter<TInsert>(sourceCompiler.GetEntities(), sourceCompiler.VisitedEntityIds, sourceCompiler.ReachableRelations);

                    _inserterCache.Add(cacheKey, inserter);

                    return inserter;
                }
            }
        }
    }
}
