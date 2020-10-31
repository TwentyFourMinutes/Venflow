using System.Reflection;
using Venflow.Enums;

namespace Venflow.Modeling
{
    internal class EntityIndex
    {
        internal string? Name { get; }
        internal PropertyInfo[] Properties { get; }
        internal bool IsUnique { get; }
        internal bool IsConcurrent { get; }
        internal IndexMethod IndexMethod { get; }
        internal IndexSortOrder? SortOder { get; }
        internal IndexNullSortOrder? NullSortOder { get; }

        internal EntityIndex(string? name, PropertyInfo[] properties, bool isUnique, bool isConcurrent, IndexMethod indexMethod, IndexSortOrder? sortOder, IndexNullSortOrder? nullSortOder)
        {
            Name = name;
            Properties = properties;
            IsUnique = isUnique;
            IsConcurrent = isConcurrent;
            IndexMethod = indexMethod;
            SortOder = sortOder;
            NullSortOder = nullSortOder;
        }
    }
}
