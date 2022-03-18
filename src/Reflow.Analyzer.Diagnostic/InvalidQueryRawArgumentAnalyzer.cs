using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reflow.Keys.Analyzer.Diagnostic
{
    [Shared]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InvalidQueryRawArgumentAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            new[]
            {
                new DiagnosticDescriptor(
                    "RF1002",
                    "Invalid argument.",
                    "Only inline lambdas are allowed as an argument.",
                    "Reflow.Analyzer.Diagnostic",
                    DiagnosticSeverity.Error,
                    true
                ),
                new DiagnosticDescriptor(
                    "RF1003",
                    "Invalid argument.",
                    "Only inline non-interpolated strings are allowed as an argument, if you want string interpolation consider using a different method.",
                    "Reflow.Analyzer.Diagnostic",
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
            context.RegisterSyntaxNodeAction(
                nodeContext =>
                {
                    var invocationSyntax = (InvocationExpressionSyntax)nodeContext.Node;

                    var memberAccessSyntax =
                        (MemberAccessExpressionSyntax)invocationSyntax.Expression;

                    if (
                        memberAccessSyntax.Name.Identifier.Text is not "QueryRaw"
                        || invocationSyntax.ArgumentList.Arguments.Count != 1
                    )
                    {
                        return;
                    }

                    var argumentSyntax = invocationSyntax.ArgumentList.Arguments[0].Expression;

                    if (argumentSyntax is not ParenthesizedLambdaExpressionSyntax lambdaSyntax)
                    {
                        nodeContext.ReportDiagnostic(
                            Microsoft.CodeAnalysis.Diagnostic.Create(
                                SupportedDiagnostics[0],
                                Location.Create(argumentSyntax.SyntaxTree, argumentSyntax.Span)
                            )
                        );

                        return;
                    }

                    var isValidExpression = true;
                    SyntaxNode node;

                    if (lambdaSyntax.ExpressionBody is not null)
                    {
                        node = lambdaSyntax.ExpressionBody;
                    }
                    else if (lambdaSyntax.Body is not null)
                    {
                        var nodes = lambdaSyntax.Body.ChildNodes().Take(2).ToArray();

                        node = lambdaSyntax.Body;

                        if (nodes.Length != 1)
                        {
                            isValidExpression = false;
                        }
                    }
                    else
                    {
                        return;
                    }

                    if (
                        !isValidExpression
                        || node is not LiteralExpressionSyntax literalSyntax
                        || !literalSyntax.IsKind(SyntaxKind.StringLiteralExpression)
                    )
                    {
                        nodeContext.ReportDiagnostic(
                            Microsoft.CodeAnalysis.Diagnostic.Create(
                                SupportedDiagnostics[1],
                                Location.Create(node.SyntaxTree, node.Span)
                            )
                        );
                    }
                },
                SyntaxKind.InvocationExpression
            );
        }
    }
}
