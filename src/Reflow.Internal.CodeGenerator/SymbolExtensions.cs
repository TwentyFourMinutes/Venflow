﻿using Microsoft.CodeAnalysis;

namespace Reflow.Internal
{
    public static class SymbolExtensions
    {
        public static string GetFullName(this ISymbol symbol)
        {
            if (symbol.ContainingNamespace.IsGlobalNamespace)
                return symbol.Name;
            else
                return symbol.ContainingNamespace.ToString() + "." + symbol.Name;
        }

        public static bool IsReflowSymbol(this ISymbol symbol)
        {
            var assemblyIdentity = symbol.ContainingAssembly.Identity;

            return assemblyIdentity.Name is "Reflow"
                && assemblyIdentity.PublicKey.SequenceEqual(AssemblyInfo.PublicKey);
        }
    }
}