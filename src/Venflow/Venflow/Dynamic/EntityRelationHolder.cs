using System.Collections.Generic;
using Venflow.Modeling;

namespace Venflow.Dynamic
{
    internal class EntityRelationHolder
    {
        internal Entity Entity { get; }
        internal List<EntityRelation> Relations { get; }
        internal List<EntityRelation> AssigningRelations { get; }

        internal EntityRelationHolder(Entity entity)
        {
            Entity = entity;
            Relations = new List<EntityRelation>();
            AssigningRelations = new List<EntityRelation>();
        }
    }
}
