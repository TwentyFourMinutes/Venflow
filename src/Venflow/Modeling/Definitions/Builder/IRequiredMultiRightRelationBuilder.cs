namespace Venflow.Modeling.Definitions.Builder
{
    /// <summary>
    /// This interface hosts relation methods for the right side of a relation.
    /// </summary>
    public interface IRequiredMultiRightRelationBuilder<TEntity, TRelation> : IMultiRightRelationBuilder<TEntity, TRelation>, IRequiredSingleRightRelationBuilder<TEntity, TRelation> where TEntity : class, new() where TRelation : class
    {
    }
}
