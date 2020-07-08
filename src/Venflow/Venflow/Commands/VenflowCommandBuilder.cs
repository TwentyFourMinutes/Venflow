using Npgsql;
using System.Collections.Generic;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowCommandBuilder<TEntity> : IVenflowCommandBuilder<TEntity> where TEntity : class
    {
        internal JoinBuilderValues? JoinValues { get; set; }

        private readonly bool _disposeCommand;
        private readonly DbConfiguration _dbConfiguration;
        private readonly Entity<TEntity> _entityConfiguration;
        private readonly NpgsqlCommand _command;

        internal VenflowCommandBuilder(NpgsqlConnection dbConnection, DbConfiguration dbConfiguration, Entity<TEntity> entityConfiguration, bool disposeCommand = true)
        {
            _dbConfiguration = dbConfiguration;
            _entityConfiguration = entityConfiguration;
            _disposeCommand = disposeCommand;
            _command = new NpgsqlCommand();
            _command.Connection = dbConnection;
        }

        #region Query

        public IQueryCommandBuilder<TEntity, TEntity> QuerySingle()
        {
            return new VenflowQueryCommandBuilder<TEntity, TEntity>(_dbConfiguration, _entityConfiguration, _command, true, _disposeCommand);
        }

        public IQueryCommandBuilder<TEntity, TEntity> QuerySingle(string sql)
        {
            return new VenflowQueryCommandBuilder<TEntity, TEntity>(_dbConfiguration, _entityConfiguration, _command, sql, _disposeCommand);
        }

        public IQueryCommandBuilder<TEntity, TEntity> QuerySingle(string sql, params NpgsqlParameter[] parameters)
        {
            return new VenflowQueryCommandBuilder<TEntity, TEntity>(_dbConfiguration, _entityConfiguration, _command, sql, parameters, _disposeCommand);
        }

        public IQueryCommandBuilder<TEntity, List<TEntity>> QueryBatch()
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_dbConfiguration, _entityConfiguration, _command, true, _disposeCommand);
        }

        public IQueryCommandBuilder<TEntity, List<TEntity>> QueryBatch(ulong count)
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_dbConfiguration, _entityConfiguration, _command, count, _disposeCommand);
        }

        public IQueryCommandBuilder<TEntity, List<TEntity>> QueryBatch(string sql)
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_dbConfiguration, _entityConfiguration, _command, sql, _disposeCommand);
        }

        public IQueryCommandBuilder<TEntity, List<TEntity>> QueryBatch(string sql, params NpgsqlParameter[] parameters)
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_dbConfiguration, _entityConfiguration, _command, sql, parameters, _disposeCommand);
        }

        #endregion

        #region Insert

        public IInsertCommandBuilder<TEntity> Insert()
        {
            return new VenflowInsertCommandBuilder<TEntity>(_dbConfiguration, _entityConfiguration, _command, _disposeCommand);
        }

        #endregion

        #region Delete

        public IDeleteCommandBuilder<TEntity> Delete()
        {
            return new VenflowDeleteCommandBuilder<TEntity>(_dbConfiguration, _entityConfiguration, _command, _disposeCommand);
        }

        #endregion

        #region Update

        public IUpdateCommandBuilder<TEntity> Update()
        {
            return new VenflowUpdateCommandBuilder<TEntity>(_dbConfiguration, _entityConfiguration, _command, _disposeCommand);
        }

        #endregion
    }
}