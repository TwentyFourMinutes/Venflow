using Venflow.Modeling;

namespace Venflow.Dynamic.Materializer
{
    internal class QueryEntityHolder
    {
        internal int Id { get; }
        internal Entity Entity { get; }
        internal List<(EntityRelation, QueryEntityHolder)> SelfAssignedRelations { get; }
        internal List<(EntityRelation, QueryEntityHolder)> ForeignAssignedRelations { get; }
        internal List<EntityRelation> InitializeNavigations { get; }

        internal bool HasRelations => SelfAssignedRelations.Count > 0 || ForeignAssignedRelations.Count > 0;
        internal bool RequiresChangedLocal { get; set; }
        internal bool RequiresDBNullCheck { get; set; }

        internal QueryEntityHolder(Entity entity, int id)
        {
            SelfAssignedRelations = new List<(EntityRelation, QueryEntityHolder)>();
            ForeignAssignedRelations = new List<(EntityRelation, QueryEntityHolder)>();
            InitializeNavigations = new List<EntityRelation>();

            Entity = entity;
            Id = id;
        }
    }
}
