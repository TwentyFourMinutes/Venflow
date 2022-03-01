using System.Data.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Models;
using Reflow.Analyzer.Operations;
using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

namespace Reflow.Analyzer.Emitters
{
    internal static class LambdaLinksEmitter
    {
        internal static ArrayCreationExpressionSyntax Emit(IEnumerable<IOperation> operations)
        {
            var syntaxLinks = new List<ExpressionSyntax>();
            var arguments = new List<ExpressionSyntax>(6);

            foreach (var operation in operations)
            {
                var link = operation.FluentCall.LambdaLink;

                arguments.Add(TypeOf(Type(link.ClassName)));
                arguments.Add(Constant(link.IdentifierName));
                arguments.Add(Constant(link.LambdaIndex));
                arguments.Add(Constant(link.HasClosure));

                if (link.Data is not null)
                {
                    if (link.Data is QueryLinkData queryData)
                    {
                        arguments.Add(GetQuerySpecificExpressions(operation, queryData));
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

            return ArrayInitializer(Array(Type("Reflow.Lambdas.LambdaLink")), syntaxLinks);
        }

        private static ExpressionSyntax GetQuerySpecificExpressions(
            IOperation operation,
            QueryLinkData data
        )
        {
            var query = (Query)operation;

            TypeSyntax returnType;

            if (query.Type.HasFlag(OperationType.Single))
            {
                if (query.Type.HasFlag(OperationType.WithRelations))
                {
                    returnType = GenericType(typeof(Task<>), Type(data.Entity));
                }
                else
                {
                    returnType = Type(data.Entity);
                }
            }
            else
            {
                returnType = GenericType(
                    typeof(Task<>),
                    GenericType(typeof(IList<>), Type(data.Entity))
                );
            }

            return Instance(Type("Reflow.Lambdas.QueryLinkData"))
                .WithArguments(
                    Constant(data.Caching),
                    Constant(data.MinimumSqlLength),
                    data.ParameterIndecies is null
                      ? Null()
                      : ArrayInitializer(
                            Array(Type(typeof(short))),
                            data.ParameterIndecies.Select(Constant)
                        ),
                    data.HelperStrings is null
                      ? Null()
                      : ArrayInitializer(
                            Array(Type(typeof(string))),
                            data.HelperStrings.Select(Constant)
                        ),
                    ArrayInitializer(
                        Array(Type(typeof(Type))),
                        new[] { query.Entity }
                            .Concat(
                                query.JoinedEntities.FlattenedPath.Select(x => x.RightEntitySymbol)
                            )
                            .Select(x => TypeOf(Type(x)))
                    ),
                    Cast(
                        GenericType(
                            typeof(Func<,,>),
                            Type(typeof(DbDataReader)),
                            Type(typeof(ushort[])),
                            returnType
                        ),
                        AccessMember(Type(data.Location!.FullTypeName), data.Location!.MethodName)
                    )
                );
        }
    }
}
