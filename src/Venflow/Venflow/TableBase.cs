using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Commands;
using Venflow.Modeling;

namespace Venflow
{
    /// <summary>
    /// A <see cref="TableBase{TEntity}"/> is used to perform query operations of all sorts.
    /// </summary>
    /// <typeparam name="TEntity">The entity which represents a table in the Database.</typeparam>
    public class TableBase<TEntity> where TEntity : class, new()
    {
        private protected Database Database { get; }
        private protected Entity<TEntity> Configuration { get; }

        internal TableBase(Database database, Entity<TEntity> configuration)
        {
            Database = database;
            Configuration = configuration;
        }

        #region QueryAsync

        /// <summary>
        /// Asynchronously queries one or more entities with the configured joins.
        /// </summary>
        /// <param name="queryCommand">A <see cref="IQueryCommand{TEntity, TReturn}"/> instance representing the query which will be performed.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the result of the executed query.</returns>
        /// <remarks>This method could represents the following SQL statement "SELECT * FROM table".</remarks>
        public Task<TReturn?> QueryAsync<TReturn>(IQueryCommand<TEntity, TReturn> queryCommand, CancellationToken cancellationToken = default) where TReturn : class, new()
        {
            var command = (VenflowBaseCommand<TEntity>)queryCommand;

            command.Database = Database;
            command.UnderlyingCommand.Connection = Database.GetConnection();

            return queryCommand.QueryAsync(cancellationToken);
        }

        #endregion

