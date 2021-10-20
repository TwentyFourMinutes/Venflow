namespace Venflow.Commands
{
    /// <summary>
    /// Represents a pre-command builder to configure the query.
    /// </summary>s
    /// <typeparam name="TEntity">The type of the entity which will be queried.</typeparam>
    /// <typeparam name="TReturn">The return type of the query.</typeparam>
    public interface IPreCommandBuilder<TEntity, TReturn> : IQueryCommandBuilder<TEntity, TReturn> where TEntity : class, new() where TReturn : class, new()
    {
        /// <summary>
        /// Defines if <b>&gt;&lt;</b> should be replaced by automatically generated joins in your SQL.
        /// </summary>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IBaseQueryRelationBuilder<TEntity, TEntity, TReturn> AddFormatter();
    }
}
