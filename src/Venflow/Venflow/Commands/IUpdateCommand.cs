using System.Threading;
using System.Threading.Tasks;

namespace Venflow.Commands
{
    public interface IUpdateCommand<TEntity> : IVenflowCommand<TEntity> where TEntity : class
    {
        Task<IUpdateCommand<TEntity>> PrepareAsync(CancellationToken cancellationToken = default);
        IUpdateCommand<TEntity> Unprepare();
    }
}