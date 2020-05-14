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

        internal string ColumnListString { get; }
        internal string NonPrimaryColumnListString { get; }

        internal Entity(Type entityType, Func<ChangeTracker<TEntity>, TEntity>? changeTrackerFactory, string tableName, EntityColumnCollection<TEntity> columns, PrimaryEntityColumn<TEntity> primaryColumn)
        {
            EntityType = entityType;
            ChangeTrackerFactory = changeTrackerFactory;
            TableName = "\"" + tableName + "\"";
            Columns = columns;
            PrimaryColumn = primaryColumn;

            ColumnListString = GetColumnListString(false);
            NonPrimaryColumnListString = GetColumnListString(true);
        }

        private string GetColumnListString(bool excludePrimaryColumns)
        {
            var sb = new StringBuilder();

            sb.Append("(");

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
            sb.Append(")");

            return sb.ToString();
        }

        internal TEntity GetProxiedEntity(bool trackChanges = false)
        {
            return ChangeTrackerFactory.Invoke(new ChangeTracker<TEntity>(this,trackChanges));
        }
    }

    internal interface IEntity
    {
    }
}
