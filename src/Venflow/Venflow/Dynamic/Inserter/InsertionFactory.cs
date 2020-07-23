using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venflow.Modeling;

namespace Venflow.Dynamic.Inserter
{
    internal class InsertionFactory<TEntity> where TEntity : class
    {
        private Func<NpgsqlConnection, List<TEntity>, Task<int>>? _inserter;

        private readonly Entity<TEntity> _entity;

        internal InsertionFactory(Entity<TEntity> entity)
        {
            _entity = entity;
        }

        internal Func<NpgsqlConnection, List<TEntity>, Task<int>> GetOrCreateInserter(Database database)
        {
            if (_entity.Relations is { }) // TODO: Check for relations with no left Foreign properties
            {
                if (_inserter is null)
                {
                    var sourceCompiler = new InsertionSourceCompiler();

                    sourceCompiler.Compilev2(_entity);

                    return _inserter = new InsertionFactoryCompiler<TEntity>(_entity).CreateInserter(sourceCompiler.GenerateSortedEntities());
                }
                else
                {
                    return _inserter;
                }
            }
            else
            {
                // TODO: Create Single Inserter
                if (_inserter is null)
                {
                    var sourceCompiler = new InsertionSourceCompiler();

                    sourceCompiler.Compilev2(_entity);

                    return _inserter = new InsertionFactoryCompiler<TEntity>(_entity).CreateInserter(sourceCompiler.GenerateSortedEntities());
                }
                else
                {
                    return _inserter;
                }
            }
        }
    }
}
