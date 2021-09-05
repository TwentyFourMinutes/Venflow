using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using Venflow.Dynamic;
using Venflow.Dynamic.Proxies;
using Venflow.Dynamic.Retriever;
using Venflow.Enums;

namespace Venflow.Modeling.Definitions.Builder
{
    internal class EntityBuilder<TEntity> : EntityBuilder, IEntityBuilder<TEntity> where TEntity : class, new()
    {
        internal bool IsRegularEntity { get; set; }

        internal override Type Type { get; }

        internal ChangeTrackerFactory<TEntity>? ChangeTrackerFactory { get; private set; }
        internal string TableName { get; private set; }
        internal IDictionary<string, ColumnDefinition> ColumnDefinitions { get; }

        internal bool EntityInNullableContext { get; }
        internal bool DefaultPropNullability { get; }

        private readonly ValueRetrieverFactory<TEntity> _valueRetrieverFactory;

        internal EntityBuilder(string tableName)
        {
            TableName = tableName;

            Type = typeof(TEntity);
            ColumnDefinitions = new Dictionary<string, ColumnDefinition>();
            _valueRetrieverFactory = new ValueRetrieverFactory<TEntity>(Type);
            IsRegularEntity = true;

            // Check if the entity has a NullableContextAttribute which means that it is in a null-able environment.
            var nullableContextAttribute = Type.GetCustomAttribute<NullableContextAttribute>();

            if (nullableContextAttribute is not null)
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

            DiscorverColumns();
        }

        IEntityBuilder<TEntity> IEntityBuilder<TEntity>.MapToTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException($"The table name '{tableName}' is invalid.", nameof(tableName));
            }

            TableName = tableName;

