using System.Collections.Generic;
using Venflow.Modeling;

namespace Venflow.Dynamic.Materializer
{
    internal class QueryEntityHolder
    {
        internal int Id { get; }
        internal Entity Entity { get; }
        internal List<(EntityRelation, QueryEntityHolder)> LateAssignedRelations { get; }
        internal List<(EntityRelation, QueryEntityHolder)> AssignedRelations { get; }
        internal List<EntityRelation> InitializeNavigation { get; }

        internal QueryEntityHolder(Entity entity, int id)
        {
            LateAssignedRelations = new List<(EntityRelation, QueryEntityHolder)>();
            AssignedRelations = new List<(EntityRelation, QueryEntityHolder)>();
            InitializeNavigation = new List<EntityRelation>();

            Entity = entity;
            Id = id;
        }
    }
}