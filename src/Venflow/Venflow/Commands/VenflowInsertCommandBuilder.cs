using Npgsql;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowInsertCommandBuilder<TEntity> : IInsertCommandBuilder<TEntity> where TEntity : class
    {
        private InsertOptions _insertOptions = InsertOptions.SetIdentityColumns;

        private readonly bool _disposeCommand;
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

        IInsertCommand<TEntity> ISpecficVenflowCommandBuilder<IInsertCommand<TEntity>>.Build()
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

        IInsertCommandBuilder<TEntity> IInsertCommandBuilder<TEntity>.DoNotDoNotSetPopulateRelation()
        {
            _insertOptions &= ~InsertOptions.SetIdentityColumns;

            return this;
        }

    }
}