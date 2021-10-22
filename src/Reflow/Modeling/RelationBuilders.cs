namespace Reflow.Modeling
{
    public interface IRequiredSingleRightRelationBuilder<TEntity, TRelation> where TEntity : class, new() where TRelation : class
    {
        IForeignKeyRelationBuilder<TEntity, TRelation> WithOne(Func<TRelation, TEntity> navigationProperty);
    }

    public interface IRequiredMultiRightRelationBuilder<TEntity, TRelation> : IMultiRightRelationBuilder<TEntity, TRelation>, IRequiredSingleRightRelationBuilder<TEntity, TRelation> where TEntity : class, new() where TRelation : class
    {
    }

    public interface INotRequiredSingleRightRelationBuilder<TEntity, TRelation> : IRequiredSingleRightRelationBuilder<TEntity, TRelation> where TEntity : class, new() where TRelation : class
    {
        IForeignKeyRelationBuilder<TEntity, TRelation> WithOne();
    }

    public interface INotRequiredMultiRightRelationBuilder<TEntity, TRelation> : IMultiRightRelationBuilder<TEntity, TRelation>, INotRequiredSingleRightRelationBuilder<TEntity, TRelation> where TEntity : class, new() where TRelation : class
    {
        IForeignKeyRelationBuilder<TEntity, TRelation> WithMany();
    }

    public interface IMultiRightRelationBuilder<TEntity, TRelation> where TEntity : class, new() where TRelation : class
    {
        IForeignKeyRelationBuilder<TEntity, TRelation> WithMany(Func<TRelation, IList<TEntity>> navigationProperty);
    }

    public interface ILeftRelationBuilder<TEntity> where TEntity : class, new()
    {
        INotRequiredMultiRightRelationBuilder<TEntity, TRelation> HasOne<TRelation>(Func<TEntity, TRelation> navigationProperty) where TRelation : class;
        IRequiredMultiRightRelationBuilder<TEntity, TRelation> HasOne<TRelation>() where TRelation : class;
        INotRequiredSingleRightRelationBuilder<TEntity, TRelation> HasMany<TRelation>(Func<TEntity, IList<TRelation>> navigationProperty) where TRelation : class;
        IRequiredSingleRightRelationBuilder<TEntity, TRelation> HasMany<TRelation>() where TRelation : class;
    }

    public interface IForeignKeyRelationBuilder<TEntity, TRelation> where TEntity : class, new() where TRelation : class
    {
        void UsingForeignKey<TKey>(Func<TEntity, TKey> navigationProperty);

        void UsingForeignKey<TKey>(Func<TRelation, TKey> navigationProperty);
    }
}
