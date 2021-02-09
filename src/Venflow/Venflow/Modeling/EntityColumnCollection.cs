using System.Collections.Generic;

namespace Venflow.Modeling
{
    internal class EntityColumnCollection<TEntity> : DualKeyCollection<string, EntityColumn<TEntity>> where TEntity : class, new()
    {
        internal int RegularColumnsOffset { get; }
        internal int LastNonReadOnlyColumnsIndex { get; }
        internal int ReadOnlyCount { get; }
        internal int ChangeTrackedCount { get; }

        internal EntityColumnCollection(EntityColumn<TEntity>[] firstCollction, Dictionary<string, EntityColumn<TEntity>> twoToOne, int regularColumnsOffset, int lastNonReadOnlyColumnsIndex, int readOnlyCount, int changeTrackedCount) : base(firstCollction, twoToOne)
        {
            LastNonReadOnlyColumnsIndex = lastNonReadOnlyColumnsIndex;
            RegularColumnsOffset = regularColumnsOffset;
            ReadOnlyCount = readOnlyCount;
            ChangeTrackedCount = changeTrackedCount;
        }
    }
}
