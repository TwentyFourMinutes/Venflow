namespace Venflow.Modeling
{
    internal class EntityColumnCollection<TEntity> : DualKeyCollection<string, EntityColumn<TEntity>> where TEntity : class, new()
    {
        internal int RegularColumnsOffset { get; }
        internal int LastRegularColumnsIndex { get; }
        internal int ReadOnlyCount { get; }
        internal int ChangeTrackedCount { get; }

        internal EntityColumnCollection(EntityColumn<TEntity>[] firstCollction, Dictionary<string, EntityColumn<TEntity>> twoToOne, int regularColumnsOffset, int lastRegularColumnsIndex, int readOnlyCount, int changeTrackedCount) : base(firstCollction, twoToOne)
        {
            LastRegularColumnsIndex = lastRegularColumnsIndex;
            RegularColumnsOffset = regularColumnsOffset;
            ReadOnlyCount = readOnlyCount;
            ChangeTrackedCount = changeTrackedCount;
        }
    }
}
