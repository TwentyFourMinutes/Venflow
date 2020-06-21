using System.Reflection;
using Venflow.Enums;

namespace Venflow.Modeling
{
    internal class ForeignEntity
    {
        internal Entity BaseEntity { get; }

        internal Entity RelationEntity { get; }

        internal EntityColumn ForeignKey { get; }

        internal PropertyInfo ForeignEntityColumn { get; }

        internal RelationType RelationType { get; }

        internal ForeignEntity(Entity baseEntity, Entity relationEntity, EntityColumn foreignKey, PropertyInfo foreignEntityColumn, RelationType relationType)
        {
            BaseEntity = baseEntity;
            RelationEntity = relationEntity;
            ForeignKey = foreignKey;
            ForeignEntityColumn = foreignEntityColumn;
            RelationType = relationType;
        }
    }
}
