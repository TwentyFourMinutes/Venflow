using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Venflow.Dynamic")]

namespace Venflow.Modeling
{
    internal class ChangeTracker<TEntity> where TEntity : class
    {
        internal bool TrackChanges { get; set; }
        internal bool IsDirty { get; set; }

        private byte[]? _changedColumns;

        private readonly int _columnLength;

        internal ChangeTracker(int columnLength, bool trackChanges)
        {
            _columnLength = columnLength;
            TrackChanges = trackChanges;
            _changedColumns = null!;
        }

        internal void MakeDirty(byte propertyIndex)
        {
            if (!TrackChanges)
                return;

            if (!IsDirty)
            {
                _changedColumns = new byte[_columnLength];

                IsDirty = true;
            }

            _changedColumns[propertyIndex] = propertyIndex;
        }

        internal byte[]? GetColumns()
        {
            return _changedColumns;
        }
    }
}
