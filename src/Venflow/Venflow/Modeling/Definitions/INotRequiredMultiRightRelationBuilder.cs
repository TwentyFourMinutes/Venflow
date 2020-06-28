namespace Venflow.Modeling.Definitions
{
    public interface INotRequiredMultiRightRelationBuilder<TEntity, TRelation> : IMultiRightRelationBuilder<TEntity, TRelation>, INotRequiredSingleRightRelationBuilder<TEntity, TRelation> where TEntity : class where TRelation : class
    {
    }
}
