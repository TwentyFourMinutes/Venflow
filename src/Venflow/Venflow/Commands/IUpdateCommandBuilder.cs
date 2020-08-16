namespace Venflow.Commands
{
    /// <summary>
    /// Represents a command builder to configure the update.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity which will be updated.</typeparam>
    public interface IUpdateCommandBuilder<TEntity> : ISpecficVenflowCommandBuilder<IUpdateCommand<TEntity>> where TEntity : class, new()
    {

    }
}