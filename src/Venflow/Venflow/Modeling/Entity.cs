using System;

namespace Venflow.Modeling
{
    internal class Entity<TEntity> : IEntity where TEntity : class
    {
        internal EntityColumnCollection<TEntity> Columns { get; }
        internal PrimaryEntityColumn<TEntity> PrimaryColumn { get; }

        internal string TableName { get; }
        internal Type EntityType { get; }
        internal Func<ChangeTracker<TEntity>, TEntity>? ChangeTrackerFactory { get; }

        internal Entity(Type entityType, Func<ChangeTracker<TEntity>, TEntity>? changeTrackerFactory, string tableName, EntityColumnCollection<TEntity> columns, PrimaryEntityColumn<TEntity> primaryColumn)
        {
            EntityType = entityType;
            ChangeTrackerFactory = changeTrackerFactory;
            TableName = "\"" + tableName + "\"";
            Columns = columns;
            PrimaryColumn = primaryColumn;
        }

        internal TEntity GetProxiedEntity()
        {
            return ChangeTrackerFactory.Invoke(new ChangeTracker<TEntity>(this));
        }
    }

    internal interface IEntity
    {
    }
}
