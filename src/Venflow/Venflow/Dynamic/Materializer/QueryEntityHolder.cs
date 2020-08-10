using System.Collections.Generic;
using Venflow.Modeling;

namespace Venflow.Dynamic.Materializer
{
    internal class QueryEntityHolder
    {
        internal int Id { get; }
        internal Entity Entity { get; }
        internal List<(EntityRelation, QueryEntityHolder)> AssignedRelations { get; }
        internal List<(EntityRelation, QueryEntityHolder)> AssigningRelations { get; }
        internal List<EntityRelation> InitializeNavigation { get; }

        internal bool HasRelations => AssignedRelations.Count > 0 || AssigningRelations.Count > 0;
        internal bool RequiresChangedLocal { get; set; }

        internal QueryEntityHolder(Entity entity, int id)
        {
            AssignedRelations = new List<(EntityRelation, QueryEntityHolder)>();
            AssigningRelations = new List<(EntityRelation, QueryEntityHolder)>();
            InitializeNavigation = new List<EntityRelation>();

            Entity = entity;
            Id = id;
        }
    }
}