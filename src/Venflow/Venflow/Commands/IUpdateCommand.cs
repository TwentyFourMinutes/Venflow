using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow.Commands
{
    public interface IUpdateCommand<TEntity> : IVenflowCommand<TEntity> where TEntity : class, new()
    {
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    }
}