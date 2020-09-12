using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Npgsql;
using Venflow.Dynamic.Proxies;
using Venflow.Dynamic.Retriever;
using Venflow.Enums;

namespace Venflow.Modeling.Definitions.Builder
{
    internal class EntityBuilder<TEntity> : EntityBuilder, IEntityBuilder<TEntity> where TEntity : class, new()
    {
        internal bool IsCustomEntity { get; set; }

        internal override Type Type { get; }

        internal ChangeTrackerFactory<TEntity>? ChangeTrackerFactory { get; private set; }
        internal string? TableName { get; private set; }
        internal IDictionary<string, ColumnDefinition<TEntity>> ColumnDefinitions { get; }

        internal bool EntityInNullableContext { get; }
        internal bool DefaultPropNullability { get; }

        private readonly HashSet<string> _ignoredColumns;
        private readonly ValueRetrieverFactory<TEntity> _valueRetrieverFactory;

        internal EntityBuilder(string tableName)
        {
            TableName = tableName;

            Type = typeof(TEntity);
            ColumnDefinitions = new Dictionary<string, ColumnDefinition<TEntity>>();
            _ignoredColumns = new HashSet<string>();
            _valueRetrieverFactory = new ValueRetrieverFactory<TEntity>(Type);
            IsCustomEntity = true;

            // Check if the entity has a NullableContextAttribute which means that it is in a null-able environment.
            var nullableContextAttribute = Type.GetCustomAttribute<NullableContextAttribute>();

            if (nullableContextAttribute is { })
            {
                // Flag == 1 All props are not null-able if not otherwise specified. Flag == 2 reversed.
                DefaultPropNullability = nullableContextAttribute.Flag == 2;
                EntityInNullableContext = true;
            }
            else
            {
                DefaultPropNullability = true;
                EntityInNullableContext = false;
            }
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

        IEntityBuilder<TEntity> IEntityBuilder<TEntity>.MapPostgresEnum<TTarget>(Expression<Func<TEntity, TTarget?>> propertySelector, string? name, INpgsqlNameTranslator? npgsqlNameTranslator)
        {
            var property = propertySelector.ValidatePropertySelector();

            MapPostgresEnum<TTarget>(property, name, npgsqlNameTranslator);

            return this;
        }

        IEntityBuilder<TEntity> IEntityBuilder<TEntity>.MapPostgresEnum<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, string? name, INpgsqlNameTranslator? npgsqlNameTranslator)
        {
            var property = propertySelector.ValidatePropertySelector();

            MapPostgresEnum<TTarget>(property, name, npgsqlNameTranslator);

            return this;
        }

        private void MapPostgresEnum<TTarget>(PropertyInfo property, string? name, INpgsqlNameTranslator? npgsqlNameTranslator)
            where TTarget : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);

                name = underlyingType is { } ? underlyingType.Name : property.PropertyType.Name;

                var nameBuilder = new StringBuilder(name.Length * 2 - 1);

                nameBuilder.Append(char.ToLowerInvariant(name[0]));

                for (int i = 1; i < name.Length; i++)
                {
                    var c = name[i];

                    if (char.IsUpper(c))
                    {
                        nameBuilder.Append('_');
                        nameBuilder.Append(char.ToLowerInvariant(c));
                    }
                    else
                    {
                        nameBuilder.Append(c);
                    }
                }

