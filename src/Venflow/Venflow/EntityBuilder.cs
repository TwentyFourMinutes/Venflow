using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace Venflow
{
    public class EntityBuilder<TEntity> where TEntity : class
    {
        private readonly Type _type;
        private readonly IDictionary<string, ColumnDefinition> _columnDefinitions;

        private string? _tableName;

        internal EntityBuilder()
        {
            _type = typeof(TEntity);
            _columnDefinitions = new Dictionary<string, ColumnDefinition>();
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
                definition = new ColumnDefinition(columnName);

                _columnDefinitions.Add(property.Name, definition);
            }

            return this;
        }

        public EntityBuilder<TEntity> MapId<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, DatabaseGeneratedOption option)
        {
            var property = ValidatePropertySelector(propertySelector);

            var entityParameter = Expression.Parameter(_type, "entity");
            var valueParameter = Expression.Parameter(typeof(object), "value");

            var valueProperty = Expression.Property(entityParameter, property);

            var valueAssignment = Expression.Assign(valueProperty, Expression.Convert(valueParameter, typeof(TTarget)));

            var valueWriter = Expression.Lambda<Action<TEntity, object>>(valueAssignment, entityParameter, valueParameter).Compile();

            var isServerSideGenerated = option != DatabaseGeneratedOption.None;

            var columnDefinition = new PrimaryColumnDefinition<TEntity>(property.Name)
            {
                ValueWriter = valueWriter,
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
            PrimaryEntityColumn<TEntity>? primaryColumn = null;


            var notMappedAttributeType = typeof(NotMappedAttribute);
            var npgsqlParameterType = typeof(NpgsqlParameter<>);
            var constructorTypes = new Type[2];
            constructorTypes[0] = typeof(string);

            var entityParameter = Expression.Parameter(_type, "entity");

            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];

                if (!property.CanWrite || !property.SetMethod!.IsPublic || Attribute.IsDefined(property, notMappedAttributeType))
                {
                    continue;
                }

                var hasCustomDefinition = false;

                var propertyGetter = Expression.Property(entityParameter, property);

                constructorTypes[1] = property.PropertyType;

                var constructor = npgsqlParameterType.MakeGenericType(property.PropertyType).GetConstructor(constructorTypes)!;

                var parameterInstance = Expression.New(constructor, Expression.Constant("@" + property.Name), propertyGetter);

                var prameterValueRetriever = Expression.Lambda<Func<TEntity, NpgsqlParameter>>(parameterInstance, entityParameter).Compile();


                if (_columnDefinitions.TryGetValue(property.Name, out var definition))
                {
                    switch (definition)
                    {
                        case PrimaryColumnDefinition<TEntity> primaryDefintion:
                            primaryColumn = new PrimaryEntityColumn<TEntity>(definition.Name, prameterValueRetriever, primaryDefintion.ValueWriter!, primaryDefintion.IsServerSideGenerated);

                            hasCustomDefinition = true;
                            break;
                    }
                }

                if (!hasCustomDefinition)
                {
                    var columnName = definition?.Name ?? property.Name;

                    columns.Add(new EntityColumn<TEntity>(columnName, prameterValueRetriever));
                }
            }

            if (primaryColumn is null)
            {
                throw new InvalidOperationException("The entityBuilder didn't configure the primary key nor is any property named 'Id'.");
            }

            if (_tableName is null)
            {
                _tableName = _type.Name + "s";
            }

            columns.Insert(0, primaryColumn);

            return new KeyValuePair<string, IEntity>(_type.Name, new Entity<TEntity>(_tableName, columns.ToArray(), primaryColumn));
        }
    }
}
