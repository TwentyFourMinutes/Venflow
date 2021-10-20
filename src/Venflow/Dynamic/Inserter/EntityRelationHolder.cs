using Venflow.Modeling;

namespace Venflow.Dynamic.Inserter
{
    internal class EntityRelationHolder
    {
        internal Entity Entity { get; }
        internal List<EntityRelation> SelfAssignedRelations { get; }
        internal List<EntityRelation> ForeignAssignedRelations { get; }
        internal EntityRelation? DirectAssignedRelation { get; set; }

        internal EntityRelationHolder(Entity entity)
        {
            Entity = entity;
            SelfAssignedRelations = new List<EntityRelation>();
            ForeignAssignedRelations = new List<EntityRelation>();
        }
    }
}
