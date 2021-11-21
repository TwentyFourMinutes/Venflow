using System.Data.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.CodeGenerator;
using Reflow.Analyzer.Models;
using Reflow.Analyzer.Sections.LambdaSorter;
using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

namespace Reflow.Analyzer.Emitters
{
    internal static class LambdaLinksEmitter
    {
        internal static SourceText Emit(IList<ICommandOperation> commands)
        {
            var syntaxLinks = new List<ExpressionSyntax>();
            var arguments = new List<ExpressionSyntax>(6);

            for (var commandIndex = 0; commandIndex < commands.Count; commandIndex++)
            {
                var command = commands[commandIndex];
                var link = command.FluentCall.LambdaLink;

                arguments.Add(TypeOf(Type(link.ClassName)));
                arguments.Add(Constant(link.IdentifierName));
                arguments.Add(Constant(link.LambdaIndex));
                arguments.Add(Constant(link.HasClosure));

                if (link.Data is not null)
                {
                    if (link.Data is QueryLinkData queryData)
                    {
                        arguments.Add(GetQuerySpecificExpressions(command, queryData));
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

        private static ExpressionSyntax GetQuerySpecificExpressions(
            ICommandOperation command,
            QueryLinkData data
        )
        {
            var query = (Query)command;

            return Instance(Type("Reflow.Lambdas.QueryLinkData"))
                .WithArguments(
                    Constant(data.MinimumSqlLength),
                    ArrayInitializer(
                        Array(Type(typeof(short))),
                        data.ParameterIndecies.Select(x => Constant(x))
                    ),
                    ArrayInitializer(
                        Array(Type(typeof(Type))),
                        data.UsedEntities.Select(x => TypeOf(Type(x)))
                    ),
                    Cast(
                        GenericType(
                            typeof(Func<,,>),
                            Type(typeof(DbDataReader)),
                            Type(typeof(ushort[])),
                            query.Type.HasFlag(QueryType.Single)
                              ? Type(data.Entity)
                              : GenericType(
                                    typeof(Task<>),
                                    GenericType(typeof(IList<>), Type(data.Entity))
                                )
                        ),
                        AccessMember(Type(data.Location!.FullTypeName), data.Location!.MethodName)
                    )
                );
        }
    }
}
