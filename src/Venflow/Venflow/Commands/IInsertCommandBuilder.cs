using System.Collections.Generic;

namespace Venflow.Commands
{
    public interface IInsertCommandBuilder<TEntity> where TEntity : class
    {
        IInsertCommandBuilder<TEntity> ReturnComputedColumns(bool returnComputedColumns = true);

        IInsertCommand<TEntity> Single(TEntity entity);

        IInsertCommand<TEntity> Batch(IEnumerable<TEntity> entities);
    }
}