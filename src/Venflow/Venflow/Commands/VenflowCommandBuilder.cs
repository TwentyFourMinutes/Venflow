using System;
using System.Collections.Generic;
using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowCommandBuilder<TEntity> : IVenflowCommandBuilder<TEntity> where TEntity : class, new()
    {
        internal RelationBuilderValues? RelationValues { get; set; }

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

        IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> IVenflowCommandBuilder<TEntity>.QuerySingle(string sql)
        {
            return new VenflowQueryCommandBuilder<TEntity, TEntity>(_database, _entityConfiguration, _command, sql, _disposeCommand, true);
        }

        IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> IVenflowCommandBuilder<TEntity>.QuerySingle(string sql, params NpgsqlParameter[] parameters)
        {
            return new VenflowQueryCommandBuilder<TEntity, TEntity>(_database, _entityConfiguration, _command, sql, parameters, _disposeCommand, true);
        }

        IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> IVenflowCommandBuilder<TEntity>.QueryInterpolatedSingle(FormattableString sql)
        {
            return new VenflowQueryCommandBuilder<TEntity, TEntity>(_database, _entityConfiguration, _command, sql, _disposeCommand, true);
        }

        IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> IVenflowCommandBuilder<TEntity>.QueryBatch(string sql)
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_database, _entityConfiguration, _command, sql, _disposeCommand, false);
        }

        IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> IVenflowCommandBuilder<TEntity>.QueryBatch(string sql, params NpgsqlParameter[] parameters)
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_database, _entityConfiguration, _command, sql, parameters, _disposeCommand, false);
        }

        IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> IVenflowCommandBuilder<TEntity>.QueryInterpolatedBatch(FormattableString sql)
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_database, _entityConfiguration, _command, sql, _disposeCommand, false);
        }

        #endregion

        #region Insert

        IBaseInsertRelationBuilder<TEntity, TEntity> IVenflowCommandBuilder<TEntity>.Insert()
        {
            return new VenflowInsertCommandBuilder<TEntity>(_database, _entityConfiguration, _command, _disposeCommand);
        }

        #endregion

        #region Delete

        IDeleteCommandBuilder<TEntity> IVenflowCommandBuilder<TEntity>.Delete()
        {
            return new VenflowDeleteCommandBuilder<TEntity>(_database, _entityConfiguration, _command, _disposeCommand);
        }

        #endregion

        #region Update

        IUpdateCommandBuilder<TEntity> IVenflowCommandBuilder<TEntity>.Update()
        {
            return new VenflowUpdateCommandBuilder<TEntity>(_database, _entityConfiguration, _command, _disposeCommand);
        }

        #endregion
    }
}