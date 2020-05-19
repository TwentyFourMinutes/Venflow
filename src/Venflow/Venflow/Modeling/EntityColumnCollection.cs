using System.Collections.Generic;

namespace Venflow.Modeling
{
    internal class EntityColumnCollection<TEntity> : DualKeyCollection<string, EntityColumn<TEntity>> where TEntity : class
    {
        internal int RegularColumnsOffset { get; }

        internal EntityColumnCollection(EntityColumn<TEntity>[] firstCollction, Dictionary<string, int> twoToOne, int regularColumnsOffset) : base(firstCollction, twoToOne)
        {
            RegularColumnsOffset = regularColumnsOffset;
        }
    }
}
