using System.Collections.Generic;

namespace Venflow.Commands
{
    public interface IDeleteCommandBuilder<TEntity> where TEntity : class
    {
        IDeleteCommand<TEntity> Single(TEntity entity);

        IDeleteCommand<TEntity> Batch(IEnumerable<TEntity> entities);
    }
}