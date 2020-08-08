namespace Venflow.Commands
{
    public interface IPreCommandBuilder<TEntity, TReturn> : IQueryCommandBuilder<TEntity, TReturn> where TEntity : class, new() where TReturn : class, new()
    {
        IQueryCommandBuilder<TEntity, TReturn> AddFormatter();
    }
}