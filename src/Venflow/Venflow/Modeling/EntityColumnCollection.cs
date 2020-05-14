using System.Collections.Generic;
using System.Numerics;

namespace Venflow.Modeling
{
    internal class EntityColumnCollection<TEntity> : DualKeyCollection<string, EntityColumn<TEntity>>, IEntity where TEntity : class
    {
        internal EntityColumnCollection(EntityColumn<TEntity>[] firstCollction, Dictionary<string, int> twoToOne) : base(firstCollction, twoToOne)
        {

        }

        internal EntityColumn<TEntity> GetColumnByFlagValue(ulong flagValue)
        {
            return base[BitOperations.LeadingZeroCount(flagValue)];
        }

        internal EntityColumn<TEntity> GetColumnByFlagPosition(byte flagPosition)
        {
            return base[flagPosition];
        }
    }
}
