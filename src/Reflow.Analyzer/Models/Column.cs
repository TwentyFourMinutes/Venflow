using Microsoft.CodeAnalysis;

namespace Reflow.Analyzer.Sections
{
    internal class Column
    {
        internal string ColumnName { get; set; }
        internal string PropertyName { get; }
        internal INamedTypeSymbol Type { get; }
        internal bool IsUpdatable { get; }

        internal Column(string propertyName, INamedTypeSymbol type, bool isUpdatable)
        {
            PropertyName = propertyName;
            ColumnName = propertyName;
            Type = type;
            IsUpdatable = isUpdatable;
        }
    }
}
