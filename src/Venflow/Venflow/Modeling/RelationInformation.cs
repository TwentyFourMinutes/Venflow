using Venflow.Enums;

namespace Venflow.Modeling
{
    internal class RelationInformation
    {
        internal string? ConstraintName { get; }
        internal ConstraintAction? OnUpdateAction { get; }
        internal ConstraintAction? OnDeleteAction { get; }

        internal RelationInformation(string? constraintName, ConstraintAction? onUpdateAction, ConstraintAction? onDeleteAction)
        {
            ConstraintName = constraintName;
            OnUpdateAction = onUpdateAction;
            OnDeleteAction = onDeleteAction;
        }
    }
}
