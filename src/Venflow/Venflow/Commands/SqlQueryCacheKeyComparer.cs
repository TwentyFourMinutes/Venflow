using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Venflow.Commands
{
    internal class SqlQueryCacheKeyComparer : IEqualityComparer<SqlQueryCacheKey>
    {
        internal static SqlQueryCacheKeyComparer Default { get; } = new SqlQueryCacheKeyComparer();

        private SqlQueryCacheKeyComparer()
        {

        }

        public bool Equals(
#if !NET48
            [AllowNull]
#endif
            SqlQueryCacheKey x,
#if !NET48
            [AllowNull]
#endif
            SqlQueryCacheKey y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(
#if !NET48
            [DisallowNull]
#endif
            SqlQueryCacheKey obj)
        {
            return obj.GetHashCode();
        }
    }
}
