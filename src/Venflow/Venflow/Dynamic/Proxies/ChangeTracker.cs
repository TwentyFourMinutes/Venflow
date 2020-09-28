using System;

namespace Venflow.Dynamic.Proxies
{
    internal class ChangeTracker<TEntity> where TEntity : class, new()
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
            lock (this)
            {
                if (!TrackChanges)
                    return;

                if (!IsDirty)
                {
                    _changedColumns = new byte[_columnLength];

                    IsDirty = true;
                }

                _changedColumns[propertyIndex] = (byte) (propertyIndex + 1);
            }
        }

        internal Span<byte> GetColumns()
        {
            return _changedColumns.AsSpan();
        }
    }
}
