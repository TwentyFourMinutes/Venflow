using System;
using Venflow.Dynamic.Inserter;
using Venflow.Dynamic.Materializer;
using Venflow.Dynamic.Proxies;

namespace Venflow.Modeling
{
    internal class Entity<TEntity> : Entity where TEntity : class, new()
    {
        internal EntityColumnCollection<TEntity> Columns { get; }
        internal PrimaryEntityColumn<TEntity>? PrimaryColumn { get; }

        internal Func<ChangeTracker<TEntity>, TEntity>? ChangeTrackerFactory { get; }
        internal Func<ChangeTracker<TEntity>, TEntity, TEntity>? ChangeTrackerApplier { get; }

        internal MaterializerFactory<TEntity> MaterializerFactory { get; }
        internal InsertionFactory<TEntity> InsertionFactory { get; }

        internal override bool HasDbGeneratedPrimaryKey => PrimaryColumn.IsServerSideGenerated;

        internal Entity(Type entityType, Type? proxyEntityType, string tableName, bool isInNullableContext, bool defaultPropNullability, EntityColumnCollection<TEntity> columns, PrimaryEntityColumn<TEntity>? primaryColumn, string columnListString, string nonPrimaryColumnListString, Func<ChangeTracker<TEntity>, TEntity>? changeTrackerFactory, Func<ChangeTracker<TEntity>, TEntity, TEntity>? changeTrackerApplier) : base(entityType, proxyEntityType, tableName, isInNullableContext, defaultPropNullability, columnListString, nonPrimaryColumnListString)
        {
            ChangeTrackerFactory = changeTrackerFactory;
            ChangeTrackerApplier = changeTrackerApplier;
            Columns = columns;
            PrimaryColumn = primaryColumn;

            MaterializerFactory = new MaterializerFactory<TEntity>(this);
            InsertionFactory = new InsertionFactory<TEntity>(this);
        }

        internal TEntity ApplyChangeTracking(TEntity entity)
        {
            if (ChangeTrackerApplier is null)
            {
                throw new InvalidOperationException($"The entity {EntityType.Name} doesn't contain any properties which are marked as virtual. Therefor no proxy entity exists.");
            }

            return ChangeTrackerApplier.Invoke(new ChangeTracker<TEntity>(Columns.Count, false), entity);
        }

        internal override EntityColumn GetPrimaryColumn()
        {
            return PrimaryColumn;
        }

        internal override EntityColumn GetColumn(int index)
        {
            return Columns[index];
        }

        internal override EntityColumn GetColumn(string columnName)
        {
            return Columns[columnName];
        }

        internal override bool TryGetColumn(string columnName, out EntityColumn? entityColumn)
        {
            if (Columns.TryGetValue(columnName, out var tempColumn))
            {
                entityColumn = tempColumn;

                return true;
            }
            else
            {
                entityColumn = null;

                return false;
            }
        }

        internal override int GetColumnCount()
        {
            return Columns.Count;
        }

        internal override int GetRegularColumnOffset()
        {
            return Columns.RegularColumnsOffset;
        }
    }

    internal abstract class Entity
    {
        internal string EntityName { get; }
        internal string TableName { get; }

        internal bool IsInNullableContext { get; }
        internal bool DefaultPropNullability { get; }

        internal abstract bool HasDbGeneratedPrimaryKey { get; }

        internal Type EntityType { get; }
        internal Type? ProxyEntityType { get; }

        internal TrioKeyCollection<uint, string, EntityRelation>? Relations { get; set; }

        internal string ColumnListString { get; }
        internal string NonPrimaryColumnListString { get; }

        protected Entity(Type entityType, Type? proxyEntityType, string tableName, bool isInNullableContext, bool defaultPropNullability, string columnListString, string nonPrimaryColumnListString)
        {
            EntityType = entityType;
            ProxyEntityType = proxyEntityType;
            EntityName = entityType.Name;
            TableName = "\"" + tableName + "\"";
            IsInNullableContext = isInNullableContext;
            DefaultPropNullability = defaultPropNullability;
            ColumnListString = columnListString;
            NonPrimaryColumnListString = nonPrimaryColumnListString;
        }

        internal abstract EntityColumn GetPrimaryColumn();
        internal abstract int GetColumnCount();
        internal abstract int GetRegularColumnOffset();
        internal abstract EntityColumn GetColumn(int index);
        internal abstract EntityColumn GetColumn(string columnName);
        internal abstract bool TryGetColumn(string columnName, out EntityColumn? entityColumn);
    }
}
