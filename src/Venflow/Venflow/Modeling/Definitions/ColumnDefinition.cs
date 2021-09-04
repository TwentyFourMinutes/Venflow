using System.Reflection;
using NpgsqlTypes;
using Venflow.Enums;

namespace Venflow.Modeling.Definitions
{
    internal class ColumnDefinition
    {
        internal PropertyInfo Property { get; }
        internal string Name { get; set; }
        internal NpgsqlDbType? DbType { get; set; }
        internal ColumnOptions Options { get; set; }

        internal ColumnDefinition(PropertyInfo property)
        {
            Property = property;
            Name = property.Name;
        }
    }
}
