using System.Reflection;

namespace Venflow.Modeling
{
    internal class ForeignEntity
    {
        internal Entity Entity { get; }

        internal EntityColumn ForeignKey { get; }

        internal PropertyInfo ForeignEntityColumn { get; }

        internal ForeignEntity(Entity entity, EntityColumn foreignKey, PropertyInfo foreignEntityColumn)
        {
            Entity = entity;
            ForeignKey = foreignKey;
            ForeignEntityColumn = foreignEntityColumn;
        }
    }
}
