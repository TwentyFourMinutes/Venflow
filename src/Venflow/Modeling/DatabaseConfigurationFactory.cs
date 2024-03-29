using System.Collections.ObjectModel;
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

        internal DatabaseConfiguration BuildConfiguration(Type databaseType, DatabaseConfigurationOptionsBuilder configurationOptionsBuilder)
        {
            var tables = GetDatabaseTables(databaseType);

            CreateEntityConfigurations(databaseType, tables, configurationOptionsBuilder);

            var entities = new Dictionary<string, Entity>();
            var entitiesArray = new Entity[_entityFactories.Count];

            var entityFactoriesSpan = _entityFactories.AsSpan();

            for (var i = 0; i < entityFactoriesSpan.Length; i++)
            {
                entityFactoriesSpan[i].ConfigureForeignRelations(_entityBuilders);
            }

            for (var i = 0; i < entityFactoriesSpan.Length; i++)
            {
                var entity = entityFactoriesSpan[i].BuildEntity();

                entities.Add(entity.EntityName, entity);
                entitiesArray[i] = entity;
            }

            for (var i = 0; i < entityFactoriesSpan.Length; i++)
            {
                var entityFactory = entityFactoriesSpan[i];

                entityFactory.ApplyForeignRelations(entities);
            }

            return new DatabaseConfiguration(
                DatabaseTableFactory.CreateInstantiater(databaseType, tables, entitiesArray),
                new ReadOnlyDictionary<string, Entity>(entities),
                entitiesArray,
                configurationOptionsBuilder
            );
        }

        private void CreateEntityConfigurations(Type databaseType, List<PropertyInfo> databaseTables, DatabaseConfigurationOptionsBuilder configurationOptionsBuilder)
        {
            var configurationAssembliesSpan = (configurationOptionsBuilder.ConfigurationAssemblies).AsSpan();

            VenflowConfiguration.SetDefaultValidationIfNeeded(databaseType.Assembly);

            var configurationType = typeof(EntityConfiguration<>);

            var configurations = new Dictionary<Type, Type>();

            for (var assemblyIndex = configurationAssembliesSpan.Length - 1; assemblyIndex >= 0; assemblyIndex--)
            {
                // See https://stackoverflow.com/q/63942274/10070647
                var assemblyTypesSpan = new ReadOnlySpan<Type>(configurationAssembliesSpan[assemblyIndex].GetTypes());

                for (var typeIndex = assemblyTypesSpan.Length - 1; typeIndex >= 0; typeIndex--)
                {
                    var assemblyType = assemblyTypesSpan[typeIndex];

                    if (assemblyType.IsNotPublic ||
                        assemblyType.BaseType is null ||
                        !assemblyType.BaseType.IsGenericType ||
                        assemblyType.BaseType.GetGenericTypeDefinition() != configurationType)
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

            for (var i = 0; i < databaseTablesSpan.Length; i++)
            {
                var property = databaseTablesSpan[i];

                var entityType = property.PropertyType.GetGenericArguments()[0];

                TypeFactory.AddEntityAssembly(entityType.Assembly.GetName().Name!);
                var entityBuilderType = genericEntityBuilderType.MakeGenericType(entityType);
                var entityBuilderInstance = (EntityBuilder) Activator.CreateInstance(entityBuilderType, BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] {configurationOptionsBuilder, property.Name}, null)!;

                if (configurations.TryGetValue(entityType, out var configuration))
                {
                    var entityConfiguration = (IEntityConfiguration)Activator.CreateInstance(configuration)!;

                    AddToConfigurations(entityConfiguration.BuildConfiguration(entityBuilderInstance));
                }
                else
                {
                    var entityFactoryType = genericEntityFactoryType.MakeGenericType(entityType);
                    var entityFactoryInstance = Activator.CreateInstance(entityFactoryType, BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { entityBuilderInstance }, null)!;

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

            for (var i = 0; i < propertiesSpan.Length; i++)
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
