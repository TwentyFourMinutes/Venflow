using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow.Commands
{
    public interface IDeleteCommand<TEntity> : IVenflowCommand<TEntity> where TEntity : class
    {
        Task<IDeleteCommand<TEntity>> PrepareAsync(CancellationToken cancellationToken = default);
        IDeleteCommand<TEntity> Unprepare();

        Task<int> DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}