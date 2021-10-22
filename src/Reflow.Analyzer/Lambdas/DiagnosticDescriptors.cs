using Microsoft.CodeAnalysis;

namespace Reflow.Analyzer.Lambdas
{
    internal static class DiagnosticDescriptors
    {
        internal static DiagnosticDescriptor InvalidBody { get; } =
            new DiagnosticDescriptor(
                "ExpectsInterpolateStringBody",
                "Expected static string interpolated body",
                "A query lambda can only have a string interpolated body.",
                "LambdaRules",
                DiagnosticSeverity.Error,
                true
            );

        internal static DiagnosticDescriptor UnexpectedSymbol { get; } =
            new DiagnosticDescriptor(
                "UnexpectedSymbolInStringInterpolation",
                "Unexpected Symbol",
                "The symbol '{0}' is not expected in an interpolated string.",
                "LambdaRules",
                DiagnosticSeverity.Error,
                true
            );
    }
}
