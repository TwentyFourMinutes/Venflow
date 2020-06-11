using System.Reflection;
using Venflow.Enums;

namespace Venflow.Modeling
{
    internal class ForeignEntity
    {
        internal Entity Entity { get; }

        internal EntityColumn ForeignKey { get; }

        internal PropertyInfo ForeignEntityColumn { get; }

        internal RelationType RelationType { get; }

        internal ForeignEntity(Entity entity, EntityColumn foreignKey, PropertyInfo foreignEntityColumn, RelationType relationType)
        {
            Entity = entity;
            ForeignKey = foreignKey;
            ForeignEntityColumn = foreignEntityColumn;
            RelationType = relationType;
        }
    }
}
