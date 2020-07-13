using Venflow.Modeling.Definitions.Builder;

namespace Venflow.Modeling.Definitions
{
    public abstract class EntityConfiguration<TEntity> : EntityConfiguration where TEntity : class
    {
        protected abstract void Configure(IEntityBuilder<TEntity> entityBuilder);

        internal sealed override EntityFactory BuildConfiguration()
        {
            var entityBuilder = new EntityBuilder<TEntity>();

            Configure(entityBuilder);

            return new EntityFactory<TEntity>(entityBuilder);
        }
    }

    public abstract class EntityConfiguration
    {
        internal abstract EntityFactory BuildConfiguration();
    }
}
