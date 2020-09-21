using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Venflow.Commands
{
    internal class InsertCacheKeyComparer : IEqualityComparer<InsertCacheKey>
    {
        internal static InsertCacheKeyComparer Default { get; } = new InsertCacheKeyComparer();

        private InsertCacheKeyComparer()
        {

        }

        public bool Equals(
#if !NET48
            [AllowNull]
            #endif  
            InsertCacheKey x,
#if !NET48
            [AllowNull]
            #endif
            InsertCacheKey y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(
#if !NET48
            [DisallowNull]
            #endif  
            InsertCacheKey obj)
        {
            return obj.GetHashCode();
        }
    }
}