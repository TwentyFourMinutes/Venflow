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
        private readonly object _inserstionLock;

        internal InsertionFactory(Entity<TEntity> entity)
        {
            _entity = entity;

            _inserterCache = new();
            _inserstionLock = new();
        }

        internal Func<NpgsqlConnection, TInsert, CancellationToken, Task<int>> GetOrCreateInserter<TInsert>(InsertCacheKey cacheKey, bool isFullInsert) where TInsert : class
        {
            if (_inserterCache.TryGetValue(cacheKey, out var tempInserter))
            {
                return (Func<NpgsqlConnection, TInsert, CancellationToken, Task<int>>)tempInserter;
            }

            lock (_inserstionLock)
            {
                if (_inserterCache.TryGetValue(cacheKey, out tempInserter))
                {
                    return (Func<NpgsqlConnection, TInsert, CancellationToken, Task<int>>)tempInserter;
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
                        sourceCompiler.CompileFromRelations(_entity, cacheKey.Relations);
                    }

                    var inserter = new InsertionFactoryCompiler(_entity).CreateInserter<TInsert>(sourceCompiler.GetEntities(), sourceCompiler.VisitedEntityIds, sourceCompiler.ReachableRelations);

                    _inserterCache.Add(cacheKey, inserter);

                    return inserter;
                }
            }
        }
    }
}
