using System.Linq.Expressions;
using Npgsql;

namespace Venflow.Commands
{
    /// <summary>
    /// Represents a generic command builder to create any CRUD command.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity which will be used for the command.</typeparam>
    public interface IVenflowCommandBuilder<TEntity> where TEntity : class, new()
    {
        /// <summary>
        /// Creates a query command with a single result. <strong>This API does not support string interpolation!</strong> If you need to pass parameters with the query, either use <see cref="QuerySingle(string, NpgsqlParameter[])"/> or <see cref="QueryInterpolatedSingle(FormattableString)"/>.
        /// </summary>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle(string sql);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle(Expression<Func<TEntity, FormattableString>> sql);
        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne>(Expression<Func<TEntity, TOne, FormattableString>> sql) where TOne : class, new();
        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, TTwo>(Expression<Func<TEntity, TOne, TTwo, FormattableString>> sql) where TOne : class, new() where TTwo : class, new();
        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, TTwo, TThree>(Expression<Func<TEntity, TOne, TTwo, TThree, FormattableString>> sql) where TOne : class, new() where TTwo : class, new() where TThree : class, new();
        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, TTwo, TThree, TFour>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, FormattableString>> sql) where TOne : class, new() where TTwo : class, new() where TThree : class, new() where TFour : class, new();
        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, TTwo, TThree, TFour, TFive>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, TFive, FormattableString>> sql) where TOne : class, new() where TTwo : class, new() where TThree : class, new() where TFour : class, new() where TFive : class, new();
        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, TTwo, TThree, TFour, TFive, TSix>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, TFive, TSix, FormattableString>> sql) where TOne : class, new() where TTwo : class, new() where TThree : class, new() where TFour : class, new() where TFive : class, new() where TSix : class, new();
        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, TTwo, TThree, TFour, TFive, TSix, TSeven>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, TFive, TSix, TSeven, FormattableString>> sql) where TOne : class, new() where TTwo : class, new() where TThree : class, new() where TFour : class, new() where TFive : class, new() where TSix : class, new() where TSeven : class, new();
        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle<TOne, TTwo, TThree, TFour, TFive, TSix, TSeven, TEight>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, TFive, TSix, TSeven, TEight, FormattableString>> sql) where TOne : class, new() where TTwo : class, new() where TThree : class, new() where TFour : class, new() where TFive : class, new() where TSix : class, new() where TSeven : class, new() where TEight : class, new();

        /// <summary>
        /// Creates a new query command builder, which expects a single returned primary row. <strong>This API does not support string interpolation!</strong> If you want to pass interpolated SQL use <see cref="QueryInterpolatedSingle(FormattableString)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="parameters">A set of <see cref="NpgsqlParameter"/> which contain parameters for the <paramref name="sql"/> command.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QuerySingle(string sql, params NpgsqlParameter[] parameters);
        /// <summary>
        /// Creates a new query command builder, which expects a single returned primary row. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter. You should only add parameters trough string interpolation.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, TEntity> QueryInterpolatedSingle(FormattableString sql);
        /// <summary>
        /// Creates a new query command builder, which expects a set of primary rows to be returned. <strong>This API does not support string interpolation!</strong> If you need to pass parameters with the query, either use <see cref="QuerySingle(string, NpgsqlParameter[])"/> or <see cref="QueryInterpolatedSingle(FormattableString)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch(string sql);

        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch(Expression<Func<TEntity, FormattableString>> sql);
        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne>(Expression<Func<TEntity, TOne, FormattableString>> sql) where TOne : class, new();
        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, TTwo>(Expression<Func<TEntity, TOne, TTwo, FormattableString>> sql) where TOne : class, new() where TTwo : class, new();
        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, TTwo, TThree>(Expression<Func<TEntity, TOne, TTwo, TThree, FormattableString>> sql) where TOne : class, new() where TTwo : class, new() where TThree : class, new();
        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, TTwo, TThree, TFour>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, FormattableString>> sql) where TOne : class, new() where TTwo : class, new() where TThree : class, new() where TFour : class, new();
        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, TTwo, TThree, TFour, TFive>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, TFive, FormattableString>> sql) where TOne : class, new() where TTwo : class, new() where TThree : class, new() where TFour : class, new() where TFive : class, new();
        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, TTwo, TThree, TFour, TFive, TSix>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, TFive, TSix, FormattableString>> sql) where TOne : class, new() where TTwo : class, new() where TThree : class, new() where TFour : class, new() where TFive : class, new() where TSix : class, new();
        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, TTwo, TThree, TFour, TFive, TSix, TSeven>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, TFive, TSix, TSeven, FormattableString>> sql) where TOne : class, new() where TTwo : class, new() where TThree : class, new() where TFour : class, new() where TFive : class, new() where TSix : class, new() where TSeven : class, new();
        /// <summary>
        ///  Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch<TOne, TTwo, TThree, TFour, TFive, TSix, TSeven, TEight>(Expression<Func<TEntity, TOne, TTwo, TThree, TFour, TFive, TSix, TSeven, TEight, FormattableString>> sql) where TOne : class, new() where TTwo : class, new() where TThree : class, new() where TFour : class, new() where TFive : class, new() where TSix : class, new() where TSeven : class, new() where TEight : class, new();

        /// <summary>
        /// Creates a new query command builder, which expects a set of primary rows to be returned.<strong>This API does not support string interpolation!</strong> If you want to pass interpolated SQL use <see cref="QueryInterpolatedSingle(FormattableString)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="parameters">A set of <see cref="NpgsqlParameter"/> which contain parameters for the <paramref name="sql"/> command.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryBatch(string sql, params NpgsqlParameter[] parameters);
        /// <summary>
        /// Creates a new query command builder, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter. You should only add parameters trough string interpolation.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, List<TEntity>> QueryInterpolatedBatch(FormattableString sql);

        /// <summary>
        /// Creates a new insert command builder.
        /// </summary>
        /// <returns>A Fluent API Builder for a insert command.</returns>
        IBaseInsertRelationBuilder<TEntity, TEntity> Insert();
        /// <summary>
        /// Creates a new delete command builder.
        /// </summary>
        /// <returns>A Fluent API Builder for a delete command.</returns>
        IDeleteCommandBuilder<TEntity> Delete();

        /// <summary>
        /// Creates a new update command builder.
        /// </summary>
        /// <returns>A Fluent API Builder for a update command.</returns>
        IUpdateCommandBuilder<TEntity> Update();
    }
}
