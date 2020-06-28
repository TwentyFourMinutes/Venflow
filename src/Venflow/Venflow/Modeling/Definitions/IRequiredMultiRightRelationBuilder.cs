namespace Venflow.Modeling.Definitions
{
    public interface IRequiredMultiRightRelationBuilder<TEntity, TRelation> : IMultiRightRelationBuilder<TEntity, TRelation>, IRequiredSingleRightRelationBuilder<TEntity, TRelation> where TEntity : class where TRelation : class
    {
    }
}
