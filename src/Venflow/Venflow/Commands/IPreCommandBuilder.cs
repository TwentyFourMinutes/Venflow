namespace Venflow.Commands
{
    public interface IPreCommandBuilder<TEntity, TReturn> : IQueryCommandBuilder<TEntity, TReturn> where TEntity : class where TReturn : class
    {
        IQueryCommandBuilder<TEntity, TReturn> AddFormatter();
    }
}