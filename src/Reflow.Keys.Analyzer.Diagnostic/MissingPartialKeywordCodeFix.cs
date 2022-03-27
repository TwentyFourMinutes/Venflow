using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Reflow.Keys.Analyzer.Diagnostic
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class MissingPartialKeywordCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            new[] { "RF1001" }.ToImmutableArray();

        public override FixAllProvider? GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
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

                var root = await diagnostic.Location.SourceTree!
                    .GetRootAsync(context.CancellationToken)
                    .ConfigureAwait(false);

                var structSyntax = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent!
                    .AncestorsAndSelf()
                    .OfType<StructDeclarationSyntax>()
                    .First();

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Make struct partial",
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
            var newModifiers = structSyntax.Modifiers.Add(
                SyntaxFactory.Token(SyntaxKind.PartialKeyword)
            );

            var newStructSyntax = structSyntax.WithModifiers(newModifiers);

            var formattedStructSyntax = newStructSyntax.WithAdditionalAnnotations(
                Formatter.Annotation
            )!;

            var oldRoot = await document
                .GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);

            var newRoot = oldRoot!.ReplaceNode(structSyntax, formattedStructSyntax)!;

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
