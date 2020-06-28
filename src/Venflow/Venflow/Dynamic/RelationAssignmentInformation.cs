using System.Collections.Generic;
using System.Reflection;
using Venflow.Modeling;

namespace Venflow.Dynamic
{
    internal class EntityRelationAssignment
    {
        internal FieldInfo LastLeftEntity { get; }

        internal FieldInfo HasLastLeftEntityChanged { get; }

        internal List<RelationAssignmentInformation> Relations { get; }

        internal EntityRelationAssignment(FieldInfo lastLeftEntity, FieldInfo hasLastLeftEntityChanged)
        {
            LastLeftEntity = lastLeftEntity;
            HasLastLeftEntityChanged = hasLastLeftEntityChanged;

            Relations = new List<RelationAssignmentInformation>();
        }
    }

    internal class RelationAssignmentInformation
    {
        internal EntityRelation EntityRelation { get; }

        internal string LastRightName { get; }

        internal RelationAssignmentInformation(EntityRelation entityRelation, string lastRightName)
        {
            EntityRelation = entityRelation;
            LastRightName = lastRightName;
        }
    }
}