            return this;
        }

        IEntityBuilder<TEntity> IEntityBuilder<TEntity>.MapColumn<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException($"The column name '{columnName}' is invalid.", nameof(columnName));
            }

            var property = propertySelector.ValidatePropertySelector();

            ColumnDefinitions[property.Name].Name = columnName;

            return this;
        }

        IEntityBuilder<TEntity> IEntityBuilder<TEntity>.MapColumn<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, NpgsqlDbType dbType)
        {
            var property = propertySelector.ValidatePropertySelector();

            ColumnDefinitions[property.Name].DbType = dbType;

            return this;
        }

        IEntityBuilder<TEntity> IEntityBuilder<TEntity>.MapColumn<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, string columnName, NpgsqlDbType dbType)
        {
            var property = propertySelector.ValidatePropertySelector();

            ColumnDefinitions[property.Name].DbType = dbType;

            return this;
        }

        IEntityBuilder<TEntity> IEntityBuilder<TEntity>.Ignore<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector)
        {
            var property = propertySelector.ValidatePropertySelector();

            IgnoreProperty(property.Name);

            return this;
        }

        IEntityBuilder<TEntity> IEntityBuilder<TEntity>.MapId<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, DatabaseGeneratedOption option)
        {
            var property = propertySelector.ValidatePropertySelector();

            var definition = ColumnDefinitions[property.Name];

            definition.Options |= ColumnOptions.PrimaryKey;

            if (option != DatabaseGeneratedOption.None)
            {
                definition.Options |= ColumnOptions.Generated;
            }
            else
            {
                definition.Options &= ~ColumnOptions.Generated;
            }

            return this;
        }

        [Obsolete("This method will be removed in the next major version. Please instead use the DatabaseConfigurationOptionsBuilder.RegisterPostgresEnum method on the Database.Configure method.")]
        IEntityBuilder<TEntity> IEntityBuilder<TEntity>.MapPostgresEnum<TTarget>(Expression<Func<TEntity, TTarget?>> propertySelector, string? name, INpgsqlNameTranslator? npgsqlNameTranslator)
        {
            var property = propertySelector.ValidatePropertySelector();

            MapPostgresEnum<TTarget>(property, name, npgsqlNameTranslator);

            return this;
        }

        [Obsolete("This method will be removed in the next major version. Please instead use the DatabaseConfigurationOptionsBuilder.RegisterPostgresEnum method on the Database.Configure method.")]
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

                name = underlyingType is not null ? underlyingType.Name : property.PropertyType.Name;

                var nameBuilder = new StringBuilder(name.Length * 2 - 1);

                nameBuilder.Append(char.ToLowerInvariant(name[0]));

                var nameSpan = name.AsSpan();

                for (int i = 1; i < nameSpan.Length; i++)
                {
                    var c = nameSpan[i];

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

            if (!ParameterTypeHandler.PostgreEnums.Contains(property.PropertyType))
            {
                NpgsqlConnection.GlobalTypeMapper.MapEnum<TTarget>(name, npgsqlNameTranslator);

                ParameterTypeHandler.PostgreEnums.Add(property.PropertyType);
            }

            ColumnDefinitions[property.Name].Options |= ColumnOptions.PostgreEnum;
        }

        INotRequiredSingleRightRelationBuilder<TEntity, TRelation> ILeftRelationBuilder<TEntity>.HasMany<TRelation>(Expression<Func<TEntity, IList<TRelation>>> navigationProperty) where TRelation : class
        {
            var property = navigationProperty.ValidatePropertySelector(false);

            IgnoreProperty(property.Name);

            if (!property.CanWrite &&
                property.GetBackingField() is null)
            {
                throw new InvalidOperationException($"The foreign property '{property.Name}' on the entity '{property.ReflectedType.Name}' doesn't implement a setter, nor does it match the common backing field pattern ('<{property.Name}>k__BackingField', '{char.ToLowerInvariant(property.Name[0])}{property.Name.Substring(1)}' or  '_{char.ToLowerInvariant(property.Name[0])}{property.Name.Substring(1)}').");
            }

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

            if (!property.CanWrite &&
                property.GetBackingField() is null)
            {
                throw new InvalidOperationException($"The foreign property '{property.Name}' on the entity '{property.ReflectedType.Name}' doesn't implement a setter, nor does it match the common backing field pattern ('<{property.Name}>k__BackingField', '{char.ToLowerInvariant(property.Name[0])}{property.Name.Substring(1)}' or  '_{char.ToLowerInvariant(property.Name[0])}{property.Name.Substring(1)}').");
            }

            return new RightRelationBuilder<TEntity, TRelation>(RelationPartType.One, property, this);
        }

        IRequiredMultiRightRelationBuilder<TEntity, TRelation> ILeftRelationBuilder<TEntity>.HasOne<TRelation>()
        {
            return new RightRelationBuilder<TEntity, TRelation>(RelationPartType.One, null, this);
        }

        internal override void IgnoreProperty(string propertyName)
            => ColumnDefinitions.Remove(propertyName);

        internal EntityColumnCollection<TEntity> Build()
        {
            var columns = new LinkedList<EntityColumn<TEntity>>();
            var nameToColumn = new Dictionary<string, EntityColumn<TEntity>>();
            var changeTrackingColumns = new Dictionary<int, EntityColumn<TEntity>>();

            LinkedListNode<EntityColumn<TEntity>>? firstRegularNode = default;
            LinkedListNode<EntityColumn<TEntity>>? firstReadOnlyNode = default;

            var columnIndex = 0;
            var regularColumnsOffset = 0;
            var readOnlyCount = 0;
            var lastRegularColumnsIndex = 0;

            foreach (var columnDefinition in ColumnDefinitions.Values)
            {
                var property = columnDefinition.Property;
                var isPropertyTypeNullableReferenceType = property.IsNullableReferenceType(EntityInNullableContext, DefaultPropNullability);

                if (isPropertyTypeNullableReferenceType)
                {
                    columnDefinition.Options |= ColumnOptions.NullableReferenceType;
                }

                var setMethod = property.GetSetMethod();
                var isReadOnly = setMethod is null;

                if (isReadOnly)
                {
                    columnDefinition.Options |= ColumnOptions.ReadOnly;
                }

                var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                if (type.IsEnum && ParameterTypeHandler.PostgreEnums.Contains(type))
                {
                    columnDefinition.Options |= ColumnOptions.PostgreEnum;
                }

                var expectsChangeTracking = IsRegularEntity && !isReadOnly && setMethod.IsVirtual && !setMethod.IsFinal;

                string? columnName = null;

                if (IsRegularEntity)
                {
                    if (columnDefinition.Options.HasFlag(ColumnOptions.PrimaryKey))
                    {
                        if (EntityInNullableContext &&
                            isPropertyTypeNullableReferenceType)
                        {
                            throw new InvalidOperationException($"The property '{property.Name}' on the entity '{Type.Name}' is marked as null-able. This is not allowed, a primary key always has to be not-null.");
                        }

                        if (isReadOnly &&
                            !columnDefinition.Options.HasFlag(ColumnOptions.Generated))
                        {
                            throw new InvalidOperationException($"The property '{property.Name}' on the entity '{Type.Name}' is marked as read-only. This is not allowed on non database generated primary key.");
                        }

                        regularColumnsOffset++;
                    }
                    else
                    {
                        var relation = Relations.FirstOrDefault(x => x.ForeignKeyColumnName == property.Name);

                        if (relation is not null)
                        {
                            columnName = columnDefinition.Name;

                            relation.ForeignKeyColumnName = columnName;
                        }
                    }
                }

                if (columnName is null)
                {
                    columnName = columnDefinition.Name;
                }

                var column = new EntityColumn<TEntity>(property, columnName, _valueRetrieverFactory.GenerateRetriever(columnDefinition), columnDefinition.DbType, columnDefinition.Options);

                if (expectsChangeTracking)
                {
                    changeTrackingColumns.Add(columnIndex, column);
                }

                nameToColumn.Add(columnName, column);

                if (columnDefinition.Options.HasFlag(ColumnOptions.PrimaryKey))
                {
                    if (firstRegularNode is null)
                    {
                        if (firstReadOnlyNode is null)
                        {
                            columns.AddLast(column);
                        }
                        else
                        {
                            columns.AddBefore(firstReadOnlyNode, column);
                        }
                    }
                    else
                    {
                        columns.AddBefore(firstRegularNode, column);
                    }
                }
                else if (columnDefinition.Options.HasFlag(ColumnOptions.ReadOnly))
                {
                    if (firstReadOnlyNode is null)
                    {
                        firstReadOnlyNode = columns.AddLast(column);
                    }
                    else
                    {
                        columns.AddLast(column);
                    }

                    readOnlyCount++;
                }
                else
                {
                    if (firstRegularNode is null)
                    {
                        if (firstReadOnlyNode is null)
                        {
                            firstRegularNode = columns.AddLast(column);
                        }
                        else
                        {
                            firstRegularNode = columns.AddBefore(firstReadOnlyNode, column);
                        }
                    }
                    else
                    {
                        if (firstReadOnlyNode is null)
                        {
                            columns.AddLast(column);
                        }
                        else
                        {
                            columns.AddBefore(firstReadOnlyNode, column);
                        }
                    }
                }

                columnIndex++;
            }

            lastRegularColumnsIndex = columns.Count - 1 - readOnlyCount;

            if (regularColumnsOffset == 0 &&
                IsRegularEntity)
            {
                throw new InvalidOperationException($"The EntityBuilder couldn't find the primary key on the entity '{Type.Name}', it isn't named 'Id', the KeyAttribute wasn't set nor was any property in the configuration defined as the primary key.");
            }

            if (columns.Count == 0)
            {
                throw new InvalidOperationException($"The entity '{Type.Name}' doesn't contain any columns/mapped properties. An entity needs at least one column/mapped property.");
            }

            if (regularColumnsOffset == columns.Count)
            {
                throw new InvalidOperationException($"The entity '{Type.Name}' doesn't contain any non-primary/non-database generated columns/mapped properties. An entity needs at least one non-primary/non-database generated column/mapped property.");
            }

            if (readOnlyCount == columns.Count + regularColumnsOffset)
            {
                throw new InvalidOperationException($"The entity '{Type.Name}' doesn't contain any non-read-only properties. An entity needs at least one non-read-only property.");
            }

            if (changeTrackingColumns.Count != 0)
            {
                if (Type.GetConstructor(Type.EmptyTypes) is null)
                    throw new InvalidOperationException($"The entity '{Type.Name}' contains virtual properties. However the entity doesn't contain a public parameterless constructor and therefore can't create a proxy.");

                ChangeTrackerFactory = new ChangeTrackerFactory<TEntity>(Type);

                ChangeTrackerFactory.GenerateEntityProxy(changeTrackingColumns);
            }

            return new EntityColumnCollection<TEntity>(columns.ToArray(), nameToColumn, regularColumnsOffset, lastRegularColumnsIndex, readOnlyCount, changeTrackingColumns.Count);
        }

        private void DiscorverColumns()
        {
            var properties = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (properties is null || properties.Length == 0)
            {
                throw new TypeArgumentException($"The entity '{Type.Name}' doesn't contain any columns/properties. An entity needs at least one column/property.");
            }

            var notMappedAttributeType = typeof(NotMappedAttribute);
            Type? primaryKeyAttributeType = default;

            if (IsRegularEntity)
            {
                primaryKeyAttributeType = typeof(KeyAttribute);
            }

            var propertiesSpan = properties.AsSpan();

            for (int i = 0; i < propertiesSpan.Length; i++)
            {
                var property = propertiesSpan[i];

                var hasPropertySetter = property.GetSetMethod() is not null;
                var hasPropertyBackingField = property.GetBackingField() is not null;

                if (((property.CanWrite && property.SetMethod!.IsPublic) ||
                    (!hasPropertySetter && hasPropertyBackingField)) &&
                    !Attribute.IsDefined(property, notMappedAttributeType))
                {
                    var column = new ColumnDefinition(property);

                    if (IsRegularEntity &&
                        (Attribute.IsDefined(property, primaryKeyAttributeType) ||
                        property.Name == "Id"))
                    {
                        column.Options |= ColumnOptions.PrimaryKey | ColumnOptions.Generated;
                    }

                    ColumnDefinitions.Add(property.Name, column);
                }
            }
        }
    }

    internal abstract class EntityBuilder
    {
        internal static uint RelationCounter { get; set; }

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
        /// Configures the column that the property maps to, if not configured it will use the name of the property inside the entity.
        /// </summary>
        /// <typeparam name="TTarget">The type of the property.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the property on this entity type.</param>
        /// <param name="dbType">The type of the column in the database.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        IEntityBuilder<TEntity> MapColumn<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, NpgsqlDbType dbType);

        /// <summary>
        /// Configures the column that the property maps to, if not configured it will use the name of the property inside the entity.
        /// </summary>
        /// <typeparam name="TTarget">The type of the property.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the property on this entity type.</param>
        /// <param name="columnName">The name of the column in the database to which the used property should map to.</param>
        /// <param name="dbType">The type of the column in the database.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        IEntityBuilder<TEntity> MapColumn<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, string columnName, NpgsqlDbType dbType);

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
        [Obsolete("This method will be removed in the next major version. Please instead use the DatabaseConfigurationOptionsBuilder.RegisterPostgresEnum method on the Database.Configure method.")]
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
        [Obsolete("This method will be removed in the next major version. Please instead use the DatabaseConfigurationOptionsBuilder.RegisterPostgresEnum method on the Database.Configure method.")]
        IEntityBuilder<TEntity> MapPostgresEnum<TTarget>(Expression<Func<TEntity, TTarget?>> propertySelector, string? name = default, INpgsqlNameTranslator? npgsqlNameTranslator = default)
            where TTarget : struct, Enum;
    }
}
