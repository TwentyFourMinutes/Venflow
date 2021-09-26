using System.Diagnostics.CodeAnalysis;

namespace Venflow.Commands
{
    internal readonly struct SqlQueryCacheKey
    {
        internal bool IsChangeTracking => _isChangeTracking;

        private readonly Type _returnType;

        private readonly string _sql;

        private readonly bool _isChangeTracking;

        public SqlQueryCacheKey(string sql, bool isChangeTracking, Type returnType)
        {
            _sql = sql;
            _isChangeTracking = isChangeTracking;
            _returnType = returnType;
        }

        public bool Equals(
#if !NET48
            [AllowNull]
#endif
            SqlQueryCacheKey y)
        {
            return _isChangeTracking == y._isChangeTracking && _returnType == y._returnType && _sql == y._sql;
        }

        public new int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(_sql);

            hashCode.Add(_returnType);

            hashCode.Add(_isChangeTracking);

            return hashCode.ToHashCode();
        }
    }
}
