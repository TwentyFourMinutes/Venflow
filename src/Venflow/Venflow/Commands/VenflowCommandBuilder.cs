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

        internal VenflowCommandBuilder(Database database, Entity<TEntity> entityConfiguration, bool disposeCommand = true)
        {
            _database = database;
            _entityConfiguration = entityConfiguration;
            _disposeCommand = disposeCommand;
        }

        #region Query

        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle(string sql)
        {
            return new VenflowQueryCommandBuilder<TEntity, TEntity>(_database, _entityConfiguration, sql, _disposeCommand, true);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle(string sql, params NpgsqlParameter[] parameters)
        {
            return new VenflowQueryCommandBuilder<TEntity, TEntity>(_database, _entityConfiguration, sql, parameters, _disposeCommand, true);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle(string sql, IList<NpgsqlParameter> parameters)
        {
            return new VenflowQueryCommandBuilder<TEntity, TEntity>(_database, _entityConfiguration, sql, parameters, _disposeCommand, true);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QueryInterpolatedSingle(FormattableString sql)
        {
            return new VenflowQueryCommandBuilder<TEntity, TEntity>(_database, _entityConfiguration, sql, _disposeCommand, true);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch(string sql)
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_database, _entityConfiguration, sql, _disposeCommand, false);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch(string sql, params NpgsqlParameter[] parameters)
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_database, _entityConfiguration, sql, parameters, _disposeCommand, false);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch(string sql, IList<NpgsqlParameter> parameters)
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_database, _entityConfiguration, sql, parameters, _disposeCommand, false);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryInterpolatedBatch(FormattableString sql)
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_database, _entityConfiguration, sql, _disposeCommand, false);
        }

        #endregion

        #region Insert

        public IBaseInsertRelationBuilder<TEntity, TEntity> Insert()
        {
            return new VenflowInsertCommandBuilder<TEntity>(_database, _entityConfiguration);
        }

        #endregion

        #region Delete

        public IDeleteCommandBuilder<TEntity> Delete()
        {
            return new VenflowDeleteCommandBuilder<TEntity>(_database, _entityConfiguration, _disposeCommand);
        }

        #endregion

        #region Update

        public IUpdateCommandBuilder<TEntity> Update()
        {
            return new VenflowUpdateCommandBuilder<TEntity>(_database, _entityConfiguration, _disposeCommand);
        }

        #endregion
    }
}