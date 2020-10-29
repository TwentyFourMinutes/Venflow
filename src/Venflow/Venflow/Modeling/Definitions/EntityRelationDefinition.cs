using System.Reflection;
using Venflow.Enums;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.Modeling.Definitions
{
    internal class EntityRelationDefinition
    {
        internal bool IsProcessed { get; set; }

        internal uint RelationId { get; }

        internal EntityBuilder LeftEntityBuilder { get; }
        internal PropertyInfo? LeftNavigationProperty { get; }

        internal string RightEntityName { get; }
        internal PropertyInfo? RightNavigationProperty { get; }

        internal string ForeignKeyColumnName { get; set; }

        internal RelationType RelationType { get; }
        internal ForeignKeyLocation ForeignKeyLocation { get; }

        internal RelationInformationDefinition? Information { get; }

        internal EntityRelationDefinition(uint relationId, EntityBuilder leftEntity, PropertyInfo? leftNavigationProperty, string rightEntityName, PropertyInfo? rightNavigationProperty, string foreignKeyColumnName, RelationType relationType, ForeignKeyLocation foreignKeyLocation)
        {
            RelationId = relationId;
            LeftEntityBuilder = leftEntity;
            LeftNavigationProperty = leftNavigationProperty;
            RightEntityName = rightEntityName;
            RightNavigationProperty = rightNavigationProperty;
            ForeignKeyColumnName = foreignKeyColumnName;
            RelationType = relationType;
            ForeignKeyLocation = foreignKeyLocation;

            if (VenflowConfiguration.PopulateEntityInformation)
                Information = new RelationInformationDefinition();
        }
    }
}