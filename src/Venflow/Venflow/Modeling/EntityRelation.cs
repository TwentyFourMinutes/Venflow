using System.Reflection;
using Venflow.Enums;

namespace Venflow.Modeling
{
    internal class EntityRelation
    {
        internal uint RelationId { get; }

        internal Entity LeftEntity { get; }
        internal PropertyInfo? LeftNavigationProperty { get; }
        internal bool IsLeftNavigationPropertyInitialized { get; }
        internal bool IsLeftNavigationPropertyNullable { get; }

        internal Entity RightEntity { get; }
        internal PropertyInfo? RightNavigationProperty { get; }
        internal bool IsRightNavigationPropertyNullable { get; }
        internal bool IsRightNavigationPropertyInitialized { get; }

        internal EntityColumn ForeignKeyColumn { get; }
        internal RelationType RelationType { get; }
        internal ForeignKeyLocation ForeignKeyLocation { get; }

        internal EntityRelation Sibiling { get; set; }

        internal EntityRelation(uint relationId, Entity leftEntity, PropertyInfo? leftNavigationProperty, bool isLeftNavigationPropertyInitialized, bool isLeftNavigationPropertyNullable, Entity rightEntity,
                                PropertyInfo? rightNavigationProperty, bool isRightNavigationPropertyInitialized, bool isRightNavigationPropertyNullable, EntityColumn foreignKeyColumn, RelationType relationType, ForeignKeyLocation foreignKeyLocation)
        {
            RelationId = relationId;
            LeftEntity = leftEntity;
            LeftNavigationProperty = leftNavigationProperty;
            IsLeftNavigationPropertyInitialized = isLeftNavigationPropertyInitialized;
            IsLeftNavigationPropertyNullable = isLeftNavigationPropertyNullable;
            RightEntity = rightEntity;
            RightNavigationProperty = rightNavigationProperty;
            IsRightNavigationPropertyInitialized = isRightNavigationPropertyInitialized;
            IsRightNavigationPropertyNullable = isRightNavigationPropertyNullable;
            ForeignKeyColumn = foreignKeyColumn;
            RelationType = relationType;
            ForeignKeyLocation = foreignKeyLocation;
        }
    }
}
