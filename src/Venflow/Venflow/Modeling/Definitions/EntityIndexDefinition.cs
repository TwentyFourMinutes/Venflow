using System.Reflection;
using Venflow.Enums;

namespace Venflow.Modeling.Definitions
{
    internal class EntityIndexDefinition
    {
        internal string? Name { get; set; }
        internal PropertyInfo[] Properties { get; }
        internal bool IsUnique { get; set; }
        internal bool IsConcurrent { get; set; }
        internal IndexMethod IndexMethod { get; set; }
        internal IndexSortOrder? SortOder { get; set; }
        internal IndexNullSortOrder? NullSortOder { get; set; }

        internal EntityIndexDefinition(PropertyInfo property)
        {
            Properties = new[] { property };
        }

        internal EntityIndexDefinition(PropertyInfo[] properties)
        {
            Properties = properties;
        }
    }
}
