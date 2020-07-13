namespace Venflow.Modeling.Definitions.Builder
{
    public interface INotRequiredMultiRightRelationBuilder<TEntity, TRelation> : IMultiRightRelationBuilder<TEntity, TRelation>, INotRequiredSingleRightRelationBuilder<TEntity, TRelation> where TEntity : class where TRelation : class
    {
        IForeignKeyRelationBuilder<TEntity, TRelation> WithMany();
    }
}
