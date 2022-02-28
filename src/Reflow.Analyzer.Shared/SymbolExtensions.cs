using Microsoft.CodeAnalysis;

namespace Reflow.Analyzer.Shared
{
    internal static class SymbolExtensions
    {
        internal static string GetNamespace(this ISymbol symbol)
        {
            if (symbol.ContainingNamespace.IsGlobalNamespace)
                throw new NotImplementedException();
            else
                return symbol.ContainingNamespace.ToString();
        }

        internal static string GetFullName(this ISymbol symbol)
        {
            if (symbol.ContainingNamespace.IsGlobalNamespace)
                return symbol.MetadataName;
            else
                return symbol.ContainingNamespace.ToString() + "." + symbol.MetadataName;
        }

        internal static bool IsReflowSymbol(this ISymbol symbol)
        {
            var assemblyIdentity = symbol.ContainingAssembly.Identity;

            return assemblyIdentity.Name is "Reflow" or "Reflow.Keys"
                && assemblyIdentity.PublicKey.SequenceEqual(AssemblyInfo.PublicKey);
        }
    }
}
