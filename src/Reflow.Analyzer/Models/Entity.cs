using Microsoft.CodeAnalysis;

namespace Reflow.Analyzer.Models
{
    internal class Entity
    {
        internal IPropertySymbol PropertySymbol { get; }
        internal ITypeSymbol EntitySymbol { get; }
        internal List<Column> Columns { get; }

        internal Entity(IPropertySymbol symbol, ITypeSymbol entitySymbol)
        {
            PropertySymbol = symbol;
            EntitySymbol = entitySymbol;
            Columns = new();
        }
    }
}
