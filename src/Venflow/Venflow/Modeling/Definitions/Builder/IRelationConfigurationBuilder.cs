namespace Venflow.Modeling.Definitions.Builder
{
    public interface IRelationConfigurationBuilder<TEntity, TRelation>
        where TEntity : class, new()
        where TRelation : class
    {
        IRelationConfigurationBuilder<TEntity, TRelation> HasConstraintName(string name);
        IRelationConfigurationBuilder<TEntity, TRelation> OnDelete(ConstraintAction constraintAction);
        IRelationConfigurationBuilder<TEntity, TRelation> OnUpdate(ConstraintAction constraintAction);
    }
}
