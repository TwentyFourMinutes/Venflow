namespace Venflow.Commands
{
    public interface IInsertCommandBuilder<TEntity> : ISpecficVenflowCommandBuilder<IInsertCommand<TEntity>> where TEntity : class
    {
        IInsertCommandBuilder<TEntity> SetIdentityColumns();
        IInsertCommandBuilder<TEntity> PopulateRelation();
        IInsertCommandBuilder<TEntity> DoNotSetIdentityColumns();
        IInsertCommandBuilder<TEntity> DoNotDoNotSetPopulateRelation();
    }
}