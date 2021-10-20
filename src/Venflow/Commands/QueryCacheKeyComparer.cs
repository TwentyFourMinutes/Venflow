using System.Diagnostics.CodeAnalysis;

namespace Venflow.Commands
{
    internal class QueryCacheKeyComparer : IEqualityComparer<QueryCacheKey>
    {
        internal static QueryCacheKeyComparer Default { get; } = new QueryCacheKeyComparer();

        private QueryCacheKeyComparer()
        {

        }

        public bool Equals(
#if !NET48
            [AllowNull]
#endif
            QueryCacheKey x,
#if !NET48
            [AllowNull]
#endif
            QueryCacheKey y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(
#if !NET48
            [DisallowNull]
#endif
            QueryCacheKey obj)
        {
            return obj.GetHashCode();
        }
    }
}
