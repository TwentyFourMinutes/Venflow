namespace Venflow.Modeling.Definitions.Builder
{
    public interface INotRequiredSingleRightRelationBuilder<TEntity, TRelation> : IRequiredSingleRightRelationBuilder<TEntity, TRelation> where TEntity : class where TRelation : class
    {
        /// <summary>
        /// <para>
        ///     Configures this as a one-to-one relationship.
        /// </para>
        /// <para>
        ///     Note that calling this method will explicitly configure this side of the relationship to use no navigation property, even if such a property exists on the entity type. If the navigation property is to be used, then it must be specified.
        /// </para>
        /// </summary>
        /// <returns>An object that can be used to configure the relationship.</returns>
        IForeignKeyRelationBuilder<TEntity, TRelation> WithOne();
    }
}
