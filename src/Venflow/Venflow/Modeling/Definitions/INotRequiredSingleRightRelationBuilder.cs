namespace Venflow.Modeling.Definitions
{
    public interface INotRequiredSingleRightRelationBuilder<TEntity, TRelation> : IRequiredSingleRightRelationBuilder<TEntity, TRelation> where TEntity : class where TRelation : class
    {
        IForeignKeyRelationBuilder<TEntity, TRelation> WithOne();
    }
}
