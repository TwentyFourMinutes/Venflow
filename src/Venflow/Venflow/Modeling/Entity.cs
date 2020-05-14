using System;
using System.Text;

namespace Venflow.Modeling
{
    internal class Entity<TEntity> : IEntity where TEntity : class
    {
        internal EntityColumnCollection<TEntity> Columns { get; }
        internal PrimaryEntityColumn<TEntity> PrimaryColumn { get; }

        internal string TableName { get; }
        internal Type EntityType { get; }
        internal Func<ChangeTracker<TEntity>, TEntity>? ChangeTrackerFactory { get; }
        internal Func<ChangeTracker<TEntity>, TEntity, TEntity>? ChangeTrackerApplier { get; }

        internal string ColumnListString { get; }
        internal string NonPrimaryColumnListString { get; }

        internal Entity(Type entityType, Func<ChangeTracker<TEntity>, TEntity>? changeTrackerFactory, Func<ChangeTracker<TEntity>, TEntity, TEntity>? changeTrackerApplier, string tableName, EntityColumnCollection<TEntity> columns, PrimaryEntityColumn<TEntity> primaryColumn)
        {
            EntityType = entityType;
            ChangeTrackerFactory = changeTrackerFactory;
            ChangeTrackerApplier = changeTrackerApplier;
            TableName = "\"" + tableName + "\"";
            Columns = columns;
            PrimaryColumn = primaryColumn;

            ColumnListString = GetColumnListString(false);
            NonPrimaryColumnListString = GetColumnListString(true);
        }

        private string GetColumnListString(bool excludePrimaryColumns)
        {
            var sb = new StringBuilder();

            foreach (var column in Columns.Values)
            {
                if (excludePrimaryColumns && column is PrimaryEntityColumn<TEntity>)
                {
                    continue;
                }

                sb.Append('"');
                sb.Append(column.ColumnName);
                sb.Append("\",");
            }

            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        internal TEntity GetProxiedEntity(bool trackChanges = false)
        {
            return ChangeTrackerFactory.Invoke(new ChangeTracker<TEntity>(this, trackChanges));
        }

        internal TEntity ApplyChangeTracking(TEntity entity)
        {
            return ChangeTrackerApplier.Invoke(new ChangeTracker<TEntity>(this, false), entity);
        }
    }

    internal interface IEntity
    {
    }
}
