using System;
using System.Collections.Generic;
using Venflow.Commands;
using Venflow.Enums;
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

        internal Delegate GetOrCreateInserter<TInsert>(RelationBuilderValues relationBuilderValues, bool shouldLog, bool isSingleInsert, bool isFullInsert) where TInsert : class
        {
            var insertOptions = InsertCacheKeyOptions.None;

            if (isSingleInsert)
                insertOptions |= InsertCacheKeyOptions.IsSingleInsert;

            if (isFullInsert)
                insertOptions |= InsertCacheKeyOptions.IsFullInsert;

            if (shouldLog)
                insertOptions |= InsertCacheKeyOptions.HasLogging;

            var cacheKey = new InsertCacheKey(!isFullInsert && relationBuilderValues is not null ? relationBuilderValues.GetFlattenedRelations() : Array.Empty<EntityRelation>(), insertOptions);

            if (_inserterCache.TryGetValue(cacheKey, out var tempInserter))
            {
                return tempInserter!;
            }

            lock (_insertionLock)
            {
                if (_inserterCache.TryGetValue(cacheKey, out tempInserter))
                {
                    return tempInserter!;
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

                    var inserter = new InsertionFactoryCompiler(_entity).CreateInserter<TInsert>(sourceCompiler.GetEntities(), sourceCompiler.VisitedEntityIds, sourceCompiler.ReachableRelations, shouldLog);

                    _inserterCache.Add(cacheKey, inserter);

                    return inserter;
                }
            }
        }
    }
}
