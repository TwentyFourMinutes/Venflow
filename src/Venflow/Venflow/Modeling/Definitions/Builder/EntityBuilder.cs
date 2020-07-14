using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Venflow.Dynamic.Proxies;
using Venflow.Enums;

namespace Venflow.Modeling.Definitions.Builder
{
    internal class EntityBuilder<TEntity> : EntityBuilder, IEntityBuilder<TEntity> where TEntity : class
    {
        internal override Type Type { get; }

        internal ChangeTrackerFactory<TEntity>? ChangeTrackerFactory { get; private set; }
        internal string? TableName { get; private set; }
        internal IDictionary<string, ColumnDefinition<TEntity>> ColumnDefinitions { get; }

        private readonly HashSet<string> _ignoredColumns;

        internal EntityBuilder(string tableName)
        {
            TableName = tableName;

            Type = typeof(TEntity);
            ColumnDefinitions = new Dictionary<string, ColumnDefinition<TEntity>>();
            _ignoredColumns = new HashSet<string>();
        }

        IEntityBuilder<TEntity> IEntityBuilder<TEntity>.MapToTable(string tableName)
        {
            TableName = tableName;

            return this;
        }

        IEntityBuilder<TEntity> IEntityBuilder<TEntity>.MapColumn<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, string columnName)
        {
            var property = propertySelector.ValidatePropertySelector();

            if (ColumnDefinitions.TryGetValue(property.Name, out var definition))
            {
                definition.Name = columnName;
            }
            else
            {
                definition = new ColumnDefinition<TEntity>(columnName);

                ColumnDefinitions.Add(property.Name, definition);
            }

            return this;
        }

