using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Internal;
using static Reflow.Internal.CSharpCodeGenerator;

namespace Reflow.Analyzer.Lambdas.Emitters
{
    internal static class LambdaLinksEmitter
    {
        internal static SourceText Emit(
            IList<LambdaLink> links,
            IList<ClosureLambdaLink> closureLinks
        )
        {
            var syntaxLinks = new SyntaxList<ExpressionSyntax>();

            for (var linkIndex = 0; linkIndex < links.Count; linkIndex++)
            {
                var link = links[linkIndex];

                syntaxLinks = syntaxLinks.Add(
                    Instance(Type(nameof(LambdaLink)))
                        .WithArguments(
                            Constant(link.FullClassName),
                            Constant(link.FullLambdaName),
                            Instance(Type(nameof(LambdaData)))
                                .WithArguments(
                                    Constant(link.Data.MinimumSqlLength),
                                    ArrayInitializer(
                                        Array(Type(TypeCode.Int16)),
                                        link.Data.ParameterIndecies.Select(
                                            x => (ExpressionSyntax)Constant(x)
                                        )
                                    )
                                )
                        )
                );
            }

            for (var linkIndex = 0; linkIndex < closureLinks.Count; linkIndex++)
            {
                var link = closureLinks[linkIndex];

                syntaxLinks = syntaxLinks.Add(
                    Instance(Type(nameof(ClosureLambdaLink)))
                        .WithArguments(
                            Constant(link.FullClassName),
                            Constant(link.MemberIndex),
                            Constant(link.FullLambdaName),
                            Instance(Type(nameof(LambdaData)))
                                .WithArguments(
                                    Constant(link.Data.MinimumSqlLength),
                                    ArrayInitializer(
                                        Array(Type(TypeCode.Int16)),
                                        link.Data.ParameterIndecies.Select(
                                            x => (ExpressionSyntax)Constant(x)
                                        )
                                    )
                                )
                        )
                );
            }

            return File("Reflow")
                .WithMembers(
                    Class("Lambdas", CSharpModifiers.Public | CSharpModifiers.Static)
                        .WithMembers(
                            Field(
                                    "Links",
                                    Array(Type(nameof(LambdaLink))),
                                    CSharpModifiers.Public | CSharpModifiers.Static
                                )
                                .WithInitializer(
                                    ArrayInitializer(Array(Type(nameof(LambdaLink))), syntaxLinks)
                                )
                        )
                )
                .GetText();
        }
    }
}
