using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Reflow.Analyzer.Shared;

namespace Reflow.Keys.Analyzer.Diagnostic
{
    [Shared]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MissingPartialKeywordAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            new[]
            {
                new DiagnosticDescriptor(
                    "RF1001",
                    "The partial keyword is missing.",
                    "The 'Reflow.GeneratedKeyAttribute' can only be applied to partial structs.",
                    "Reflow.Keys.Analyzer.Diagnostic",
                    DiagnosticSeverity.Error,
                    true
                )
            }.ToImmutableArray();

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics
            );
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(
                symbolContext =>
                {
                    var symbol = (INamedTypeSymbol)symbolContext.Symbol;

                    if (
                        symbol.TypeKind is TypeKind.Struct
                        && symbol.DeclaringSyntaxReferences.Length == 1
                    )
                    {
                        var structSyntax =
                            (StructDeclarationSyntax)symbol.DeclaringSyntaxReferences[
                                0
                            ].GetSyntax();

                        if (structSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
                            return;

                        var attributeSymbol = symbol
                            .GetAttributes()
                            .FirstOrDefault(
                                x =>
                                    x.AttributeClass is not null
                                    && x.AttributeClass.OriginalDefinition.GetFullName()
                                        is "Reflow.GeneratedKey"
                                            or "Reflow.GeneratedKey`1"
                                    && x.AttributeClass.OriginalDefinition.IsReflowSymbol()
                            );

                        if (attributeSymbol is null)
                            return;

                        symbolContext.ReportDiagnostic(
                            Microsoft.CodeAnalysis.Diagnostic.Create(
                                SupportedDiagnostics[0],
                                Location.Create(
                                    attributeSymbol.ApplicationSyntaxReference!.SyntaxTree,
                                    attributeSymbol.ApplicationSyntaxReference!.Span
                                )
                            )
                        );
                    }
                },
                SymbolKind.NamedType
            );
        }
    }
}
