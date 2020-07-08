namespace Venflow.Commands
{
    public interface IInsertCommandBuilder<TEntity> : ISpecficVenflowCommandBuilder<IInsertCommand<TEntity>> where TEntity : class
    {
        IInsertCommandBuilder<TEntity> ReturnComputedColumns(bool returnComputedColumns = true);
    }
}