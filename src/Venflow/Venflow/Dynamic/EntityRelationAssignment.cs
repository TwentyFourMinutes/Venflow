using System.Collections.Generic;
using System.Reflection;

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
}
