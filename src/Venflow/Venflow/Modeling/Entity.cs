using Npgsql;
using System;
using System.Text;

namespace Venflow.Modeling
{
    internal class Entity<TEntity> : IEntity where TEntity : class
    {
        string IEntity.EntityName => EntityType.Name;

        internal EntityColumnCollection<TEntity> Columns { get; }
        internal PrimaryEntityColumn<TEntity> PrimaryColumn { get; }

        internal string TableName { get; }
        internal Type EntityType { get; }
        internal Type? ProxyEntityType { get; }
        internal Func<ChangeTracker<TEntity>, TEntity>? ChangeTrackerFactory { get; }
        internal Func<ChangeTracker<TEntity>, TEntity, TEntity>? ChangeTrackerApplier { get; }
        internal Action<TEntity, StringBuilder, string, NpgsqlParameterCollection> InsertWriter { get; }

        internal QueryCommandCache<TEntity> QueryCommandCache { get; }

        internal string ColumnListString { get; }
        internal string NonPrimaryColumnListString { get; }

        internal Entity(Type entityType, Type? proxyEntityType, string tableName, EntityColumnCollection<TEntity> columns, PrimaryEntityColumn<TEntity> primaryColumn, string columnListString, string nonPrimaryColumnListString, Action<TEntity, StringBuilder, string, NpgsqlParameterCollection> insertWriter, Func<ChangeTracker<TEntity>, TEntity>? changeTrackerFactory, Func<ChangeTracker<TEntity>, TEntity, TEntity>? changeTrackerApplier)
        {
            EntityType = entityType;
            TableName = "\"" + tableName + "\"";
            InsertWriter = insertWriter;
            ChangeTrackerFactory = changeTrackerFactory;
            ChangeTrackerApplier = changeTrackerApplier;
            Columns = columns;
            PrimaryColumn = primaryColumn;
            ColumnListString = columnListString;
            NonPrimaryColumnListString = nonPrimaryColumnListString;

            QueryCommandCache = new QueryCommandCache<TEntity>(entityType, proxyEntityType, Columns);
        }

        internal TEntity GetProxiedEntity(bool trackChanges = false)
        {
            return ChangeTrackerFactory.Invoke(new ChangeTracker<TEntity>(Columns.Count, trackChanges));
        }

        internal TEntity ApplyChangeTracking(TEntity entity)
        {
            return ChangeTrackerApplier.Invoke(new ChangeTracker<TEntity>(Columns.Count, false), entity);
        }
    }

    internal interface IEntity
    {
        string EntityName { get; }
    }
}
