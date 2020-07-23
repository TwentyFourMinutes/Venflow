using Venflow.Modeling.Definitions.Builder;

namespace Venflow.Modeling.Definitions
{
    public abstract class EntityConfiguration<TEntity> : EntityConfiguration where TEntity : class
    {
        protected abstract void Configure(IEntityBuilder<TEntity> entityBuilder);

        internal sealed override EntityFactory BuildConfiguration(string tableName)
        {
            var entityBuilder = new EntityBuilder<TEntity>(tableName);

            Configure(entityBuilder);

            return new EntityFactory<TEntity>(entityBuilder);
        }
    }

    public abstract class EntityConfiguration
    {
        internal abstract EntityFactory BuildConfiguration(string tableName);
    }
}
