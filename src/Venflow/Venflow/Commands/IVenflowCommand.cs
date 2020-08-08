using System;

namespace Venflow.Commands
{
    public interface IVenflowCommand<TEntity> : IAsyncDisposable where TEntity : class, new()
    {

    }
}
