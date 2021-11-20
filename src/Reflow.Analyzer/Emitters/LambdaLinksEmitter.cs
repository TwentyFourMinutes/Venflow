using System.Data.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.CodeGenerator;
using Reflow.Analyzer.Models.Definitions;
using Reflow.Analyzer.Sections.LambdaSorter;
using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

namespace Reflow.Analyzer.Emitters
{
    internal static class LambdaLinksEmitter
    {
        internal static SourceText Emit(IList<LambdaLinkDefinition> links)
        {
            var syntaxLinks = new List<ExpressionSyntax>();
            var arguments = new List<ExpressionSyntax>(6);

            for (var linkIndex = 0; linkIndex < links.Count; linkIndex++)
            {
                var link = links[linkIndex];

                arguments.Add(TypeOf(Type(link.ClassName)));
                arguments.Add(Constant(link.IdentifierName));
                arguments.Add(Constant(link.LambdaIndex));
                arguments.Add(Constant(link.HasClosure));

                if (link.Data is not null)
                {
                    if (link.Data is QueryLinkData queryData)
                    {
                        arguments.Add(
                            Instance(Type("Reflow.Lambdas.QueryLinkData"))
                                .WithArguments(
                                    Constant(queryData.MinimumSqlLength),
                                    ArrayInitializer(
                                        Array(Type(typeof(short))),
                                        queryData.ParameterIndecies.Select(x => Constant(x))
                                    ),
                                    ArrayInitializer(
                                        Array(Type(typeof(Type))),
                                        queryData.UsedEntities.Select(x => TypeOf(Type(x)))
                                    ),
                                    Cast(
                                        GenericType(
                                            typeof(Func<,,>),
                                            Type(typeof(DbDataReader)),
                                            Type(typeof(ushort[])),
                                            Type(queryData.Entity)
                                        ),
                                        AccessMember(
                                            Type(queryData.Location!.FullTypeName),
                                            queryData.Location!.MethodName
                                        )
                                    )
                                )
                        );
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }

                syntaxLinks.Add(
                    Instance(Type("Reflow.Lambdas.LambdaLink")).WithArguments(arguments)
                );

                arguments.Clear();
            }

            return File("Reflow.Lambdas")
                .WithMembers(
                    Class("LambdaLinks", CSharpModifiers.Public | CSharpModifiers.Static)
                        .WithMembers(
                            Field(
                                    "Links",
                                    Array(Type("Reflow.Lambdas.LambdaLink")),
                                    CSharpModifiers.Public | CSharpModifiers.Static
                                )
                                .WithInitializer(
                                    ArrayInitializer(
                                        Array(Type("Reflow.Lambdas.LambdaLink")),
                                        syntaxLinks
                                    )
                                )
                        )
                )
                .GetText();
        }
    }
}
