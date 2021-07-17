using System;
using System.Diagnostics.CodeAnalysis;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal readonly struct InsertCacheKey
    {
        internal EntityRelation[] Relations => _relations;

        private readonly bool _isSingleInsert;
        private readonly EntityRelation[] _relations;

        internal InsertCacheKey(EntityRelation[] relations, bool isSingleInsert)
        {
            _relations = relations;
            _isSingleInsert = isSingleInsert;
        }

        public bool Equals(
#if !NET48
            [AllowNull]
#endif
            InsertCacheKey y)
        {
            if (y._relations.Length != _relations.Length ||
                y._isSingleInsert != _isSingleInsert)
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

            hashCode.Add(_isSingleInsert);

            var relaionsSpan = _relations.AsSpan();

            for (int relationIndex = relaionsSpan.Length - 1; relationIndex >= 0; relationIndex--)
            {
                hashCode.Add(relaionsSpan[relationIndex].RelationId);
            }

            return hashCode.ToHashCode();
        }
    }
}