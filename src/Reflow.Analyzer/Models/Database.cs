using Microsoft.CodeAnalysis;
using Reflow.Analyzer.Operations;

namespace Reflow.Analyzer.Models
{
    internal class Database
    {
        internal ITypeSymbol Symbol { get; }
        internal List<(IPropertySymbol PropertySymbol, INamedTypeSymbol EntitySymbol)> EntitySymbols { get; }
        internal Dictionary<ITypeSymbol, Entity> Entities { get; }
        internal List<Query> Queries { get; }

        internal Database(ITypeSymbol symbol)
        {
            Symbol = symbol;
            EntitySymbols = new();
            Entities = new(SymbolEqualityComparer.Default);
            Queries = new();
        }
    }
}
