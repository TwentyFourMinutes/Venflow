using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace Venflow.Modeling
{
    public class EntityBuilder<TEntity> where TEntity : class
    {
        private readonly Type _type;
        private readonly IDictionary<string, ColumnDefinition<TEntity>> _columnDefinitions;
        private readonly ChangeTrackerFactory _changeTrackerFactory;

        private string? _tableName;

        internal EntityBuilder(ChangeTrackerFactory changeTrackerFactory)
        {
            _changeTrackerFactory = changeTrackerFactory;
            _type = typeof(TEntity);
            _columnDefinitions = new Dictionary<string, ColumnDefinition<TEntity>>();
        }

        public EntityBuilder<TEntity> MapToTable(string tableName)
        {
            _tableName = tableName;

            return this;
        }

        public EntityBuilder<TEntity> MapColumn<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, string columnName)
        {
            var property = ValidatePropertySelector(propertySelector);

            if (_columnDefinitions.TryGetValue(property.Name, out var definition))
            {
                definition.Name = columnName;
            }
            else
            {
                definition = new ColumnDefinition<TEntity>(columnName);

                _columnDefinitions.Add(property.Name, definition);
            }

            return this;
        }

        public EntityBuilder<TEntity> MapId<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, DatabaseGeneratedOption option)
        {
            var property = ValidatePropertySelector(propertySelector);

            var isServerSideGenerated = option != DatabaseGeneratedOption.None;

            var columnDefinition = new PrimaryColumnDefinition<TEntity>(property.Name)
            {
                IsServerSideGenerated = isServerSideGenerated
            };

            if (_columnDefinitions.TryGetValue(property.Name, out var definition))
            {
                columnDefinition.Name = definition.Name;

                _columnDefinitions.Remove(property.Name);
            }

            _columnDefinitions.Add(property.Name, columnDefinition);

            return this;
        }

        private PropertyInfo ValidatePropertySelector<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector)
        {
            if (propertySelector is null)
            {
                throw new ArgumentNullException(nameof(propertySelector));
            }

            var body = propertySelector.Body as MemberExpression;

            if (body is null)
            {
                throw new ArgumentException($"The provided {nameof(propertySelector)} is not pointing to a property.", nameof(propertySelector));
            }

            var property = body.Member as PropertyInfo;

            if (property is null)
            {
                throw new ArgumentException($"The provided {nameof(propertySelector)} is not pointing to a property.", nameof(propertySelector));
            }

            if (!property.CanWrite || !property.SetMethod.IsPublic)
            {
                throw new ArgumentException($"The provided property doesn't contain a setter or it isn't public.", nameof(propertySelector));
            }

            if (_type != property.ReflectedType &&
                !_type.IsSubclassOf(property.ReflectedType!))
            {
                throw new ArgumentException($"The provided {nameof(propertySelector)} is not pointing to a property on the entity itself.", nameof(propertySelector));
            }

            return property;
        }

        internal KeyValuePair<string, IEntity> Build()
        {
            var properties = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (properties is null || properties.Length == 0)
            {
                throw new TypeArgumentException("The provided generic type argument doesn't contain any public properties with a getter and a setter.");
            }

            var columns = new List<EntityColumn<TEntity>>();
            var nameToColumn = new Dictionary<string, int>();
            var changeTrackingColumns = new Dictionary<int, EntityColumn<TEntity>>();
            PrimaryEntityColumn<TEntity>? primaryColumn = null;

            var notMappedAttributeType = typeof(NotMappedAttribute);
            var npgsqlParameterType = typeof(NpgsqlParameter<>);
            var npgsqlDataReaderType = typeof(NpgsqlDataReader);

            var constructorTypes = new Type[2];
            constructorTypes[0] = typeof(string);
            var indexParameter = Expression.Parameter(constructorTypes[0], "index");

            var entityParameter = Expression.Parameter(_type, "entity");
            var valueParameter = Expression.Parameter(typeof(object), "value");

            var stringConcatMethod = constructorTypes[0].GetMethod("Concat", new[] { constructorTypes[0], constructorTypes[0] });

            var columnIndex = 0;
            var regularColumnsOffset = 0;
            var propertyFlagValue = 1uL;

            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];

                if (!property.CanWrite || !property.SetMethod!.IsPublic || Attribute.IsDefined(property, notMappedAttributeType))
                {
                    continue;
                }

                var hasCustomDefinition = false;

                constructorTypes[1] = property.PropertyType;

                var valueProperty = Expression.Property(entityParameter, property);

                var constructor = npgsqlParameterType.MakeGenericType(property.PropertyType).GetConstructor(constructorTypes)!;

                var parameterInstance = Expression.New(constructor, Expression.Add(Expression.Constant("@" + property.Name), indexParameter, stringConcatMethod), valueProperty);

                var parameterValueRetriever = Expression.Lambda<Func<TEntity, string, NpgsqlParameter>>(parameterInstance, entityParameter, indexParameter).Compile();

                Expression valueAssignment;
                var valueRetriever = GetDbValueRetrieverMethod(property, npgsqlDataReaderType);

                if (property.PropertyType.IsClass || Nullable.GetUnderlyingType(property.PropertyType) is { })
                {
                    valueAssignment = Expression.Assign(valueProperty, Expression.TypeAs(valueParameter, property.PropertyType));
                }
                else
                {
                    valueAssignment = Expression.Assign(valueProperty, Expression.Convert(valueParameter, property.PropertyType));
                }

                var valueWriter = Expression.Lambda<Action<TEntity, object>>(valueAssignment, entityParameter, valueParameter).Compile();

                if (_columnDefinitions.TryGetValue(property.Name, out var definition))
                {
                    switch (definition)
                    {
                        case PrimaryColumnDefinition<TEntity> primaryDefintion:
                            primaryColumn = new PrimaryEntityColumn<TEntity>(property, definition.Name, propertyFlagValue, valueRetriever, valueWriter, parameterValueRetriever, primaryDefintion.IsServerSideGenerated);

                            columns.Insert(0, primaryColumn);

                            regularColumnsOffset++;

                            if (property.GetSetMethod().IsVirtual)
                            {
                                changeTrackingColumns.Add(columnIndex, primaryColumn);
                            }

                            nameToColumn.Add(definition.Name, columnIndex);

                            hasCustomDefinition = true;

                            break;
                    }
                }

                if (!hasCustomDefinition)
                {
                    var columnName = definition?.Name ?? property.Name;

                    var column = new EntityColumn<TEntity>(property, columnName, propertyFlagValue, valueRetriever, valueWriter, parameterValueRetriever);

                    columns.Add(column);

                    if (property.GetSetMethod().IsVirtual)
                    {
                        changeTrackingColumns.Add(columnIndex, column);
                    }

                    nameToColumn.Add(columnName, columnIndex);
                }

                columnIndex++;
                propertyFlagValue <<= 1;
            }

            if (primaryColumn is null)
            {
                throw new InvalidOperationException("The entityBuilder didn't configure the primary key nor is any property named 'Id'.");
            }

            if (_tableName is null)
            {
                _tableName = _type.Name + "s";
            }

            Type? proxyType = null;
            Func<ChangeTracker<TEntity>, TEntity>? factory = null;
            Func<ChangeTracker<TEntity>, TEntity, TEntity>? applier = null;

            var entityColumns = new EntityColumnCollection<TEntity>(columns.ToArray(), nameToColumn);

            if (changeTrackingColumns.Count != 0)
            {
                (proxyType, factory, applier) = _changeTrackerFactory.GenerateEntityProxyFactories(_type, changeTrackingColumns, entityColumns);
            }

            return new KeyValuePair<string, IEntity>(_type.Name, new Entity<TEntity>(_type, proxyType, factory, applier, _tableName, entityColumns, regularColumnsOffset, primaryColumn));
        }

        private MethodInfo GetDbValueRetrieverMethod(PropertyInfo property, Type readerType)
        {
            return readerType!.GetMethod("GetFieldValue", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(property.PropertyType);
        }
    }
}
