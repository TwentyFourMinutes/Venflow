using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Venflow.Modeling.ProxyTypes")]

namespace Venflow.Modeling
{
    internal class ChangeTracker<TEntity> where TEntity : class
    {
        internal bool TrackChanges { get; set; }
        internal bool IsDirty { get; private set; }

        private EntityColumn<TEntity>?[] _changedColumns;

        private readonly Entity<TEntity> _entity;

        internal ChangeTracker(Entity<TEntity> entity, bool trackChanges)
        {
            _entity = entity;
            TrackChanges = trackChanges;
            _changedColumns = null!;
        }

        internal void MakeDirty(byte propertyIndex)
        {
            if (!TrackChanges)
                 return;

            if (!IsDirty)
            {
                _changedColumns = new EntityColumn<TEntity>?[_entity.Columns.Count];

                IsDirty = true;
            }

            if (_changedColumns[propertyIndex] is null)
            {
                _changedColumns[propertyIndex] = _entity.Columns.GetColumnByFlagPosition(propertyIndex);
            }
        }

        internal EntityColumn<TEntity>?[] GetColumns()
        {
            return _changedColumns;
        }
    }
}
