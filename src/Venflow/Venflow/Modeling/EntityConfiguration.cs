using System.Collections.Generic;

namespace Venflow.Modeling
{
    public abstract class EntityConfiguration<TEntity> : EntityConfiguration where TEntity : class
    {
        protected abstract void Configure(EntityBuilder<TEntity> entityBuilder);

        internal sealed override KeyValuePair<string, IEntity> BuildConfiguration(ChangeTrackerFactory changeTrackerFactory)
        {
            var entityBuilder = new EntityBuilder<TEntity>(changeTrackerFactory);

            Configure(entityBuilder);

            return entityBuilder.Build();
        }
    }

    public abstract class EntityConfiguration
    {
        internal abstract KeyValuePair<string, IEntity> BuildConfiguration(ChangeTrackerFactory changeTrackerFactory);
    }
}
