using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reflow.Analyzer.Incremental
{
    [Generator(LanguageNames.CSharp)]
    public class IncrementalGenerator : IIncrementalGenerator
    {
        public static Stopwatch sw = new Stopwatch();

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var queryClassDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    static (node, _) =>
                    {
                        if (!sw.IsRunning)
                            sw.Restart();

                        if (node is not LambdaExpressionSyntax lambdaSyntax)
                            return false;

                        var invocationSyntax =
                            lambdaSyntax.FirstAncestorOrSelf<InvocationExpressionSyntax>();

                        if (invocationSyntax is null)
                            return false;

                        var memberAccessSyntax = invocationSyntax
                            .ChildNodes()
                            .OfType<MemberAccessExpressionSyntax>()
                            .FirstOrDefault();

                        if (
                            memberAccessSyntax is null
                            || memberAccessSyntax.Name.Identifier.Text
                                is not "Query"
                                    and not "QueryRaw"
                        )
                        {
                            return false;
                        }

                        return true;
                    },
                    static (ctx, _) => ctx.Node.FirstAncestorOrSelf<ClassDeclarationSyntax>()
                )
                .Where(static x => x is not null)
                .Select(static (x, _) => x!);

            IncrementalValueProvider<(Compilation Compilation, ImmutableArray<ClassDeclarationSyntax> Classes)> compilationAndClasses =
                context.CompilationProvider
                    .Combine(queryClassDeclarations.Collect())
                    .Select((x, _) => (x.Left, x.Right.Distinct().ToImmutableArray()));

            context.RegisterSourceOutput(
                compilationAndClasses,
                static (spc, source) =>
                {
                    if (source.Classes.IsDefaultOrEmpty)
                    {
                        return;
                    }

                    sw.Stop();

                    spc.ReportDiagnostic(
                        Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "INCREMNTAL",
                                "Incremental Time",
                                "{0} ms",
                                "main",
                                DiagnosticSeverity.Warning,
                                true
                            ),
                            null,
                            sw.ElapsedMilliseconds
                        )
                    );


                }
            );
        }
    }
}
