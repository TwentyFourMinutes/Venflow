using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Venflow.Modeling.Definitions;

namespace Venflow.Modeling
{
    public class DbConfigurator
    {
        private readonly List<EntityFactory> _entityFactories;
        private readonly Dictionary<string, EntityBuilder> _entityBuilders;

        internal DbConfigurator()
        {
            _entityFactories = new List<EntityFactory>();
            _entityBuilders = new Dictionary<string, EntityBuilder>();
        }

        public DbConfigurator AddEntity<TEntity>(EntityConfiguration<TEntity> configuration) where TEntity : class
        {
            AddToConfigurations(configuration.BuildConfiguration());

            return this;
        }

        public DbConfigurator AddEntity<TEntityConfiguration, TEntity>() where TEntity : class
                                                                         where TEntityConfiguration : EntityConfiguration<TEntity>, new()
        {
            AddToConfigurations(new TEntityConfiguration().BuildConfiguration());

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

            AddToConfigurations(configurationInstance.BuildConfiguration());

            return this;
        }

        internal IReadOnlyDictionary<string, Entity> BuildConfiguration()
        {
            var entities = new Dictionary<string, Entity>();

            for (int i = 0; i < _entityFactories.Count; i++)
            {
                _entityFactories[i].ConfigureForeignRelations(_entityBuilders);
            }

            for (int i = 0; i < _entityFactories.Count; i++)
            {
                var entityFactory = _entityFactories[i].BuildEntity();

                entities.Add(entityFactory.EntityName, entityFactory);
            }

            for (int i = 0; i < _entityFactories.Count; i++)
            {
                var entityFactory = _entityFactories[i];

                entityFactory.ApplyForeignRelations(entities);
            }

            return new ReadOnlyDictionary<string, Entity>(entities);
        }

        private void AddToConfigurations(EntityFactory entityFactory)
        {
            _entityFactories.Add(entityFactory);
            _entityBuilders.Add(entityFactory.EntityBuilder.Type.Name, entityFactory.EntityBuilder);
        }
    }
}
