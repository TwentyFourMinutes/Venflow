using Venflow.Modeling;

namespace Venflow.Dynamic
{
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
