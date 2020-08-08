using System.Collections.Generic;

namespace Venflow.Modeling
{
    internal class EntityColumnCollection<TEntity> : DualKeyCollection<string, EntityColumn<TEntity>> where TEntity : class, new()
    {
        internal int RegularColumnsOffset { get; }

        internal EntityColumnCollection(EntityColumn<TEntity>[] firstCollction, Dictionary<string, EntityColumn<TEntity>> twoToOne, int regularColumnsOffset) : base(firstCollction, twoToOne)
        {
            RegularColumnsOffset = regularColumnsOffset;
        }
    }
}
