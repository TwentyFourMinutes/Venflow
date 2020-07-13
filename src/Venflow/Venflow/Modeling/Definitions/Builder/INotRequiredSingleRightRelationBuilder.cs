namespace Venflow.Modeling.Definitions.Builder
{
    public interface INotRequiredSingleRightRelationBuilder<TEntity, TRelation> : IRequiredSingleRightRelationBuilder<TEntity, TRelation> where TEntity : class where TRelation : class
    {
        IForeignKeyRelationBuilder<TEntity, TRelation> WithOne();
    }
}
