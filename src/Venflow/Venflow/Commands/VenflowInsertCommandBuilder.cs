using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Dynamic;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowInsertCommandBuilder<TEntity> : IInsertCommandBuilder<TEntity> where TEntity : class, new()
    {
        private InsertOptions _insertOptions = InsertOptions.SetIdentityColumns;
        private bool _disposeCommand;

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
            return new VenflowInsertCommand<TEntity>(_database, _entityConfiguration, _command, _insertOptions, _disposeCommand);
        }

        IInsertCommandBuilder<TEntity> IInsertCommandBuilder<TEntity>.SetIdentityColumns()
        {
            _insertOptions |= InsertOptions.SetIdentityColumns;

            return this;
        }

        IInsertCommandBuilder<TEntity> IInsertCommandBuilder<TEntity>.PopulateRelation()
        {
            _insertOptions |= InsertOptions.PopulateRelations;

            return this;
        }

        IInsertCommandBuilder<TEntity> IInsertCommandBuilder<TEntity>.DoNotSetIdentityColumns()
        {
            _insertOptions &= ~InsertOptions.PopulateRelations;

            return this;
        }

        IInsertCommandBuilder<TEntity> IInsertCommandBuilder<TEntity>.DoNotSetPopulateRelation()
        {
            _insertOptions &= ~InsertOptions.SetIdentityColumns;

            return this;
        }

        IInsertRelationBuilder<TToEntity, TEntity> IInsertCommandBuilder<TEntity>.InsertWith<TToEntity>(Expression<System.Func<TEntity, TToEntity>> propertySelector)
        {
            var relationBuilder = new RelationBuilder<TToEntity, TEntity>(_entityConfiguration);

            _relationBuilderValues = relationBuilder.RelationBuilderValues;

            return relationBuilder.InsertWith(propertySelector);
        }

        IInsertRelationBuilder<TToEntity, TEntity> IInsertCommandBuilder<TEntity>.InsertWith<TToEntity>(Expression<System.Func<TEntity, IList<TToEntity>>> propertySelector)
        {
            var relationBuilder = new RelationBuilder<TToEntity, TEntity>(_entityConfiguration);

            _relationBuilderValues = relationBuilder.RelationBuilderValues;

            return relationBuilder.InsertWith(propertySelector);
        }

        IInsertRelationBuilder<TToEntity, TEntity> IInsertCommandBuilder<TEntity>.InsertWith<TToEntity>(Expression<System.Func<TEntity, List<TToEntity>>> propertySelector)
        {
            var relationBuilder = new RelationBuilder<TToEntity, TEntity>(_entityConfiguration);

            _relationBuilderValues = relationBuilder.RelationBuilderValues;

            return relationBuilder.InsertWith(propertySelector);
        }

        Task<int> IInsertCommandBuilder<TEntity>.InsertAsync(TEntity entity, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().InsertAsync(entity, cancellationToken);
        }

        Task<int> IInsertCommandBuilder<TEntity>.InsertAsync(IList<TEntity> entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().InsertAsync(entities, cancellationToken);
        }
    }
}