using System.Reflection;
using Venflow.Enums;

namespace Venflow.Modeling.Definitions
{
    internal class EntityRelationDefinition
    {
        internal PropertyInfo ForeignProperty { get; }

        internal bool IsKeyInRelation { get; }

        internal PropertyInfo ForeignKeyProperty { get; }

        internal string RelationEntityName { get; }

        internal RelationType RelationType { get; }

        internal string ForeignKeyColumnName { get; set; }

        internal bool IsProcessed { get; set; }

        internal EntityRelationDefinition(PropertyInfo foreignProperty, bool isKeyInRelation, PropertyInfo foreignKeyProperty, string relationEntityName, RelationType relationType)
        {
            ForeignProperty = foreignProperty;
            IsKeyInRelation = isKeyInRelation;
            ForeignKeyProperty = foreignKeyProperty;
            ForeignKeyColumnName = foreignKeyProperty.Name;
            RelationEntityName = relationEntityName;
            RelationType = relationType;
        }
    }
}