using System.Collections.Generic;

namespace Venflow.Modeling
{
    internal class EntityColumnCollection<TEntity> : DualKeyCollection<string, EntityColumn<TEntity>>, IEntity where TEntity : class
    {
        internal EntityColumnCollection(EntityColumn<TEntity>[] firstCollction, Dictionary<string, int> twoToOne) : base(firstCollction, twoToOne)
        {

        }
    }
}
