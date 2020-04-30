using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow
{
    internal class ColumnMapper<TEntity> where TEntity : class
    {
        private readonly VenflowCommand<TEntity> _command;

        internal ColumnMapper(VenflowCommand<TEntity> command)
        {
            _command = command;
        }

        internal async Task<List<TEntity>> GetEntitiesAsync(NpgsqlDataReader dataReader, CancellationToken cancellationToken)
        {
            var entities = new List<TEntity>();

            if (!await dataReader.ReadAsync(cancellationToken))
            {
                return entities;
            }

            if (_command.EntityFactory is null)
            {
                var columnSchemas = dataReader.GetColumnSchema();

                var dataReaderParameter = Expression.Parameter(typeof(NpgsqlDataReader), "dataReader");

                var bindings = new MemberBinding[columnSchemas.Count];

                for (int i = 0; i < columnSchemas.Count; i++)
                {
                    var columnSchema = columnSchemas[i];

                    var column = _command.EntityConfiguration.Columns[columnSchema.ColumnName];

                    Expression valueGetter = Expression.Call(dataReaderParameter, column.DbValueRetriever, Expression.Constant(columnSchema.ColumnOrdinal));

                    bindings[i] = Expression.Bind(column.PropertyInfo, valueGetter);
                }

                _command.EntityFactory = Expression.Lambda<Func<NpgsqlDataReader, TEntity>>(Expression.MemberInit(Expression.New(_command.EntityConfiguration.EntityType), bindings), dataReaderParameter).Compile();
            }

            var factory = _command.EntityFactory;

            entities.Add(factory(dataReader));

            while (await dataReader.ReadAsync(cancellationToken))
            {
                entities.Add(factory(dataReader));
            }

            return entities;
        }
    }
}
