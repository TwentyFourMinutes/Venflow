using Venflow.Modeling.Definitions.Builder;

namespace Venflow.Modeling.Definitions
{
    /// <summary>
    /// Allows configuration for an entity type. Inherit from this class and override the <see cref="Configure(IEntityBuilder{TEntity})"/> method to configure the entity <see cref="TEntity"></see>.
    /// </summary>
    /// <typeparam name="TEntity">The entity to be configured.</typeparam>
    /// <remarks>Classes which inherit from this one, have to be in the same assembly as the <see cref="Database"/> in order to be discoverable.</remarks>
    public abstract class EntityConfiguration<TEntity> : EntityConfiguration where TEntity : class
    {
        /// <summary>
        /// Allows for configuration of the entity <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="entityBuilder">The builder used to configure the entity.</param>
        protected abstract void Configure(IEntityBuilder<TEntity> entityBuilder);

        internal sealed override EntityFactory BuildConfiguration(string tableName)
        {
            var entityBuilder = new EntityBuilder<TEntity>(tableName);

            Configure(entityBuilder);

            return new EntityFactory<TEntity>(entityBuilder);
        }
    }

    /// <summary>
    /// <strong>This is a class which is only used for type safety, do not inherit!</strong>
    /// </summary>
    public abstract class EntityConfiguration
    {
        internal abstract EntityFactory BuildConfiguration(string tableName);
    }
}
