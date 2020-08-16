using System;

namespace Venflow.Commands
{
    /// <summary>
    /// The base command for all other CRUD commands.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity which is being used in the current operation.</typeparam>
    public interface IVenflowCommand<TEntity> : IAsyncDisposable where TEntity : class, new()
    {

    }
}
