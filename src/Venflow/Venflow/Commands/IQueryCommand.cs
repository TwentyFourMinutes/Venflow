using System.Threading;
using System.Threading.Tasks;

namespace Venflow.Commands
{
    public interface IQueryCommand<TEntity, TReturn> : IVenflowCommand<TEntity> where TEntity : class where TReturn : class
    {
        Task<IQueryCommand<TEntity, TReturn>> PrepareAsync(CancellationToken cancellationToken = default);
        Task<IQueryCommand<TEntity, TReturn>> UnprepareAsync(CancellationToken cancellationToken = default);

        Task<TReturn> QueryAsync(CancellationToken cancellationToken = default);
    }
}