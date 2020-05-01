using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Venflow.Modeling
{
    public class DbConfigurator
    {
        private readonly IDictionary<string, IEntity> _entities;

        internal DbConfigurator()
        {
            _entities = new Dictionary<string, IEntity>();
        }

        public DbConfigurator AddEntity<TEntity>(EntityConfiguration<TEntity> configuration) where TEntity : class
        {
            _entities.Add(configuration.BuildConfiguration());

            return this;
        }

        public DbConfigurator AddEntity<TEntityConfiguration, TEntity>() where TEntity : class
                                                                         where TEntityConfiguration : EntityConfiguration<TEntity>, new()
        {
            _entities.Add(new TEntityConfiguration().BuildConfiguration());
            return this;
        }

        public DbConfigurator AddEntity<TEntity>() where TEntity : class
        {
            var entityType = typeof(TEntity);

            var entityConfigurationBaseType = typeof(EntityConfiguration<>);
            var configuration = entityType.Assembly.GetTypes().FirstOrDefault(x => x.IsPublic && x.IsClass && x.BaseType is { } &&
                                                                              x.BaseType.IsGenericType && x.BaseType.GenericTypeArguments.Length == 1 &&
                                                                              x.BaseType == entityConfigurationBaseType.MakeGenericType(entityType));

            if (configuration is null)
            {
                throw new TypeArgumentException("The provided generic type argument doesn't contain any public Configuration class inheriting IEntityConfiguration.");
            }

            var configurationInstance = (EntityConfiguration)Activator.CreateInstance(configuration)!;

            _entities.Add(configurationInstance.BuildConfiguration());

            return this;
        }

        internal IReadOnlyDictionary<string, IEntity> BuildConfiguration()
        {
            return new ReadOnlyDictionary<string, IEntity>(_entities);
        }
    }
}
