using System.Collections.Generic;

namespace Venflow
{
    public abstract class EntityConfiguration<TEntity> : EntityConfiguration where TEntity : class
    {
        protected abstract void Configure(EntityBuilder<TEntity> entityBuilder);

        internal sealed override KeyValuePair<string, IEntity> BuildConfiguration()
        {
            var entityBuilder = new EntityBuilder<TEntity>();

            Configure(entityBuilder);

            return entityBuilder.Build();
        }
    }

    public abstract class EntityConfiguration
    {
        internal abstract KeyValuePair<string, IEntity> BuildConfiguration();
    }
}
