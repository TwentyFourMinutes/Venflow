using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow.Commands
{
    public interface IDeleteCommand<TEntity> : IVenflowCommand<TEntity> where TEntity : class
    {
        Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<int> DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    }
}