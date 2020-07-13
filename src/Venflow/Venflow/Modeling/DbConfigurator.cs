using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Venflow.Modeling.Definitions;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.Modeling
{
    internal class DbConfigurator
    {
        private readonly List<EntityFactory> _entityFactories;
        private readonly Dictionary<string, EntityBuilder> _entityBuilders;

        internal DbConfigurator()
        {
            _entityFactories = new List<EntityFactory>();
            _entityBuilders = new Dictionary<string, EntityBuilder>();
        }

        internal IReadOnlyDictionary<string, Entity> BuildConfiguration(DbConfiguration dbConfiguration, Type dbConfigurationType)
        {
            var tables = FindDbConfigurations(dbConfigurationType);

            var entities = new Dictionary<string, Entity>();

            for (int i = 0; i < _entityFactories.Count; i++)
            {
                _entityFactories[i].ConfigureForeignRelations(_entityBuilders);
            }

            for (int i = 0; i < _entityFactories.Count; i++)
            {
                var entity = _entityFactories[i].BuildEntity();

                var table = tables[i];

                table.GetSetMethod().Invoke(dbConfiguration, new object[] { table.PropertyType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(DbConfiguration), entity.GetType() }, null).Invoke(new object[] { dbConfiguration, entity }) });

                entities.Add(entity.EntityName, entity);
            }

            for (int i = 0; i < _entityFactories.Count; i++)
            {
                var entityFactory = _entityFactories[i];

                entityFactory.ApplyForeignRelations(entities);
            }

            return new ReadOnlyDictionary<string, Entity>(entities);
        }

        private List<PropertyInfo> FindDbConfigurations(Type dbConfigurationType)
        {
            var properties = dbConfigurationType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var assembly = dbConfigurationType.Assembly;

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
