using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Reflow.Analyzer.Shared;

namespace Reflow.Keys.Diagnostics
{
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
                    "Reflow.Keys.Diagnostics",
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
                        && symbol
                            .GetAttributes()
                            .Any(
                                x =>
                                    x.AttributeClass is not null
                                    && x.AttributeClass.OriginalDefinition.GetFullName()
                                        is "Reflow.GeneratedKey"
                                            or "Reflow.GeneratedKey`1"
                                    && x.AttributeClass.OriginalDefinition.IsReflowSymbol()
                            )
                    )
                    {
                        symbolContext.ReportDiagnostic(
                            Diagnostic.Create(
                                SupportedDiagnostics[0],
                                symbolContext.Symbol.Locations.First()
                            )
                        );
                    }
                },
                SymbolKind.NamedType
            );
        }
    }

    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class MissingPartialKeywordCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            new[] { "RF1001" }.ToImmutableArray();

        public override FixAllProvider? GetFixAllProvider()
        {
            return base.GetFixAllProvider();
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            for (
                var diagnosticIndex = 0;
                diagnosticIndex < context.Diagnostics.Length;
                diagnosticIndex++
            )
            {
                var diagnostic = context.Diagnostics[diagnosticIndex];

                var root = await diagnostic.Location.SourceTree!.GetRootAsync(
                    context.CancellationToken
                );

                var structSyntax = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent!
                    .AncestorsAndSelf()
                    .OfType<StructDeclarationSyntax>()
                    .First();

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Make class partial",
                        ct => MakeStructPartial(context.Document, structSyntax, ct),
                        "ReflowKey"
                    ),
                    diagnostic
                );
            }
        }

        private static async Task<Document> MakeStructPartial(
            Document document,
            StructDeclarationSyntax structSyntax,
            CancellationToken cancellationToken
        )
        {
            var firstToken = structSyntax.GetFirstToken();

            var leadingTrivia = firstToken.LeadingTrivia;
            var trimmedStructSyntax = structSyntax.ReplaceToken(
                firstToken,
                firstToken.WithLeadingTrivia(SyntaxTriviaList.Empty)
            );

            var partialToken = SyntaxFactory.Token(
                leadingTrivia,
                SyntaxKind.PartialKeyword,
                SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker)
            );

            var newModifiers = trimmedStructSyntax.Modifiers.Add(partialToken);

            var newStructSyntax = trimmedStructSyntax.WithModifiers(newModifiers);

            var formattedStructSyntax = newStructSyntax.WithAdditionalAnnotations(
                Formatter.Annotation
            )!;

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);

            var newRoot = oldRoot!.ReplaceNode(structSyntax, formattedStructSyntax)!;

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
