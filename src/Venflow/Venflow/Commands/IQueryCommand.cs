using System.Threading;
using System.Threading.Tasks;

namespace Venflow.Commands
{
    public interface IQueryCommand<TEntity> : IVenflowCommand<TEntity> where TEntity : class
    {
        Task<IQueryCommand<TEntity>> PrepareAsync(CancellationToken cancellationToken = default);
        IQueryCommand<TEntity> Unprepare();
    }
}