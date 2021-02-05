using System;
using System.Diagnostics.CodeAnalysis;

namespace Venflow.Commands
{
    internal readonly struct SqlQueryCacheKey
    {
        internal bool IsChangeTracking => _isChangeTracking;

        private readonly string _sql;

        private readonly bool _isChangeTracking;

        public SqlQueryCacheKey(string sql, bool isChangeTracking)
        {
            _sql = sql;
            _isChangeTracking = isChangeTracking;
        }

        public bool Equals(
#if !NET48
            [AllowNull]
#endif
            SqlQueryCacheKey y)
        {
            return _isChangeTracking == y._isChangeTracking && _sql == y._sql;
        }

        public new int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(_sql);

            hashCode.Add(_isChangeTracking);

            return hashCode.ToHashCode();
        }
    }
}