                name = nameBuilder.ToString();
            }

            if (!PostgreSQLEnums.Contains(name))
            {
                NpgsqlConnection.GlobalTypeMapper.MapEnum<TTarget>(name, npgsqlNameTranslator);

                PostgreSQLEnums.Add(name);
            }

            var columnDefinition = new PostgreEnumColumnDefenition<TEntity>(property.Name);

            ColumnDefinitions.Add(property.Name, columnDefinition);
        }

        INotRequiredSingleRightRelationBuilder<TEntity, TRelation> ILeftRelationBuilder<TEntity>.HasMany<TRelation>(Expression<Func<TEntity, IList<TRelation>>> navigationProperty) where TRelation : class
        {
            var property = navigationProperty.ValidatePropertySelector(false);

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
                throw new TypeArgumentException($"The entity '{Type.Name}' doesn't contain any columns/properties. A entity needs at least one column/property.");
            }

            var filteredProperties = new List<PropertyInfo>();
            PropertyInfo? annotedPrimaryKey = default;

            var notMappedAttributeType = typeof(NotMappedAttribute);

            Type? primaryKeyAttributeType = default;

            if (IsCustomEntity)
            {
                primaryKeyAttributeType = typeof(KeyAttribute);
            }

            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];

                if (property.CanWrite && property.SetMethod!.IsPublic && !_ignoredColumns.Contains(property.Name) && !Attribute.IsDefined(property, notMappedAttributeType))
                {
                    if (IsCustomEntity &&
                        (Attribute.IsDefined(property, primaryKeyAttributeType) ||
                        property.Name == "Id"))
                    {
                        annotedPrimaryKey = property;
                    }

                    filteredProperties.Add(property);
                }
            }

            var columns = new List<EntityColumn<TEntity>>();
            var nameToColumn = new Dictionary<string, EntityColumn<TEntity>>();
            var changeTrackingColumns = new Dictionary<int, EntityColumn<TEntity>>();
            PrimaryEntityColumn<TEntity>? primaryColumn = null;

            // Important column specifications
            var regularColumnsOffset = 0;

            for (int i = 0; i < filteredProperties.Count; i++)
            {
                var property = filteredProperties[i];

                var hasCustomDefinition = false;

                bool isPropertyTypeNullableReferenceType = property.IsNullableReferenceType(EntityInNullableContext, DefaultPropNullability);

                // Handle custom columns

                ColumnDefinition<TEntity>? definition = default;

                if (IsCustomEntity && ColumnDefinitions.TryGetValue(property.Name, out definition))
                {
                    switch (definition)
                    {
                        case PrimaryColumnDefinition<TEntity> primaryDefintion:

                            if (EntityInNullableContext && isPropertyTypeNullableReferenceType)
                            {
                                throw new InvalidOperationException($"The property '{property.Name}' on the entity '{Type.Name}' is marked as null-able. This is not allowed, a primary key always has to be not-null.");
                            }

                            primaryColumn = new PrimaryEntityColumn<TEntity>(property, definition.Name, _valueRetrieverFactory.GenerateRetriever(property, false), primaryDefintion.IsServerSideGenerated);

                            columns.Insert(0, primaryColumn);

                            regularColumnsOffset++;

                            var setMethod = property.GetSetMethod();

                            if (setMethod.IsVirtual && !setMethod.IsFinal)
                            {
                                changeTrackingColumns.Add(i, primaryColumn);
                            }

                            nameToColumn.Add(definition.Name, primaryColumn);

                            hasCustomDefinition = true;
                            break;
                        case PostgreEnumColumnDefenition<TEntity> enumDefinition:

                            var enumColumn = new PostgreEnumEntityColumn<TEntity>(property, definition.Name, _valueRetrieverFactory.GenerateRetriever(property, true));

                            columns.Add(enumColumn);

                            setMethod = property.GetSetMethod();

                            if (setMethod.IsVirtual && !setMethod.IsFinal)
                            {
                                changeTrackingColumns.Add(i, enumColumn);
                            }

                            nameToColumn.Add(definition.Name, enumColumn);

                            hasCustomDefinition = true;
                            break;
                    }
                }
                else if (IsCustomEntity &&
                         annotedPrimaryKey == property)
                {
                    if (EntityInNullableContext &&
                        isPropertyTypeNullableReferenceType)
                    {
                        throw new InvalidOperationException($"The property '{property.Name}' on the entity '{Type.Name}' is marked as null-able. This is not allowed, a primary key always has to be not-null.");
                    }

                    primaryColumn = new PrimaryEntityColumn<TEntity>(property, annotedPrimaryKey.Name, _valueRetrieverFactory.GenerateRetriever(property, false), true);

                    columns.Insert(0, primaryColumn);

                    regularColumnsOffset++;

                    var setMethod = property.GetSetMethod();

                    if (setMethod.IsVirtual &&
                        !setMethod.IsFinal)
                    {
                        changeTrackingColumns.Add(i, primaryColumn);
                    }

                    nameToColumn.Add(annotedPrimaryKey.Name, primaryColumn);

                    hasCustomDefinition = true;
                }

                if (!hasCustomDefinition)
                {
                    string columnName;

                    if (IsCustomEntity &&
                        definition is not null)
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

                    var column = new EntityColumn<TEntity>(property, columnName, _valueRetrieverFactory.GenerateRetriever(property, false), isPropertyTypeNullableReferenceType);

                    columns.Add(column);

                    if (IsCustomEntity)
                    {
                        var setMethod = property.GetSetMethod();

                        if (setMethod.IsVirtual &&
                            !setMethod.IsFinal)
                        {
                            changeTrackingColumns.Add(i, column);
                        }
                    }

                    nameToColumn.Add(columnName, column);
                }
            }

            if (primaryColumn is null && IsCustomEntity)
            {
                throw new InvalidOperationException($"The EntityBuilder couldn't find the primary key on the entity '{Type.Name}', it isn't named 'Id', the KeyAttribute wasn't set nor was any property in the configuration defined as the primary key.");
            }

            if (columns.Count == 0)
            {
                throw new InvalidOperationException($"The entity '{Type.Name}' doesn't contain any columns/mapped properties. A entity needs at least one column/mapped property.");
            }

            if (changeTrackingColumns.Count != 0)
            {
                ChangeTrackerFactory = new ChangeTrackerFactory<TEntity>(Type);

                ChangeTrackerFactory.GenerateEntityProxy(changeTrackingColumns);
            }

            return new EntityColumnCollection<TEntity>(columns.ToArray(), nameToColumn, regularColumnsOffset);
        }
    }

    internal abstract class EntityBuilder
    {
        internal static uint RelationCounter { get; set; }

        internal static ConcurrentBag<string> PostgreSQLEnums { get; }

        static EntityBuilder()
        {
            PostgreSQLEnums = new ConcurrentBag<string>();
        }

        internal List<EntityRelationDefinition> Relations { get; }
        internal abstract Type Type { get; }

        protected EntityBuilder()
        {
            Relations = new List<EntityRelationDefinition>();
        }

        internal abstract void IgnoreProperty(string propertyName);
    }

    /// <summary>
    /// Instances of this class are returned from methods inside the <see cref="Table{TEntity}"/> class when using the Fluid API and it is not designed to be directly constructed in your application code.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    public interface IEntityBuilder<TEntity> : ILeftRelationBuilder<TEntity> where TEntity : class, new()
    {
        /// <summary>
        /// Configures the table that the entity type maps to, if not configured it will use the name of the <see cref="Table{TEntity}"/> property inside the <see cref="Database"/> class.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        IEntityBuilder<TEntity> MapToTable(string tableName);

        /// <summary>
        /// Configures the column that the property maps to, if not configured it will use the name of the property inside the entity.
        /// </summary>
        /// <typeparam name="TTarget">The type of the property.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the property on this entity type.</param>
        /// <param name="columnName">The name of the column in the database to which the used property should map to.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        IEntityBuilder<TEntity> MapColumn<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, string columnName);

        /// <summary>
        /// Ignores a property for this entity type. This is the Fluent API equivalent to the <see cref="NotMappedAttribute"/>.
        /// </summary>
        /// <typeparam name="TTarget">The type of the property.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the property on this entity type.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        IEntityBuilder<TEntity> Ignore<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector);

        /// <summary>
        /// Sets the property that defines the primary key for this entity type. This is the Fluent API equivalent to the <see cref="KeyAttribute"/>.
        /// </summary>
        /// <typeparam name="TTarget">The type of the primary key.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the primary key on this entity type.</param>
        /// <param name="option">The option which define how the primary key is generate.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        IEntityBuilder<TEntity> MapId<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, DatabaseGeneratedOption option);

        /// <summary>
        /// Maps a PostgreSQL enum to a CLR enum.
        /// </summary>
        /// <typeparam name="TTarget">The type of the enum.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the enum which should be mapped on this entity type.</param>
        /// <param name="name">The name of the enum in PostgreSQL, if none used it will try to convert the name of the CLR enum e.g. 'FooBar' to 'foo_bar'</param>
        /// <param name="npgsqlNameTranslator">A component which will be used to translate CLR names (e.g. SomeClass) into database names (e.g. some_class). Defaults to <see cref="Npgsql.NameTranslation.NpgsqlSnakeCaseNameTranslator"/>.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        IEntityBuilder<TEntity> MapPostgresEnum<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, string? name = default, INpgsqlNameTranslator? npgsqlNameTranslator = default)
            where TTarget : struct, Enum;

        /// <summary>
        /// Maps a PostgreSQL enum to a CLR enum.
        /// </summary>
        /// <typeparam name="TTarget">The type of the enum.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the enum which should be mapped on this entity type.</param>
        /// <param name="name">The name of the enum in PostgreSQL, if none used it will try to convert the name of the CLR enum e.g. 'FooBar' to 'foo_bar'</param>
        /// <param name="npgsqlNameTranslator">A component which will be used to translate CLR names (e.g. SomeClass) into database names (e.g. some_class). Defaults to <see cref="Npgsql.NameTranslation.NpgsqlSnakeCaseNameTranslator"/>.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        IEntityBuilder<TEntity> MapPostgresEnum<TTarget>(Expression<Func<TEntity, TTarget?>> propertySelector, string? name = default, INpgsqlNameTranslator? npgsqlNameTranslator = default)
            where TTarget : struct, Enum;
    }
}
