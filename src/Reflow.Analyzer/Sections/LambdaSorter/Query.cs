using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Models;
using Reflow.Analyzer.Models.Definitions;

namespace Reflow.Analyzer.Sections.LambdaSorter
{
    internal class Query
    {
        internal FluentCallDefinition FluentCall { get; private set; }
        internal QueryType Type { get; private set; }
        internal ITypeSymbol Entity { get; private set; }
        internal bool TrackChanges { get; private set; }

        public Query()
        {
            FluentCall = null!;
            Entity = null!;
        }

        internal static Query Construct(FluentCallDefinition fluentCall)
        {
            var query = new FluentReader(fluentCall).Evaluate();

            query.FluentCall = fluentCall;

            return query;
        }

        private class FluentReader : FluentSyntaxReader<Query>
        {
            internal FluentReader(FluentCallDefinition fluentCall) : base(fluentCall) { }

            protected override bool ValidateHead(
                LambdaExpressionSyntax lambdaSyntax,
                string name,
                ArgumentListSyntax list
            )
            {
                if (name is not "Query" or "QueryRaw")
                {
                    return false;
                }

                var tableSyntax =
                    (
                        (MemberAccessExpressionSyntax)lambdaSyntax.FirstAncestorOrSelf<InvocationExpressionSyntax>()!.Expression
                    ).Expression;
                var tableType = (INamedTypeSymbol)SemanticModel.GetTypeInfo(tableSyntax).Type!;

                Value.Entity = tableType.TypeArguments[0];

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

                WithLinkData(
                    new QueryLinkData(
                        Value.Entity,
                        contentLength,
                        parameterIndecies.ToArray(),
                        new string[] { Value.Entity.GetFullName() }
                    )
                );

                return true;
            }

            protected override void ReadTail(string name, ArgumentListSyntax list)
            {
                switch (name)
                {
                    case "TrackChanges":
                        Value.TrackChanges =
                            list.Arguments.Count == 0
                            || (bool)(
                                (LiteralExpressionSyntax)list.Arguments[0].Expression
                            ).Token.Value!;
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
        internal short[] ParameterIndecies { get; }

        internal QueryLinkData(
            ITypeSymbol entity,
            int minimumSqlLength,
            short[] parameterIndecies,
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
