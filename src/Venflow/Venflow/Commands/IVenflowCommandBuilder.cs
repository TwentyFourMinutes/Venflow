namespace Venflow.Commands
{
    public interface IVenflowCommandBuilder<TEntity> : IQueryCommandBuilder<TEntity>, IInsertCommandBuilder<TEntity>, IDeleteCommandBuilder<TEntity>, IUpdateCommandBuilder<TEntity> where TEntity : class
    {
        IQueryCommandBuilder<TEntity> Query();

        IInsertCommandBuilder<TEntity> Insert();

        IDeleteCommandBuilder<TEntity> Delete();

        IUpdateCommandBuilder<TEntity> Update();
    }
}