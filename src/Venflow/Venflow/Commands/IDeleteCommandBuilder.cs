namespace Venflow.Commands
{
    /// <summary>
    /// Represents a command builder to configure the deletion.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity which will be deleted.</typeparam>
    public interface IDeleteCommandBuilder<TEntity> : ISpecficVenflowCommandBuilder<IDeleteCommand<TEntity>> where TEntity : class, new()
    {

    }
}