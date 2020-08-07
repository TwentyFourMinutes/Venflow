namespace Venflow.Commands
{
    public interface IUpdateCommandBuilder<TEntity> : ISpecficVenflowCommandBuilder<IUpdateCommand<TEntity>> where TEntity : class, new()
    {

    }
}