        #region Builder

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does not support string interpolation!</strong> If you need to pass parameters with the query, either use <see cref="QuerySingle(string, NpgsqlParameter[])"/> or <see cref="QueryInterpolatedSingle(FormattableString, bool)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle(string sql, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QuerySingle(sql);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle(Expression<Func<TEntity, FormattableString>> sql, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QuerySingleBase(sql);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne>(Expression<Func<TEntity, TOne, FormattableString>> sql, bool disposeCommand = true) where TOne : class, new()
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QuerySingleBase(sql);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, Two>(Expression<Func<TEntity, TOne, Two, FormattableString>> sql, bool disposeCommand = true) where TOne : class, new() where Two : class, new()
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QuerySingleBase(sql);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, Two, TThree>(Expression<Func<TEntity, TOne, Two, TThree, FormattableString>> sql, bool disposeCommand = true) where TOne : class, new() where Two : class, new() where TThree : class, new()
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QuerySingleBase(sql);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, Two, TThree, TFour>(Expression<Func<TEntity, TOne, Two, TThree, TFour, FormattableString>> sql, bool disposeCommand = true) where TOne : class, new() where Two : class, new() where TThree : class, new() where TFour : class, new()
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QuerySingleBase(sql);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, Two, TThree, TFour, TFive>(Expression<Func<TEntity, TOne, Two, TThree, TFour, TFive, FormattableString>> sql, bool disposeCommand = true) where TOne : class, new() where Two : class, new() where TThree : class, new() where TFour : class, new() where TFive : class, new()
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QuerySingleBase(sql);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, Two, TThree, TFour, TFive, TSix>(Expression<Func<TEntity, TOne, Two, TThree, TFour, TFive, TSix, FormattableString>> sql, bool disposeCommand = true) where TOne : class, new() where Two : class, new() where TThree : class, new() where TFour : class, new() where TFive : class, new() where TSix : class, new()
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QuerySingleBase(sql);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, Two, TThree, TFour, TFive, TSix, TSeven>(Expression<Func<TEntity, TOne, Two, TThree, TFour, TFive, TSix, TSeven, FormattableString>> sql, bool disposeCommand = true) where TOne : class, new() where Two : class, new() where TThree : class, new() where TFour : class, new() where TFive : class, new() where TSix : class, new() where TSeven : class, new()
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QuerySingleBase(sql);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, Two, TThree, TFour, TFive, TSix, TSeven, TEight>(Expression<Func<TEntity, TOne, Two, TThree, TFour, TFive, TSix, TSeven, TEight, FormattableString>> sql, bool disposeCommand = true) where TOne : class, new() where Two : class, new() where TThree : class, new() where TFour : class, new() where TFive : class, new() where TSix : class, new() where TSeven : class, new() where TEight : class, new()
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QuerySingleBase(sql);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does not support string interpolation!</strong> If you want to pass interpolated SQL use <see cref="QueryInterpolatedSingle(FormattableString, bool)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="parameters">A set of <see cref="NpgsqlParameter"/> which contain parameters for the <paramref name="sql"/> command.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        /// <remarks>The command will be automatically disposed the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</remarks>
        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle(string sql, params NpgsqlParameter[] parameters)
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, true).QuerySingle(sql, parameters);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does not support string interpolation!</strong> If you want to pass interpolated SQL use <see cref="QueryInterpolatedSingle(FormattableString, bool)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="parameters">A set of <see cref="NpgsqlParameter"/> which contain parameters for the <paramref name="sql"/> command.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        /// <remarks>The command will be automatically disposed the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</remarks>
        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle(string sql, IList<NpgsqlParameter> parameters)
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, true).QuerySingle(sql, parameters);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does not support string interpolation!</strong> If you want to pass interpolated SQL use <see cref="QueryInterpolatedSingle(FormattableString, bool)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <param name="parameters">A set of <see cref="NpgsqlParameter"/> which contain parameters for the <paramref name="sql"/> command.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle(string sql, bool disposeCommand, params NpgsqlParameter[] parameters)
           => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QuerySingle(sql, parameters);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter. You should only add parameters trough string interpolation.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QueryInterpolatedSingle(FormattableString sql, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QueryInterpolatedSingle(sql);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A <see cref="FormattableSqlStringBuilder"/> instance containing the SQL statement.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QueryInterpolatedSingle(FormattableSqlStringBuilder sql, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QuerySingle(sql.Build(), sql.Parameters);

        /// <summary>
        /// Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does not support string interpolation!</strong> If you need to pass parameters with the query, either use <see cref="QuerySingle(string, NpgsqlParameter[])"/> or <see cref="QueryInterpolatedSingle(FormattableString, bool)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch(string sql, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QueryBatch(sql);

        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch(Expression<Func<TEntity, FormattableString>> sql, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QueryBatchBase(sql);

        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne>(Expression<Func<TEntity, TOne, FormattableString>> sql, bool disposeCommand = true) where TOne : class, new()
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QueryBatchBase(sql);

        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, Two>(Expression<Func<TEntity, TOne, Two, FormattableString>> sql, bool disposeCommand = true) where TOne : class, new() where Two : class, new()
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QueryBatchBase(sql);

        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, Two, TThree>(Expression<Func<TEntity, TOne, Two, TThree, FormattableString>> sql, bool disposeCommand = true) where TOne : class, new() where Two : class, new() where TThree : class, new()
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QueryBatchBase(sql);

        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, Two, TThree, TFour>(Expression<Func<TEntity, TOne, Two, TThree, TFour, FormattableString>> sql, bool disposeCommand = true) where TOne : class, new() where Two : class, new() where TThree : class, new() where TFour : class, new()
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QueryBatchBase(sql);

        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, Two, TThree, TFour, TFive>(Expression<Func<TEntity, TOne, Two, TThree, TFour, TFive, FormattableString>> sql, bool disposeCommand = true) where TOne : class, new() where Two : class, new() where TThree : class, new() where TFour : class, new() where TFive : class, new()
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QueryBatchBase(sql);

        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, Two, TThree, TFour, TFive, TSix>(Expression<Func<TEntity, TOne, Two, TThree, TFour, TFive, TSix, FormattableString>> sql, bool disposeCommand = true) where TOne : class, new() where Two : class, new() where TThree : class, new() where TFour : class, new() where TFive : class, new() where TSix : class, new()
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QueryBatchBase(sql);

        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, Two, TThree, TFour, TFive, TSix, TSeven>(Expression<Func<TEntity, TOne, Two, TThree, TFour, TFive, TSix, TSeven, FormattableString>> sql, bool disposeCommand = true) where TOne : class, new() where Two : class, new() where TThree : class, new() where TFour : class, new() where TFive : class, new() where TSix : class, new() where TSeven : class, new()
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QueryBatchBase(sql);

        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, Two, TThree, TFour, TFive, TSix, TSeven, TEight>(Expression<Func<TEntity, TOne, Two, TThree, TFour, TFive, TSix, TSeven, TEight, FormattableString>> sql, bool disposeCommand = true) where TOne : class, new() where Two : class, new() where TThree : class, new() where TFour : class, new() where TFive : class, new() where TSix : class, new() where TSeven : class, new() where TEight : class, new()
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QueryBatchBase(sql);

        /// <summary>
        /// Creates a new query command, which expects a set of primary rows to be returned.<strong>This API does not support string interpolation!</strong> If you want to pass interpolated SQL use <see cref="QueryInterpolatedSingle(FormattableString, bool)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="parameters">A set of <see cref="NpgsqlParameter"/> which contain parameters for the <paramref name="sql"/> command.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        /// <remarks>The command will be automatically disposed the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</remarks>
        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch(string sql, params NpgsqlParameter[] parameters)
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, false).QueryBatch(sql, parameters);

        /// <summary>
        /// Creates a new query command, which expects a set of primary rows to be returned.<strong>This API does not support string interpolation!</strong> If you want to pass interpolated SQL use <see cref="QueryInterpolatedSingle(FormattableString, bool)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="parameters">A set of <see cref="NpgsqlParameter"/> which contain parameters for the <paramref name="sql"/> command.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        /// <remarks>The command will be automatically disposed the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</remarks>
        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch(string sql, IList<NpgsqlParameter> parameters)
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, false).QueryBatch(sql, parameters);

        /// <summary>
        /// Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does not support string interpolation!</strong> If you want to pass interpolated SQL use <see cref="QueryInterpolatedSingle(FormattableString, bool)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <param name="parameters">A set of <see cref="NpgsqlParameter"/> which contain parameters for the <paramref name="sql"/> command.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch(string sql, bool disposeCommand, params NpgsqlParameter[] parameters)
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QueryBatch(sql, parameters);

        /// <summary>
        /// Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter. <strong>You should only add parameters trough string interpolation.</strong></param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryInterpolatedBatch(FormattableString sql, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QueryInterpolatedBatch(sql);

        /// <summary>
        /// Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A <see cref="FormattableSqlStringBuilder"/> instance containing the SQL statement.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryInterpolatedBatch(FormattableSqlStringBuilder sql, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(Database, Configuration, disposeCommand).QueryBatch(sql.Build(), sql.Parameters);

        #endregion

        private protected ValueTask ValidateConnectionAsync()
        {
            var connection = Database.GetConnection();

            if (connection.State == ConnectionState.Open)
                return default;

            if (connection.State == ConnectionState.Closed)
            {
                return new ValueTask(connection.OpenAsync());
            }
            else
            {
                throw new InvalidOperationException($"The current connection state is invalid. Expected: '{ConnectionState.Open}' or '{ConnectionState.Closed}'. Actual: '{connection.State}'.");
            }
        }
    }
}
