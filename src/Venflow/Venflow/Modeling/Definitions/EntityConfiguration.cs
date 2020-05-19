namespace Venflow.Modeling.Definitions
{
    public abstract class EntityConfiguration<TEntity> : EntityConfiguration where TEntity : class
    {
        protected abstract void Configure(EntityBuilder<TEntity> entityBuilder);

        internal sealed override IEntityFactory BuildConfiguration()
        {
            var entityBuilder = new EntityBuilder<TEntity>();

            Configure(entityBuilder);

            return new EntityFactory<TEntity>(entityBuilder);
        }
    }

    public abstract class EntityConfiguration
    {
        internal abstract IEntityFactory BuildConfiguration();
    }
}
