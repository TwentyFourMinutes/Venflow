using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Venflow.Dynamic.Instantiater;
using Venflow.Modeling.Definitions;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.Modeling
{
    internal class DatabaseConfigurationFactory
    {
        private readonly List<EntityFactory> _entityFactories;
        private readonly Dictionary<string, EntityBuilder> _entityBuilders;

        internal DatabaseConfigurationFactory()
        {
            _entityFactories = new List<EntityFactory>();
            _entityBuilders = new Dictionary<string, EntityBuilder>();
        }

        internal DatabaseConfiguration BuildConfiguration(Type databaseType)
        {
            var tables = FindEntityConfigurations(databaseType);

            var entities = new Dictionary<string, Entity>();
            var entitiesArray = new Entity[_entityFactories.Count];

            for (int i = 0; i < _entityFactories.Count; i++)
            {
                _entityFactories[i].ConfigureForeignRelations(_entityBuilders);
            }

            for (int i = 0; i < _entityFactories.Count; i++)
            {
                var entity = _entityFactories[i].BuildEntity();

                entities.Add(entity.EntityName, entity);
                entitiesArray[i] = entity;
            }

            for (int i = 0; i < _entityFactories.Count; i++)
            {
                var entityFactory = _entityFactories[i];

                entityFactory.ApplyForeignRelations(entities);
            }

            return new DatabaseConfiguration(DatabaseTableFactory.CreateInstantiater(tables, entitiesArray), new ReadOnlyDictionary<string, Entity>(entities), entitiesArray);
        }

        private List<PropertyInfo> FindEntityConfigurations(Type databaseType)
        {
            var properties = databaseType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var assembly = databaseType.Assembly;

            var tableType = typeof(Table<>);
            var configurationType = typeof(EntityConfiguration<>);
            var assemblyTypes = assembly.GetTypes();

            var configurations = new Dictionary<Type, Type>();

            for (int i = 0; i < assemblyTypes.Length; i++)
            {
                var assemblyType = assemblyTypes[i];

                if (assemblyType.IsNotPublic || assemblyType.BaseType is null || !assemblyType.BaseType.IsGenericType || assemblyType.BaseType.GetGenericTypeDefinition() != configurationType)
                    continue;

                var entityType = assemblyType.BaseType.GetGenericArguments()[0];

#if NET48
                if (configurations.ContainsKey(entityType))
                {
                    throw new InvalidOperationException($"There are two or more configurations for the entity '{entityType.Name}'");
                }
                else
                {
                    configurations.Add(entityType, assemblyType);
                }
#else
                if (!configurations.TryAdd(entityType, assemblyType))
                {
                    throw new InvalidOperationException($"There are two or more configurations for the entity '{entityType.Name}'");
                }
#endif
            }

            var tables = new List<PropertyInfo>();

            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];

                if (property.PropertyType.GetGenericTypeDefinition() != tableType)
                {
                    continue;
                }

                var entityType = property.PropertyType.GetGenericArguments()[0];

                if (!configurations.TryGetValue(entityType, out var configuration))
                {
                    throw new InvalidOperationException($"There is no entity configuration for the entity '{entityType.Name}'.");
                }

                tables.Add(property);

                var entityConfiguration = (EntityConfiguration)Activator.CreateInstance(configuration)!;

                AddToConfigurations(entityConfiguration.BuildConfiguration());
            }

            return tables;
        }

        private void AddToConfigurations(EntityFactory entityFactory)
        {
            _entityFactories.Add(entityFactory);
            _entityBuilders.Add(entityFactory.EntityBuilder.Type.Name, entityFactory.EntityBuilder);
        }
    }
}
