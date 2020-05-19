using System.Collections.Generic;

namespace Venflow.Commands
{
    public interface IUpdateCommandBuilder<TEntity> where TEntity : class
    {
        IUpdateCommand<TEntity> Single(TEntity entity);

        IUpdateCommand<TEntity> Batch(IEnumerable<TEntity> entities);
    }
}