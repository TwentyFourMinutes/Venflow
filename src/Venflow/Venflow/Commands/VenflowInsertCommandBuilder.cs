using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowInsertCommandBuilder<TEntity> : IBaseInsertRelationBuilder<TEntity, TEntity>
        where TEntity : class, new()
    {
        private RelationBuilderValues? _relationBuilderValues;
        private bool _disposeCommand;
        private bool _isFullInsert;

        private readonly NpgsqlCommand _command;
        private readonly Database _database;
        private readonly Entity<TEntity> _entityConfiguration;

        internal VenflowInsertCommandBuilder(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand command, bool disposeCommand)
        {
            _database = database;
            _entityConfiguration = entityConfiguration;
            _command = command;
            _disposeCommand = disposeCommand;
        }

        public IInsertCommand<TEntity> Build()
        {
            if (_relationBuilderValues is { })
            {
                return new VenflowInsertCommand<TEntity>(_database, _entityConfiguration, _command, _disposeCommand, _relationBuilderValues, _isFullInsert);
            }

            return new VenflowInsertCommand<TEntity>(_database, _entityConfiguration, _command, _disposeCommand, _isFullInsert);
        }

        public Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _disposeCommand = true;

            return Build().InsertAsync(entity, cancellationToken);
        }

        public Task<int> InsertAsync(IList<TEntity> entities, CancellationToken cancellationToken = default)
        {
            _disposeCommand = true;

            return Build().InsertAsync(entities, cancellationToken);
        }

        public IBaseInsertRelationBuilder<TEntity, TEntity> WithAll()
        {
            _isFullInsert = true;

            return this;
        }

        IInsertRelationBuilder<TToEntity, TEntity> IBaseInsertRelationBuilder<TEntity, TEntity>.With<TToEntity>(Expression<Func<TEntity, TToEntity>> propertySelector)
        {
            _relationBuilderValues = new RelationBuilderValues(_entityConfiguration);

            return new InsertRelationBuilder<TEntity, TEntity>(_entityConfiguration, _entityConfiguration, this, _relationBuilderValues).With(propertySelector);
        }

        IInsertRelationBuilder<TToEntity, TEntity> IBaseInsertRelationBuilder<TEntity, TEntity>.With<TToEntity>(Expression<Func<TEntity, IList<TToEntity>>> propertySelector)
        {
            _relationBuilderValues = new RelationBuilderValues(_entityConfiguration);

            return new InsertRelationBuilder<TEntity, TEntity>(_entityConfiguration, _entityConfiguration, this, _relationBuilderValues).With(propertySelector);
        }

        IInsertRelationBuilder<TToEntity, TEntity> IBaseInsertRelationBuilder<TEntity, TEntity>.With<TToEntity>(Expression<Func<TEntity, List<TToEntity>>> propertySelector)
        {
            _relationBuilderValues = new RelationBuilderValues(_entityConfiguration);

            return new InsertRelationBuilder<TEntity, TEntity>(_entityConfiguration, _entityConfiguration, this, _relationBuilderValues).With(propertySelector);
        }
    }
}