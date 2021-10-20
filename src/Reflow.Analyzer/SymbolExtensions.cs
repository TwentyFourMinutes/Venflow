using Microsoft.CodeAnalysis;

namespace Reflow.Analyzer
{
    internal static class SymbolExtensions
    {
        public static string GetFullName(this ISymbol symbol)
        {
            if (symbol.ContainingNamespace.IsGlobalNamespace)
                return symbol.Name;
            else
                return symbol.ContainingNamespace.Name + "." + symbol.Name;
        }
    }
}
