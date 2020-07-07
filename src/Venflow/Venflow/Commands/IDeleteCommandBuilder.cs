namespace Venflow.Commands
{
    public interface IDeleteCommandBuilder<TEntity> where TEntity : class
    {
        IDeleteCommand<TEntity> Compile();
    }
}