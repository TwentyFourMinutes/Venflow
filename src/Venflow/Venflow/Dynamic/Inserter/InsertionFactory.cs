using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Dynamic.Inserter
{
    internal class InsertionFactory<TEntity> where TEntity : class, new()
    {
        private readonly Entity<TEntity> _entity;

        private readonly Dictionary<Type, Delegate> _inserterCache;
        private readonly object _inserstionLock;

        internal InsertionFactory(Entity<TEntity> entity)
        {
            _entity = entity;

            _inserterCache = new();
            _inserstionLock = new();
        }

        internal Func<NpgsqlConnection, TInsert, CancellationToken, Task<int>> GetOrCreateInserter<TInsert>(InsertOptions insertOptions) where TInsert : class
        {
            lock (_inserstionLock)
            {
                if (_inserterCache.TryGetValue(typeof(TInsert), out var tempInserter))
                {
                    return (Func<NpgsqlConnection, TInsert, CancellationToken, Task<int>>)tempInserter;
                }
                else
                {
                    var sourceCompiler = new InsertionSourceCompiler();

                    sourceCompiler.RootCompile(_entity);

                    var inserter = new InsertionFactoryCompiler(_entity).CreateInserter<TInsert>(sourceCompiler.GetEntities(), sourceCompiler.VisitedEntityIds, sourceCompiler.ReachableRelations);

                    _inserterCache.Add(typeof(TInsert), inserter);

                    return inserter;
                }
            }
        }
    }
}
