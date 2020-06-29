using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow.Commands
{
    public interface IQueryCommand<TEntity> : IVenflowCommand<TEntity> where TEntity : class
    {
        Task<IQueryCommand<TEntity>> PrepareAsync(CancellationToken cancellationToken = default);
        IQueryCommand<TEntity> Unprepare();

        Task<TEntity?> QuerySingleAsync(CancellationToken cancellationToken = default);
        Task<List<TEntity>> QueryBatchAsync(CancellationToken cancellationToken = default);
    }
}