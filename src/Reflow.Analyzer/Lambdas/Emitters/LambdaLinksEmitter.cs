using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.SyntaxGenerator;
using static Reflow.Analyzer.SyntaxGenerator.CSharpSyntaxGenerator;

namespace Reflow.Analyzer.Lambdas.Emitters
{
    internal static class LambdaLinksEmitter
    {
        internal static SourceText Emit(
            IList<LambdaLink> links,
            IList<ClosureLambdaLink> closureLinks
        )
        {
            var syntaxLinks = new ObjectCreationExpressionSyntax[links.Count + closureLinks.Count];
            var parameterIndecies = new List<ExpressionSyntax>();

            var syntaxLinksIndex = 0;

            for (var linkIndex = 0; linkIndex < links.Count; linkIndex++)
            {
                var link = links[linkIndex];

                syntaxLinks[syntaxLinksIndex++] = Instance(Type(nameof(LambdaLink)))
                    .WithArguments(
                        Constant(link.FullClassName),
                        Constant(link.FullLambdaName),
                        Instance(Type(nameof(LambdaData)))
                            .WithArguments(
                                Constant(link.Data.MinimumSqlLength),
                                ArrayInitializer(
                                    Array(Type("short")),
                                    link.Data.ParameterIndecies.Select(x => Constant(x))
                                )
                            )
                    );
            }

            for (var linkIndex = 0; linkIndex < closureLinks.Count; linkIndex++)
            {
                var link = closureLinks[linkIndex];

                syntaxLinks[syntaxLinksIndex++] = Instance(Type(nameof(ClosureLambdaLink)))
                    .WithArguments(
                        Constant(link.FullClassName),
                        Constant(link.MemberIndex),
                        Constant(link.FullLambdaName),
                        Instance(Type(nameof(LambdaData)))
                            .WithArguments(
                                Constant(link.Data.MinimumSqlLength),
                                ArrayInitializer(
                                    type: Array(Type("short")),
                                    expressions: link.Data.ParameterIndecies.Select(
                                        x => Constant(x)
                                    )
                                )
                            )
                    );
            }

            return File(
                usings: new[] { "System", "System.Collections.Generic", "System.Text" },
                namespaceName: "Reflow",
                members: Class(name: "Lambdas", SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                    .WithMembers(
                        Field(
                            variable: Variable(
                                name: "Links",
                                type: Array(Type(nameof(LambdaLink))),
                                expressionSyntax: ArrayInitializer(
                                    type: Array(Type(nameof(LambdaLink))),
                                    expressions: syntaxLinks
                                )
                            ),
                            SyntaxKind.PublicKeyword,
                            SyntaxKind.StaticKeyword
                        )
                    )
            );
        }
    }
}
