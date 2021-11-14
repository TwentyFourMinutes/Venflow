//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Text;
//using Reflow.Analyzer.CodeGenerator;
//using Reflow.Analyzer.Models.Definitions;
//using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

//namespace Reflow.Analyzer.Emitters
//{
//    internal static class LambdaLinksEmitter
//    {
//        internal static SourceText Emit(
//            IList<LambdaLink> links,
//            IList<ClosureLambdaLinkDefinition> closureLinks
//        )
//        {
//            var syntaxLinks = new SyntaxList<ExpressionSyntax>();

//            for (var linkIndex = 0; linkIndex < links.Count; linkIndex++)
//            {
//                var link = links[linkIndex];

//                syntaxLinks = syntaxLinks.Add(
//                    Instance(Type("Reflow.Lambdas.LambdaLink"))
//                        .WithArguments(
//                            TypeOf(Type(link.FullClassName)),
//                            Constant(link.FullLambdaName),
//                            Instance(Type("Reflow.Lambdas.LambdaData"))
//                                .WithArguments(
//                                    Constant(link.Data.MinimumSqlLength),
//                                    ArrayInitializer(
//                                        Array(Type(typeof(short))),
//                                        link.Data.ParameterIndecies.Select(
//                                            x => (ExpressionSyntax)Constant(x)
//                                        )
//                                    ),
//                                    ArrayInitializer(
//                                        Array(Type(typeof(Type))),
//                                        link.Data.UsedEntities.Select(
//                                            x => (ExpressionSyntax)TypeOf(Type(x))
//                                        )
//                                    )
//                                )
//                        )
//                );
//            }

//            for (var linkIndex = 0; linkIndex < closureLinks.Count; linkIndex++)
//            {
//                var link = closureLinks[linkIndex];

//                syntaxLinks = syntaxLinks.Add(
//                    Instance(Type("Reflow.Lambdas.ClosureLambdaLink"))
//                        .WithArguments(
//                            TypeOf(Type(link.FullClassName)),
//                            Constant(link.MemberIndex),
//                            Constant(link.FullLambdaName),
//                            Instance(Type("Reflow.Lambdas.LambdaData"))
//                                .WithArguments(
//                                    Constant(link.Data.MinimumSqlLength),
//                                    ArrayInitializer(
//                                        Array(Type(typeof(short))),
//                                        link.Data.ParameterIndecies.Select(
//                                            x => (ExpressionSyntax)Constant(x)
//                                        )
//                                    ),
//                                    ArrayInitializer(
//                                        Array(Type(typeof(Type))),
//                                        link.Data.UsedEntities.Select(
//                                            x => (ExpressionSyntax)TypeOf(Type(x))
//                                        )
//                                    )
//                                )
//                        )
//                );
//            }

//            return File("Reflow.Lambdas")
//                .WithMembers(
//                    Class("LambdaLinks", CSharpModifiers.Public | CSharpModifiers.Static)
//                        .WithMembers(
//                            Field(
//                                    "Links",
//                                    Array(Type("Reflow.Lambdas.LambdaLink")),
//                                    CSharpModifiers.Public | CSharpModifiers.Static
//                                )
//                                .WithInitializer(
//                                    ArrayInitializer(
//                                        Array(Type("Reflow.Lambdas.LambdaLink")),
//                                        syntaxLinks
//                                    )
//                                )
//                        )
//                )
//                .GetText();
//        }
//    }
//}
