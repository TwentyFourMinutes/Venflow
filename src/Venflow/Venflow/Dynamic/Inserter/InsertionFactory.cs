using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Dynamic.Materializer;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Dynamic.Inserter
{
    internal class InsertionFactory<TEntity> where TEntity : class, new()
    {
        private readonly Entity<TEntity> _entity;

        internal InsertionFactory(Entity<TEntity> entity)
        {
            _entity = entity;
        }

        internal Func<NpgsqlConnection, TInsert, CancellationToken, Task<int>> GetOrCreateInserter<TInsert>(InsertOptions insertOptions) where TInsert : class, new()
        {
            var sourceCompiler = new InsertionSourceCompiler();

            sourceCompiler.RootCompile(_entity);

            return new InsertionFactoryCompiler(_entity).CreateInserter<TInsert>(sourceCompiler.GetEntities(), sourceCompiler.VisitedEntityIds,sourceCompiler.ReachableRelations);
        }
    }
}
