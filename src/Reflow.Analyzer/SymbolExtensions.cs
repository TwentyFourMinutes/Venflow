using Microsoft.CodeAnalysis;
using Reflow.Analyzer.Properties;

namespace Reflow.Analyzer
{
    internal static class SymbolExtensions
    {
        internal static string GetFullName(this ISymbol symbol)
        {
            if (symbol.ContainingNamespace.IsGlobalNamespace)
                return symbol.Name;
            else
                return symbol.ContainingNamespace.ToString() + "." + symbol.Name;
        }

        internal static bool IsReflowSymbol(this ISymbol symbol)
        {
            var assemblyIdentity = symbol.ContainingAssembly.Identity;

            return assemblyIdentity.Name is "Reflow"
                && assemblyIdentity.PublicKey.SequenceEqual(AssemblyInfo.PublicKey);
        }
    }
}
