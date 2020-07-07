using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow.Commands
{
    public interface IInsertCommand<TEntity> : IVenflowCommand<TEntity> where TEntity : class
    {
        Task<IInsertCommand<TEntity>> PrepareAsync(CancellationToken cancellationToken = default);
        IInsertCommand<TEntity> Unprepare();

        Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<int> InsertAsync(List<TEntity> entities, CancellationToken cancellationToken = default);
    }
}