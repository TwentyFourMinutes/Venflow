using System.Reflection;

namespace Venflow.Modeling.Definitions
{
    internal class EntityRelationDefinition
    {
        internal PropertyInfo ForeignProperty { get; }
        internal PropertyInfo ForeignKeyProperty { get; }
        internal string RelationEntityName { get; }

        public EntityRelationDefinition(PropertyInfo foreignProperty, PropertyInfo foreignKeyProperty, string relationEntityName)
        {
            ForeignProperty = foreignProperty;
            ForeignKeyProperty = foreignKeyProperty;
            RelationEntityName = relationEntityName;
        }
    }
}
