#nullable disable
using Microsoft.CodeAnalysis;

namespace Reflow.Analyzer.Database
{
    internal class DatabaseConfiguration
    {
        internal ITypeSymbol Type { get; }
        internal List<DatabaseTable> Tables { get; }

        internal DatabaseConfiguration(ITypeSymbol type)
        {
            Type = type;
            Tables = new();
        }
    }

    internal class DatabaseTable
    {
        internal IPropertySymbol Type { get; }
        internal ITypeSymbol EntityType { get; }
        internal List<IPropertySymbol> Columns { get; }

        internal DatabaseTable(IPropertySymbol type, ITypeSymbol entityType)
        {
            Type = type;
            EntityType = entityType;
            Columns = new();
        }
    }
}
