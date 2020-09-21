using System;
using System.Diagnostics.CodeAnalysis;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal struct InsertCacheKey
    {
        internal bool IsSingleInsert { get; set; }

        internal EntityRelation[] Relations => _relations;

        private readonly EntityRelation[] _relations;

        internal InsertCacheKey(EntityRelation[] relations)
        {
            _relations = relations;

            IsSingleInsert = false;
        }

        public bool Equals(
#if !NET48 
            [AllowNull] 
            #endif 
            InsertCacheKey y)
        {
            if (y._relations.Length != _relations.Length||
                y.IsSingleInsert != IsSingleInsert)
                return false;

            var relaionsSpan = _relations.AsSpan();
            var foreignRelaionsSpan = y._relations.AsSpan();

            for (int relationIndex = relaionsSpan.Length - 1; relationIndex >= 0; relationIndex--)
            {
                if (relaionsSpan[relationIndex].RelationId != foreignRelaionsSpan[relationIndex].RelationId)
                    return false;
            }

            return true;
        }

        public new int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(IsSingleInsert);

            var relaionsSpan = _relations.AsSpan();

            for (int relationIndex = relaionsSpan.Length - 1; relationIndex >= 0; relationIndex--)
            {
                hashCode.Add(relaionsSpan[relationIndex].RelationId);
            }

            return hashCode.ToHashCode();
        }
    }
}