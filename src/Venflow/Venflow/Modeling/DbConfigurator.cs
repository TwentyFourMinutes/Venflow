using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Venflow.Modeling.Definitions;

namespace Venflow.Modeling
{
    public class DbConfigurator
    {
        private readonly List<IEntityFactory> _entityFactories;

        internal DbConfigurator()
        {
            _entityFactories = new List<IEntityFactory>();
        }

        public DbConfigurator AddEntity<TEntity>(EntityConfiguration<TEntity> configuration) where TEntity : class
        {
            _entityFactories.Add(configuration.BuildConfiguration());

            return this;
        }

        public DbConfigurator AddEntity<TEntityConfiguration, TEntity>() where TEntity : class
                                                                         where TEntityConfiguration : EntityConfiguration<TEntity>, new()
        {
            _entityFactories.Add(new TEntityConfiguration().BuildConfiguration());

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

            _entityFactories.Add(configurationInstance.BuildConfiguration());

            return this;
        }

        internal IReadOnlyDictionary<string, IEntity> BuildConfiguration()
        {
            var entities = new Dictionary<string, IEntity>();

            for (int i = 0; i < _entityFactories.Count; i++)
            {
                var factory = _entityFactories[i];

                factory.BuildEntity();

                entities.Add(factory.Entity.EntityName, factory.Entity);
            }

            for (int i = 0; i < _entityFactories.Count; i++)
            {
                _entityFactories[i].ApplyRelations(entities);
            }

            return new ReadOnlyDictionary<string, IEntity>(entities);
        }
    }
}
