using System.Reflection;
using Venflow.Enums;

namespace Venflow.Modeling
{
    internal class EntityRelation
    {
        internal uint RelationId { get; }

        internal Entity LeftEntity { get; }
        internal PropertyInfo? LeftNavigationProperty { get; }

        internal Entity RightEntity { get; }
        internal PropertyInfo? RightNavigationProperty { get; }

        internal EntityColumn ForeignKeyColumn { get; }
        internal RelationType RelationType { get; }
        internal ForeignKeyLocation ForeignKeyLocation { get; }

        internal EntityRelation Sibiling { get; set; }

        internal EntityRelation(uint relationId, Entity leftEntity, PropertyInfo? leftNavigationProperty, Entity rightEntity,
            PropertyInfo? rightNavigationProperty, EntityColumn foreignKeyColumn, RelationType relationType, ForeignKeyLocation foreignKeyLocation)
        {
            RelationId = relationId;
            LeftEntity = leftEntity;
            LeftNavigationProperty = leftNavigationProperty;
            RightEntity = rightEntity;
            RightNavigationProperty = rightNavigationProperty;
            ForeignKeyColumn = foreignKeyColumn;
            RelationType = relationType;
            ForeignKeyLocation = foreignKeyLocation;
        }
    }
}