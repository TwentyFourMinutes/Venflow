namespace Venflow.Commands
{
    public interface IInsertCommandBuilder<TEntity> where TEntity : class
    {
        IInsertCommandBuilder<TEntity> ReturnComputedColumns(bool returnComputedColumns = true);
        IInsertCommand<TEntity> Todo();
    }
}