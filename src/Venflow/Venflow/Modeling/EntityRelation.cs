using System.Reflection;
using Venflow.Enums;

namespace Venflow.Modeling
{
    internal class EntityRelation
    {
        internal Entity LeftEntity { get; }
        internal PropertyInfo? LeftNavigationProperty { get; }

        internal Entity RightEntity { get; }
        internal PropertyInfo? RightNavigationProperty { get; }

        internal EntityColumn ForeignKeyColumn { get; }
        internal RelationType RelationType { get; }
        internal ForeignKeyLoaction ForeignKeyLoaction { get; }

        internal EntityRelation(Entity leftEntity, PropertyInfo? leftNavigationProperty, Entity rightEntity,
            PropertyInfo? rightNavigationProperty, EntityColumn foreignKeyColumn, RelationType relationType, ForeignKeyLoaction foreignKeyLoaction)
        {
            LeftEntity = leftEntity;
            LeftNavigationProperty = leftNavigationProperty;
            RightEntity = rightEntity;
            RightNavigationProperty = rightNavigationProperty;
            ForeignKeyColumn = foreignKeyColumn;
            RelationType = relationType;
            ForeignKeyLoaction = foreignKeyLoaction;
        }
    }
}