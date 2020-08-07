namespace Venflow.Commands
{
    public interface IDeleteCommandBuilder<TEntity> : ISpecficVenflowCommandBuilder<IDeleteCommand<TEntity>> where TEntity : class, new()
    {

    }
}