        IEntityBuilder<TEntity> IEntityBuilder<TEntity>.Ignore<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector)
        {
            var property = propertySelector.ValidatePropertySelector();

            _ignoredColumns.Add(property.Name);

            return this;
        }

        IEntityBuilder<TEntity> IEntityBuilder<TEntity>.MapId<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, DatabaseGeneratedOption option)
        {
            var property = propertySelector.ValidatePropertySelector();

            var isServerSideGenerated = option != DatabaseGeneratedOption.None;

            var columnDefinition = new PrimaryColumnDefinition<TEntity>(property.Name)
            {
                IsServerSideGenerated = isServerSideGenerated
            };

            if (ColumnDefinitions.TryGetValue(property.Name, out var definition))
            {
                columnDefinition.Name = definition.Name;

                ColumnDefinitions.Remove(property.Name);
            }

            ColumnDefinitions.Add(property.Name, columnDefinition);

            return this;
        }

        INotRequiredSingleRightRelationBuilder<TEntity, TRelation> ILeftRelationBuilder<TEntity>.HasMany<TRelation>(Expression<Func<TEntity, IList<TRelation>>> navigationProperty) where TRelation : class
        {
            var property = navigationProperty.ValidatePropertySelector();

            IgnoreProperty(property.Name);

            return new RightRelationBuilder<TEntity, TRelation>(RelationPartType.Many, property, this);
        }

        IRequiredSingleRightRelationBuilder<TEntity, TRelation> ILeftRelationBuilder<TEntity>.HasMany<TRelation>()
        {
            return new RightRelationBuilder<TEntity, TRelation>(RelationPartType.Many, null, this);
        }

        INotRequiredMultiRightRelationBuilder<TEntity, TRelation> ILeftRelationBuilder<TEntity>.HasOne<TRelation>(Expression<Func<TEntity, TRelation>> navigationProperty) where TRelation : class
        {
            var property = navigationProperty.ValidatePropertySelector();

            IgnoreProperty(property.Name);
            return new RightRelationBuilder<TEntity, TRelation>(RelationPartType.One, property, this);
        }

        IRequiredMultiRightRelationBuilder<TEntity, TRelation> ILeftRelationBuilder<TEntity>.HasOne<TRelation>()
        {
            return new RightRelationBuilder<TEntity, TRelation>(RelationPartType.One, null, this);
        }

        internal override void IgnoreProperty(string propertyName)
            => _ignoredColumns.Add(propertyName);

        internal EntityColumnCollection<TEntity> Build()
        {
            var properties = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (properties is null || properties.Length == 0)
            {
                throw new TypeArgumentException("The provided generic type argument doesn't contain any public properties with a getter and a setter.");
            }

            var filteredProperties = new List<PropertyInfo>();
            PropertyInfo? annotedPrimaryKey = default;

            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];

                if (property.CanWrite && property.SetMethod!.IsPublic && !_ignoredColumns.Contains(property.Name) && !Attribute.IsDefined(property, TypeCache.NotMappedAttribute))
                {
                    if (Attribute.IsDefined(property, TypeCache.KeyAttribute) || property.Name == "Id")
                    {
                        annotedPrimaryKey = property;
                    }

                    filteredProperties.Add(property);
                }
            }

            // ExpressionVariables

            var columns = new List<EntityColumn<TEntity>>();
            var nameToColumn = new Dictionary<string, EntityColumn<TEntity>>();
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

            for (int i = 0; i < filteredProperties.Count; i++)
            {
                var property = filteredProperties[i];

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

                if (ColumnDefinitions.TryGetValue(property.Name, out var definition))
                {
                    switch (definition)
                    {
                        case PrimaryColumnDefinition<TEntity> primaryDefintion:
                            primaryColumn = new PrimaryEntityColumn<TEntity>(property, definition.Name, valueRetriever, valueWriter, parameterValueRetriever, primaryDefintion.IsServerSideGenerated);

                            columns.Insert(0, primaryColumn);

                            regularColumnsOffset++;

                            var setMethod = property.GetSetMethod();

                            if (setMethod.IsVirtual && !setMethod.IsFinal)
                            {
                                changeTrackingColumns.Add(columnIndex + 1, primaryColumn);
                            }

                            nameToColumn.Add(definition.Name, primaryColumn);

                            hasCustomDefinition = true;
                            isPrimaryColumn = true;
                            break;
                    }
                }
                else if (annotedPrimaryKey == property)
                {
                    primaryColumn = new PrimaryEntityColumn<TEntity>(property, annotedPrimaryKey.Name, valueRetriever, valueWriter, parameterValueRetriever, true);

                    columns.Insert(0, primaryColumn);

                    regularColumnsOffset++;

                    var setMethod = property.GetSetMethod();

                    if (setMethod.IsVirtual && !setMethod.IsFinal)
                    {
                        changeTrackingColumns.Add(columnIndex + 1, primaryColumn);
                    }

                    nameToColumn.Add(annotedPrimaryKey.Name, primaryColumn);

                    hasCustomDefinition = true;
                    isPrimaryColumn = true;
                }

                if (!hasCustomDefinition)
                {
                    string columnName;

                    if (definition is not null)
                    {
                        columnName = definition.Name;

                        var relation = Relations.FirstOrDefault(x => x.ForeignKeyColumnName == property.Name);

                        if (relation is not null)
                        {
                            relation.ForeignKeyColumnName = columnName;
                        }
                    }
                    else
                    {
                        columnName = property.Name;
                    }

                    var column = new EntityColumn<TEntity>(property, columnName, valueRetriever, valueWriter, parameterValueRetriever);

                    columns.Add(column);

                    var setMethod = property.GetSetMethod();

                    if (setMethod.IsVirtual && !setMethod.IsFinal)
                    {
                        changeTrackingColumns.Add(columnIndex + 1, column);
                    }

                    nameToColumn.Add(columnName, column);
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
            }

            if (primaryColumn is null)
            {
                throw new InvalidOperationException("The EntityBuilder couldn't find the primary key, it isn't named 'Id', the KeyAttribute wasn't set nor was any property in the configuration defined as the primary key.");
            }

            if (changeTrackingColumns.Count != 0)
            {
                ChangeTrackerFactory = new ChangeTrackerFactory<TEntity>(Type);

                ChangeTrackerFactory.GenerateEntityProxy(changeTrackingColumns);
            }

            insertWriterStatments.RemoveAt(insertWriterStatments.Count - 1);

            return new EntityColumnCollection<TEntity>(columns.ToArray(), nameToColumn, regularColumnsOffset);
        }
    }

    internal abstract class EntityBuilder
    {
        internal List<EntityRelationDefinition> Relations { get; }
        internal abstract Type Type { get; }

        internal static uint RelationCounter { get; set; }

        protected EntityBuilder()
        {
            Relations = new List<EntityRelationDefinition>();
        }

        internal abstract void IgnoreProperty(string propertyName);
    }

    public interface IEntityBuilder<TEntity> : ILeftRelationBuilder<TEntity> where TEntity : class
    {
        IEntityBuilder<TEntity> MapToTable(string tableName);

        IEntityBuilder<TEntity> MapColumn<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, string columnName);

        IEntityBuilder<TEntity> Ignore<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector);

        IEntityBuilder<TEntity> MapId<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, DatabaseGeneratedOption option);
    }
}
