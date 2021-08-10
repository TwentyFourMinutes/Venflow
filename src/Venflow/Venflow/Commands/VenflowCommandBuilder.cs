using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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

        internal IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingleBase(LambdaExpression sql)
        {
            return new VenflowQueryCommandBuilder<TEntity, TEntity>(_database, _entityConfiguration, sql, _disposeCommand, true);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle(Expression<Func<TEntity, FormattableString>> sql)
        {
            return QuerySingleBase(sql);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne>(Expression<Func<TEntity, TOne, FormattableString>> sql)
            where TOne : class, new()
        {
            return QuerySingleBase(sql);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, TTwo>(Expression<Func<TEntity, TOne, TTwo, FormattableString>> sql)
            where TOne : class, new()
            where TTwo : class, new()
        {
            return QuerySingleBase(sql);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, TTwo, TThree>(Expression<Func<TEntity, TOne, TTwo, TThree, FormattableString>> sql)
            where TOne : class, new()
            where TTwo : class, new()
            where TThree : class, new()
        {
            return QuerySingleBase(sql);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, TTwo, TThree, TFour>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, FormattableString>> sql)
            where TOne : class, new()
            where TTwo : class, new()
            where TThree : class, new()
            where TFour : class, new()
        {
            return QuerySingleBase(sql);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, TTwo, TThree, TFour, TFive>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, TFive, FormattableString>> sql)
            where TOne : class, new()
            where TTwo : class, new()
            where TThree : class, new()
            where TFour : class, new()
            where TFive : class, new()
        {
            return QuerySingleBase(sql);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, TTwo, TThree, TFour, TFive, TSix>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, TFive, TSix, FormattableString>> sql)
            where TOne : class, new()
            where TTwo : class, new()
            where TThree : class, new()
            where TFour : class, new()
            where TFive : class, new()
            where TSix : class, new()
        {
            return QuerySingleBase(sql);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, TTwo, TThree, TFour, TFive, TSix, TSeven>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, TFive, TSix, TSeven, FormattableString>> sql)
            where TOne : class, new()
            where TTwo : class, new()
            where TThree : class, new()
            where TFour : class, new()
            where TFive : class, new()
            where TSix : class, new()
            where TSeven : class, new()
        {
            return QuerySingleBase(sql);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, TTwo, TThree, TFour, TFive, TSix, TSeven, TEight>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, TFive, TSix, TSeven, TEight, FormattableString>> sql)
            where TOne : class, new()
            where TTwo : class, new()
            where TThree : class, new()
            where TFour : class, new()
            where TFive : class, new()
            where TSix : class, new()
            where TSeven : class, new()
            where TEight : class, new()
        {
            return QuerySingleBase(sql);
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

        internal IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatchBase(LambdaExpression sql)
        {
            return new VenflowQueryCommandBuilder<TEntity, List<TEntity>>(_database, _entityConfiguration, sql, _disposeCommand, false);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch(Expression<Func<TEntity, FormattableString>> sql)
        {
            return QueryBatchBase(sql);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne>(Expression<Func<TEntity, TOne, FormattableString>> sql)
            where TOne : class, new()
        {
            return QueryBatchBase(sql);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, TTwo>(Expression<Func<TEntity, TOne, TTwo, FormattableString>> sql)
            where TOne : class, new()
            where TTwo : class, new()
        {
            return QueryBatchBase(sql);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, TTwo, TThree>(Expression<Func<TEntity, TOne, TTwo, TThree, FormattableString>> sql)
            where TOne : class, new()
            where TTwo : class, new()
            where TThree : class, new()
        {
            return QueryBatchBase(sql);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, TTwo, TThree, TFour>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, FormattableString>> sql)
            where TOne : class, new()
            where TTwo : class, new()
            where TThree : class, new()
            where TFour : class, new()
        {
            return QueryBatchBase(sql);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, TTwo, TThree, TFour, TFive>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, TFive, FormattableString>> sql)
            where TOne : class, new()
            where TTwo : class, new()
            where TThree : class, new()
            where TFour : class, new()
            where TFive : class, new()
        {
            return QueryBatchBase(sql);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, TTwo, TThree, TFour, TFive, TSix>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, TFive, TSix, FormattableString>> sql)
            where TOne : class, new()
            where TTwo : class, new()
            where TThree : class, new()
            where TFour : class, new()
            where TFive : class, new()
            where TSix : class, new()
        {
            return QueryBatchBase(sql);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, TTwo, TThree, TFour, TFive, TSix, TSeven>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, TFive, TSix, TSeven, FormattableString>> sql)
            where TOne : class, new()
            where TTwo : class, new()
            where TThree : class, new()
            where TFour : class, new()
            where TFive : class, new()
            where TSix : class, new()
            where TSeven : class, new()
        {
            return QueryBatchBase(sql);
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, TTwo, TThree, TFour, TFive, TSix, TSeven, TEight>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, TFive, TSix, TSeven, TEight, FormattableString>> sql)
            where TOne : class, new()
            where TTwo : class, new()
            where TThree : class, new()
            where TFour : class, new()
            where TFive : class, new()
            where TSix : class, new()
            where TSeven : class, new()
            where TEight : class, new()
        {
            return QueryBatchBase(sql);
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