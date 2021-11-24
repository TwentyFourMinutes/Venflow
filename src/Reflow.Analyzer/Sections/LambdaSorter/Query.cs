using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Models;
using Reflow.Analyzer.Models.Definitions;

namespace Reflow.Analyzer.Sections.LambdaSorter
{
    internal class Query : ICommandOperation
    {
        public FluentCallDefinition FluentCall { get; private set; }
        internal QueryType Type { get; private set; }
        internal ITypeSymbol Entity { get; private set; }
        internal bool TrackChanges { get; private set; }
        internal RelationBuilderValues JoinedEntities { get; }

        public Query()
        {
            FluentCall = null!;
            Entity = null!;
            JoinedEntities = new();
        }

        internal static Query Construct(FluentCallDefinition fluentCall)
        {
            var query = new Query { FluentCall = fluentCall };

            new FluentReader(query, fluentCall).Evaluate();

            return query;
        }

        private class FluentReader : FluentSyntaxReader<Query>
        {
            private ITypeSymbol? _previousJoinSymbol;

            internal FluentReader(Query query, FluentCallDefinition fluentCall)
                : base(query, fluentCall) { }

            protected override bool ValidateHead(
                LambdaExpressionSyntax lambdaSyntax,
                IMethodSymbol methodSymbol,
                SeparatedSyntaxList<ArgumentSyntax> arguments
            )
            {
                if (methodSymbol.Name is not "Query" and not "QueryRaw")
                {
                    return false;
                }

                var tableSyntax =
                    (
                        (MemberAccessExpressionSyntax)lambdaSyntax.FirstAncestorOrSelf<InvocationExpressionSyntax>()!.Expression
                    ).Expression;
                var tableType = (INamedTypeSymbol)SemanticModel.GetTypeInfo(tableSyntax).Type!;

                Value.Entity = tableType.TypeArguments[0];

                if (methodSymbol.Name is "QueryRaw")
                {
                    if (
                        lambdaSyntax.ExpressionBody is not LiteralExpressionSyntax literalSyntax
                        || literalSyntax.Kind() != SyntaxKind.StringLiteralExpression
                    )
                    {
                        throw new InvalidOperationException();
                    }

                    WithLinkData(
                        new QueryLinkData(
                            Value.Entity,
                            -1,
                            null,
                            new string[] { Value.Entity.GetFullName() }
                        )
                    );
                }
                else
                {
                    var contentLength = 0;
                    short argumentIndex = 0;
                    var parameterIndecies = new List<short>();
                    var interpolatedStringSyntax =
                        (InterpolatedStringExpressionSyntax)lambdaSyntax.ExpressionBody!;

                    for (
                        var contentIndex = 0;
                        contentIndex < interpolatedStringSyntax.Contents.Count;
                        contentIndex++
                    )
                    {
                        var content = interpolatedStringSyntax.Contents[contentIndex];

                        if (content is InterpolationSyntax)
                        {
                            parameterIndecies.Add(argumentIndex++);

                            contentLength += 3 + (int)Math.Log10(argumentIndex);
                        }
                        else if (content is InterpolatedStringContentSyntax stringContentSyntax)
                        {
                            contentLength += stringContentSyntax.GetText().Length;
                        }
                    }

                    if (parameterIndecies.Count == 0)
                    {
                        throw new InvalidOperationException();
                    }

                    WithLinkData(
                        new QueryLinkData(
                            Value.Entity,
                            contentLength,
                            parameterIndecies.ToArray(),
                            new string[] { Value.Entity.GetFullName() }
                        )
                    );
                }

                return true;
            }

            protected override void ReadTail(
                IMethodSymbol methodSymbol,
                SeparatedSyntaxList<ArgumentSyntax> arguments
            )
            {
                switch (methodSymbol.Name)
                {
                    case "TrackChanges":
                        Value.TrackChanges =
                            arguments.Count == 0
                            || (bool)(
                                (LiteralExpressionSyntax)arguments[0].Expression
                            ).Token.Value!;
                        return;
                    case "Join":
                    case "ThenJoin":
                        var isNested = methodSymbol.Name is "ThenJoin";
                        var typeArguments = ((INamedTypeSymbol)methodSymbol.ReturnType).TypeArguments;

                        Value.JoinedEntities.AddToPath(
                            (INamedTypeSymbol)(isNested ? _previousJoinSymbol! : typeArguments[0]),
                            (INamedTypeSymbol)typeArguments[1],
                            (IPropertySymbol)SemanticModel.GetSymbolInfo(
                                GetMemberAccessFromLambda(arguments.Single())
                            ).Symbol!, isNested);

                        if (!isNested)
                        {
                            _previousJoinSymbol = typeArguments[1];
                        }

                        return;
                    case "SingleAsync":
                        Value.Type |= QueryType.Single;
                        return;
                    case "ManyAsync":
                        Value.Type |= QueryType.Many;
                        return;
                }
            }

            protected override bool ValidateTail()
            {
                if (Value.Type is not QueryType.Single and not QueryType.Many)
                {
                    return false;
                }

                return true;
            }

            private static MemberAccessExpressionSyntax GetMemberAccessFromLambda(
                ArgumentSyntax argumentSyntax
            )
            {
                var lambda = (SimpleLambdaExpressionSyntax)argumentSyntax.Expression;
                var memberAccess = (MemberAccessExpressionSyntax)lambda.ExpressionBody!;

                if (memberAccess.Expression is not IdentifierNameSyntax)
                    throw new InvalidOperationException();

                return memberAccess;
            }
        }
    }

    [Flags]
    internal enum QueryType : byte
    {
        None = 0,
        Single = 1 << 0,
        Many = 1 << 1,
        WithRelations = 1 << 2,
    }

    internal class QueryLinkData : ILambdaLinkData
    {
        internal MethodLocation? Location { get; set; }

        internal ITypeSymbol Entity { get; }
        internal string[]? UsedEntities { get; }
        internal int MinimumSqlLength { get; }
        internal short[]? ParameterIndecies { get; }

        internal QueryLinkData(
            ITypeSymbol entity,
            int minimumSqlLength,
            short[]? parameterIndecies,
            string[] usedEntities
        )
        {
            Entity = entity;
            MinimumSqlLength = minimumSqlLength;
            ParameterIndecies = parameterIndecies;
            UsedEntities = usedEntities;
        }
    }
}
