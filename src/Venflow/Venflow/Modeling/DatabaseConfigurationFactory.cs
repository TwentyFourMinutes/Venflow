using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Venflow.Dynamic;
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

        internal DatabaseConfiguration BuildConfiguration(Type databaseType, IReadOnlyList<Assembly> configurationAssemblies)
        {
            var tables = GetDatabaseTables(databaseType);

            CreateEntityConfigurations(databaseType, tables, configurationAssemblies);

            var entities = new Dictionary<string, Entity>();
            var entitiesArray = new Entity[_entityFactories.Count];

            var entityFactoriesSpan = _entityFactories.AsSpan();

            for (int i = entityFactoriesSpan.Length - 1; i >= 0; i--)
            {
                entityFactoriesSpan[i].ConfigureForeignRelations(_entityBuilders);
            }

            for (int i = entityFactoriesSpan.Length - 1; i >= 0; i--)
            {
                var entity = entityFactoriesSpan[i].BuildEntity();

                entities.Add(entity.EntityName, entity);
                entitiesArray[i] = entity;
            }

            for (int i = entityFactoriesSpan.Length - 1; i >= 0; i--)
            {
                var entityFactory = entityFactoriesSpan[i];

                entityFactory.ApplyForeignRelations(entities);
            }

            return new DatabaseConfiguration(DatabaseTableFactory.GetOrCreateInstantiater(databaseType
                ), new ReadOnlyDictionary<string, Entity>(entities), entitiesArray);
        }

        private void CreateEntityConfigurations(Type databaseType, List<PropertyInfo> databaseTables, IReadOnlyList<Assembly> configurationAssemblies)
        {
            var configurationAssembliesSpan = ((List<Assembly>)configurationAssemblies).AsSpan();

            VenflowConfiguration.SetDefaultValidationIfNeeded(databaseType.Assembly);

            var configurationType = typeof(EntityConfiguration<>);

            var configurations = new Dictionary<Type, Type>();

            for (int assemblyIndex = configurationAssembliesSpan.Length - 1; assemblyIndex >= 0; assemblyIndex--)
            {
                // See https://stackoverflow.com/q/63942274/10070647
                var assemblyTypesSpan = new ReadOnlySpan<Type>(configurationAssembliesSpan[assemblyIndex].GetTypes());

                for (int typeIndex = assemblyTypesSpan.Length - 1; typeIndex >= 0; typeIndex--)
                {
                    var assemblyType = assemblyTypesSpan[typeIndex];

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
            }

            var genericEntityBuilderType = typeof(EntityBuilder<>);
            var genericEntityFactoryType = typeof(EntityFactory<>);

            var databaseTablesSpan = databaseTables.AsSpan();

            for (int i = databaseTablesSpan.Length - 1; i >= 0; i--)
            {
                var property = databaseTablesSpan[i];

                var entityType = property.PropertyType.GetGenericArguments()[0];

                if (configurations.TryGetValue(entityType, out var configuration))
                {
                    var entityConfiguration = (IEntityConfiguration)Activator.CreateInstance(configuration)!;

                    AddToConfigurations(entityConfiguration.BuildConfiguration(property.Name));
                }
                else
                {
                    var entityBuilderType = genericEntityBuilderType.MakeGenericType(entityType);
                    var entityFactoryType = genericEntityFactoryType.MakeGenericType(entityType);

                    var entityBuilderInstance = Activator.CreateInstance(entityBuilderType, BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { property.Name }, null);
                    var entityFactoryInstance = Activator.CreateInstance(entityFactoryType, BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { entityBuilderInstance }, null);

                    AddToConfigurations((EntityFactory)entityFactoryInstance);
                }
            }
        }

        private List<PropertyInfo> GetDatabaseTables(Type databaseType)
        {
            var propertiesSpan = databaseType.GetProperties(BindingFlags.Public | BindingFlags.Instance).AsSpan();

            VenflowConfiguration.SetDefaultValidationIfNeeded(databaseType.Assembly);

            var tableType = typeof(ITable);

            var tables = new List<PropertyInfo>();

            for (int i = propertiesSpan.Length - 1; i >= 0; i--)
            {
                var property = propertiesSpan[i];

                if (!tableType.IsAssignableFrom(property.PropertyType))
                {
                    continue;
                }

                tables.Add(property);
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
