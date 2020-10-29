using Venflow.Enums;

namespace Venflow.Modeling.Definitions
{
    internal class RelationInformationDefinition
    {
        internal string? ConstraintName { get; set; }
        internal ConstraintAction OnUpdateAction { get; set; }
        internal ConstraintAction OnDeleteAction { get; set; }
    }
}
