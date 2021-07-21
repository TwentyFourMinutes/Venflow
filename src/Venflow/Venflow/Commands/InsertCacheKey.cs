using System;
using System.Diagnostics.CodeAnalysis;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal readonly struct InsertCacheKey
    {
        internal EntityRelation[] Relations => _relations;

        private readonly InsertCacheKeyOptions _options;
        private readonly EntityRelation[] _relations;

        internal InsertCacheKey(EntityRelation[] relations, InsertCacheKeyOptions options)
        {
            _relations = relations;
            _options = options;
        }

        public bool Equals(
#if !NET48
            [AllowNull]
#endif
            InsertCacheKey y)
        {
            if (y._relations.Length != _relations.Length ||
                y._options != _options)
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

            hashCode.Add(_options);

            var relaionsSpan = _relations.AsSpan();

            for (int relationIndex = relaionsSpan.Length - 1; relationIndex >= 0; relationIndex--)
            {
                hashCode.Add(relaionsSpan[relationIndex].RelationId);
            }

            return hashCode.ToHashCode();
        }
    }
}