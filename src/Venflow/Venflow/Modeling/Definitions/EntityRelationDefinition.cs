using System.Reflection;
using Venflow.Enums;

namespace Venflow.Modeling.Definitions
{
    internal class EntityRelationDefinition
    {
        internal bool IsProcessed { get; set; }

        internal EntityBuilder LeftEntity { get; }
        internal PropertyInfo? LeftNavigationProperty { get; }

        internal string RightEntityName { get; }
        internal PropertyInfo? RightNavigationProperty { get; }

        internal string ForeignKeyColumnName { get; set; }

        internal RelationType RelationType { get; }
        internal ForeignKeyLoaction ForeignKeyLoaction { get; }

        internal EntityRelationDefinition(EntityBuilder leftEntity, PropertyInfo? leftNavigationProperty, string rightEntityName, PropertyInfo? rightNavigationProperty, string foreignKeyColumnName, RelationType relationType, ForeignKeyLoaction foreignKeyLoaction)
        {
            LeftEntity = leftEntity;
            LeftNavigationProperty = leftNavigationProperty;
            RightEntityName = rightEntityName;
            RightNavigationProperty = rightNavigationProperty;
            ForeignKeyColumnName = foreignKeyColumnName;
            RelationType = relationType;
            ForeignKeyLoaction = foreignKeyLoaction;
        }
    }
}