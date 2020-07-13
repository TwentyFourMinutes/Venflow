using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowInsertCommandBuilder<TEntity> : IInsertCommandBuilder<TEntity> where TEntity : class
    {
        private bool _returnComputedColumns;

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
            return new VenflowInsertCommand<TEntity>(_database, _entityConfiguration, _command, _returnComputedColumns, _disposeCommand);
        }

        IInsertCommandBuilder<TEntity> IInsertCommandBuilder<TEntity>.ReturnComputedColumns(bool returnComputedColumns)
        {
            if (returnComputedColumns)
            {
                _returnComputedColumns = _entityConfiguration.PrimaryColumn.IsServerSideGenerated;
            }

            return this;
        }
    }
}