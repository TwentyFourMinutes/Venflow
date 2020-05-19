using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Venflow.Modeling.Definitions
{
    public class EntityBuilder<TEntity> where TEntity : class
    {
        internal Type Type { get; }

        internal ChangeTrackerFactory<TEntity>? ChangeTrackerFactory { get; private set; }
        internal Action<TEntity, StringBuilder, string, NpgsqlParameterCollection> InsertWriter { get; private set; }
        internal string TableName { get; private set; }
        internal List<EntityRelationDefinition> Relations { get;}

        private readonly IDictionary<string, ColumnDefinition<TEntity>> _columnDefinitions;
        private readonly HashSet<string> _ignoredColumns;

        internal EntityBuilder()
        {
            Type = typeof(TEntity);
            Relations = new List<EntityRelationDefinition>();
            _columnDefinitions = new Dictionary<string, ColumnDefinition<TEntity>>();
            _ignoredColumns = new HashSet<string>();
        }

        public EntityBuilder<TEntity> MapToTable(string tableName)
        {
            TableName = tableName;

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

        public EntityBuilder<TEntity> Ignore<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector)
        {
            var property = ValidatePropertySelector(propertySelector);

            _ignoredColumns.Add(property.Name);

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

        public EntityBuilder<TEntity> MapOneToOne<TRelation, TForeignKey>(Expression<Func<TEntity, TRelation>> relationSelector, Expression<Func<TEntity, TForeignKey>> foreignSelector)
        {
            var relation = ValidatePropertySelector(relationSelector);
            var foreignKey = ValidatePropertySelector(foreignSelector);

            _ignoredColumns.Add(relation.Name);

            Relations.Add(new EntityRelationDefinition(relation, foreignKey, relation.PropertyType.Name));

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

            if (Type != property.ReflectedType &&
                !Type.IsSubclassOf(property.ReflectedType!))
            {
                throw new ArgumentException($"The provided {nameof(propertySelector)} is not pointing to a property on the entity itself.", nameof(propertySelector));
            }

            return property;
        }

        internal EntityColumnCollection<TEntity> Build()
        {
            var properties = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (properties is null || properties.Length == 0)
            {
                throw new TypeArgumentException("The provided generic type argument doesn't contain any public properties with a getter and a setter.");
            }

            // ExpressionVariables

            var columns = new List<EntityColumn<TEntity>>();
            var nameToColumn = new Dictionary<string, int>();
            var changeTrackingColumns = new Dictionary<int, EntityColumn<TEntity>>();
            PrimaryEntityColumn<TEntity>? primaryColumn = null;

            var insertWriterVariables = new List<ParameterExpression>();
            var insertWriterStatments = new List<Expression>();

            var constructorTypes = new Type[2];
            constructorTypes[0] = TypeCache.String;

            var indexParameter = Expression.Parameter(TypeCache.String, "index");
            var entityParameter = Expression.Parameter(Type, "entity");
            var valueParameter = Expression.Parameter(TypeCache.Object, "value");
            var stringBuilderParameter = Expression.Parameter(TypeCache.StringBuilder, "commandString");
            var npgsqlParameterCollectionParameter = Expression.Parameter(TypeCache.NpgsqlParameterCollection, "parameters");

            var stringConcatMethod = TypeCache.String.GetMethod("Concat", new[] { TypeCache.String, TypeCache.String });
            var stringBuilderAppend = TypeCache.StringBuilder.GetMethod("Append", new[] { TypeCache.String });
            var npgsqlParameterCollectionAdd = TypeCache.NpgsqlParameterCollection.GetMethod("Add", new Type[] { TypeCache.GenericNpgsqlParameter });

            // Important column specifications

            var columnIndex = 0;
            var regularColumnsOffset = 0;
            var propertyFlagValue = 1uL;

            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];

                if (!property.CanWrite || !property.SetMethod!.IsPublic || _ignoredColumns.Contains(property.Name) || Attribute.IsDefined(property, TypeCache.NotMappedAttribute))
                {
                    continue;
                }

                var hasCustomDefinition = false;

                // ParameterValueRetriever

                constructorTypes[1] = property.PropertyType;

                var valueProperty = Expression.Property(entityParameter, property);

                var genericNpgsqlParameter = TypeCache.GenericNpgsqlParameter.MakeGenericType(property.PropertyType);

                var constructor = genericNpgsqlParameter.GetConstructor(constructorTypes)!;

                var parameterInstance = Expression.New(constructor, Expression.Add(Expression.Constant("@" + property.Name), indexParameter, stringConcatMethod), valueProperty);

                var parameterValueRetriever = Expression.Lambda<Func<TEntity, string, NpgsqlParameter>>(parameterInstance, entityParameter, indexParameter).Compile();

                Expression valueAssignment;
                var valueRetriever = TypeCache.NpgsqlDataReader!.GetMethod("GetFieldValue", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(property.PropertyType);

                if (property.PropertyType.IsClass || Nullable.GetUnderlyingType(property.PropertyType) is { })
                {
                    valueAssignment = Expression.Assign(valueProperty, Expression.TypeAs(valueParameter, property.PropertyType));
                }
                else
                {
                    valueAssignment = Expression.Assign(valueProperty, Expression.Convert(valueParameter, property.PropertyType));
                }

                var valueWriter = Expression.Lambda<Action<TEntity, object>>(valueAssignment, entityParameter, valueParameter).Compile();

                var isPrimaryColumn = false;

                // Handle custom columns

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
                            isPrimaryColumn = true;
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

                if (!isPrimaryColumn)
                {
                    var parameterVariable = Expression.Variable(genericNpgsqlParameter, property.Name.ToLower());

                    insertWriterVariables.Add(parameterVariable);

                    insertWriterStatments.Add(Expression.Assign(parameterVariable, parameterInstance));
                    insertWriterStatments.Add(Expression.Call(stringBuilderParameter, stringBuilderAppend, Expression.Property(parameterVariable, "ParameterName")));
                    insertWriterStatments.Add(Expression.Call(npgsqlParameterCollectionParameter, npgsqlParameterCollectionAdd, parameterVariable));
                    insertWriterStatments.Add(Expression.Call(stringBuilderParameter, stringBuilderAppend, Expression.Constant(", ")));
                }

                columnIndex++;
                propertyFlagValue <<= 1;
            }

            if (primaryColumn is null)
            {
                throw new InvalidOperationException("The EntityBuilder didn't configure the primary key nor is any property named 'Id'.");
            }

            if (TableName is null)
            {
                TableName = Type.Name + "s";
            }

            if (changeTrackingColumns.Count != 0)
            {
                ChangeTrackerFactory = new ChangeTrackerFactory<TEntity>(Type);

                ChangeTrackerFactory.GenerateEntityProxy(changeTrackingColumns);
            }

            insertWriterStatments.RemoveAt(insertWriterStatments.Count - 1);

            InsertWriter = Expression.Lambda<Action<TEntity, StringBuilder, string, NpgsqlParameterCollection>>(Expression.Block(insertWriterVariables, insertWriterStatments), entityParameter, stringBuilderParameter, indexParameter, npgsqlParameterCollectionParameter).Compile();

            return new EntityColumnCollection<TEntity>(columns.ToArray(), nameToColumn, regularColumnsOffset);
        }
    }
}
