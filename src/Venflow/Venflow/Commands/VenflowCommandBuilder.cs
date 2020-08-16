using Npgsql;
using System;
using System.Collections.Generic;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowCommandBuilder<TEntity> : IVenflowCommandBuilder<TEntity> where TEntity : class, new()
    {
        internal JoinBuilderValues? JoinValues { get; set; }

        private readonly bool _disposeCommand;
        private readonly Database _database;
        private readonly Entity<TEntity> _entityConfiguration;
        private readonly NpgsqlCommand _command;

        internal VenflowCommandBuilder(NpgsqlConnection dbConnection, Database database, Entity<TEntity> entityConfiguration, bool disposeCommand = true)
        {
            _database = database;
            _entityConfiguration = entityConfiguration;
            _disposeCommand = disposeCommand;
            _command = new NpgsqlCommand();
            _command.Connection = dbConnection;
        }

        #region Query

        public IPreCommandBuilder<TEntity, TEntity> QuerySingle()
        {
            return new VenflowQueryCommandBuilder<TEntity, TEntity>(_database, _entityConfiguration, _command, QueryGenerationOptions.GenerateFullSQL, _disposeCommand,true);
        }

        public IPreCommandBuilder<TEntity, TEntity> QuerySingle(string sql)
        {
            return new VenflowQueryCommandBuilder<TEntity, TEntity>(_database, _entityConfiguration, _command, sql, _disposeCommand,true);
        }

        public IPreCommandBuilder<TEntity, TEntity> QuerySingle(string sql, params NpgsqlParameter[] parameters)
        {
            return new VenflowQueryCommandBuilder<TEntity, TEntity>(_database, _entityConfiguration, _command, sql, parameters, _disposeCommand,true);
        }

        public IPreCommandBuilder<TEntity, TEntity> QueryInterpolatedSingle(FormattableString sql)
        {
            return new VenflowQueryCommandBuilder<TEntity, TEntity>(_database, _entityConfiguration, _command, sql, _disposeCommand,true);
        }

        public IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch()
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_database, _entityConfiguration, _command, QueryGenerationOptions.GenerateFullSQL, _disposeCommand,false);
        }

        public IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch(ulong count)
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_database, _entityConfiguration, _command, count, _disposeCommand,false);
        }

        public IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch(string sql)
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_database, _entityConfiguration, _command, sql, _disposeCommand,false);
        }

        public IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch(string sql, params NpgsqlParameter[] parameters)
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_database, _entityConfiguration, _command, sql, parameters, _disposeCommand,false);
        }

        public IPreCommandBuilder<TEntity, List<TEntity>> QueryInterpolatedBatch(FormattableString sql)
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_database, _entityConfiguration, _command, sql, _disposeCommand,false);
        }

        #endregion

        #region Insert

        public IInsertCommandBuilder<TEntity> Insert()
        {
            return new VenflowInsertCommandBuilder<TEntity>(_database, _entityConfiguration, _command, _disposeCommand);
        }

        #endregion

        #region Delete

        public IDeleteCommandBuilder<TEntity> Delete()
        {
            return new VenflowDeleteCommandBuilder<TEntity>(_database, _entityConfiguration, _command, _disposeCommand);
        }

        #endregion

        #region Update

        public IUpdateCommandBuilder<TEntity> Update()
        {
            return new VenflowUpdateCommandBuilder<TEntity>(_database, _entityConfiguration, _command, _disposeCommand);
        }

        #endregion
    }
}