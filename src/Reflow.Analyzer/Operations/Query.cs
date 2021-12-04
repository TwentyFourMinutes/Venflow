using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Models;
using Reflow.Analyzer.Models.Definitions;

namespace Reflow.Analyzer.Operations
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

        internal static Query Construct(Database database, FluentCallDefinition fluentCall)
        {
            var query = new Query { FluentCall = fluentCall };

            new FluentReader(database, query, fluentCall).Evaluate();

            return query;
        }

        private class FluentReader : FluentSyntaxReader<Query>
        {
            private ITypeSymbol? _previousJoinSymbol;
            private readonly Database _database;

            internal FluentReader(Database database, Query query, FluentCallDefinition fluentCall)
                : base(query, fluentCall)
            {
                _database = database;
            }

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

                    WithLinkData(new QueryLinkData(Value.Entity, -1, null, null));
                }
                else
                {
                    var contentLength = 0;
                    short argumentIndex = 0;
                    var parameterIndecies = new List<short>();
                    var queryHelperStrings = new List<string>();
                    var interpolatedStringSyntax =
                        (InterpolatedStringExpressionSyntax)lambdaSyntax.ExpressionBody!;
                    var stringBuilder = new StringBuilder();

                    for (
                        var contentIndex = 0;
                        contentIndex < interpolatedStringSyntax.Contents.Count;
                        contentIndex++
                    )
                    {
                        var contentSyntax = interpolatedStringSyntax.Contents[contentIndex];

                        if (contentSyntax is InterpolationSyntax interpolationSyntax)
                        {
                            switch (interpolationSyntax.Expression)
                            {
                                case MemberAccessExpressionSyntax memberAccessSyntax:
                                    {
                                        var symbolInfo = SemanticModel.GetSymbolInfo(
                                            memberAccessSyntax.Expression
                                        );

                                        if (
                                            symbolInfo.Symbol is not IParameterSymbol symbol
                                            || !lambdaSyntax
                                                .GetLocation()
                                                .SourceSpan.OverlapsWith(symbol.Locations[0].SourceSpan)
                                        )
                                        {
                                            DefaultSwitchCase();
                                            break;
                                        }

                                        if (
                                            !_database.Entities.TryGetValue(
                                                symbol!.Type,
                                                out var entity
                                            )
                                        )
                                        {
                                            throw new InvalidOperationException();
                                        }

                                        var propertyName = memberAccessSyntax.Name.Identifier.Text;

                                        var column = entity.Columns.FirstOrDefault(
                                            x => x.PropertyName == propertyName
                                        );

                                        if (column is null)
                                            throw new InvalidOperationException();

                                        stringBuilder
                                            .Append('"')
                                            .Append(entity.TableName)
                                            .Append("\".\"")
                                            .Append(column.ColumnName)
                                            .Append('"');

                                        queryHelperStrings.Add(stringBuilder.ToString());
                                        contentLength += stringBuilder.Length;
                                        stringBuilder.Clear();
                                        argumentIndex++;
                                        break;
                                    }
                                case IdentifierNameSyntax identifierSyntax:
                                    {
                                        var symbolInfo = SemanticModel.GetSymbolInfo(identifierSyntax);

                                        if (
                                            symbolInfo.Symbol is not IParameterSymbol symbol
                                            || !lambdaSyntax
                                                .GetLocation()
                                                .SourceSpan.OverlapsWith(symbol.Locations[0].SourceSpan)
                                        )
                                        {
                                            DefaultSwitchCase();
                                            break;
                                        }

                                        if (
                                            !_database.Entities.TryGetValue(
                                                symbol!.Type,
                                                out var entity
                                            )
                                        )
                                        {
                                            throw new InvalidOperationException();
                                        }

                                        var appendAllColumnNames = false;

                                        if (interpolationSyntax.FormatClause is not null)
                                        {
                                            appendAllColumnNames =
                                                interpolationSyntax.FormatClause.FormatStringToken.Text switch
                                                {
                                                    "*" => true,
                                                    _ => throw new InvalidOperationException(),
                                                };
                                        }

                                        if (appendAllColumnNames)
                                        {
                                            for (
                                                var columnIndex = 0;
                                                columnIndex < entity.Columns.Count;
                                                columnIndex++
                                            )
                                            {
                                                stringBuilder
                                                    .Append('"')
                                                    .Append(entity.TableName)
                                                    .Append("\".\"")
                                                    .Append(entity.Columns[columnIndex].ColumnName)
                                                    .Append("\", ");
                                            }

                                            stringBuilder.Length -= 2;
                                        }
                                        else
                                        {
                                            stringBuilder
                                                .Append('"')
                                                .Append(entity.TableName)
                                                .Append('"');
                                        }

                                        queryHelperStrings.Add(stringBuilder.ToString());
                                        contentLength += stringBuilder.Length;
                                        stringBuilder.Clear();
                                        argumentIndex++;
                                        break;
                                    }
                                default:
                                    DefaultSwitchCase();
                                    break;
                            }

                            void DefaultSwitchCase()
                            {
                                parameterIndecies.Add(argumentIndex++);

                                contentLength += 3 + (int)Math.Log10(argumentIndex);
                            }
                        }
                        else if (contentSyntax is InterpolatedStringTextSyntax stringTextSyntax)
                        {
                            contentLength += stringTextSyntax.TextToken.Text.Length;
                        }
                    }

                    if (parameterIndecies.Count == 0 && queryHelperStrings.Count == 0)
                    {
                        throw new InvalidOperationException();
                    }

                    WithLinkData(
                        new QueryLinkData(
                            Value.Entity,
                            contentLength,
                            parameterIndecies.Count == 0 ? null : parameterIndecies,
                            queryHelperStrings.Count == 0 ? null : queryHelperStrings
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
                        var isNew = methodSymbol.Name is "Join";
                        var typeArguments =
                            ((INamedTypeSymbol)methodSymbol.ReturnType).TypeArguments;

                        Value.JoinedEntities.AddToPath(
                            (INamedTypeSymbol)(isNew ? typeArguments[0] : _previousJoinSymbol!),
                            (INamedTypeSymbol)typeArguments[1],
                            (IPropertySymbol)SemanticModel.GetSymbolInfo(
                                GetMemberAccessFromLambda(arguments.Single())
                            ).Symbol!,
                            isNew
                        );

                        if (!isNew)
                        {
                            _previousJoinSymbol = typeArguments[1];
                        }

                        Value.Type |= QueryType.WithRelations;
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
                if (!Value.Type.HasFlag(QueryType.Single) && !Value.Type.HasFlag(QueryType.Many))
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
        internal int MinimumSqlLength { get; }
        internal List<short>? ParameterIndecies { get; }
        internal List<string>? HelperStrings { get; }

        internal QueryLinkData(
            ITypeSymbol entity,
            int minimumSqlLength,
            List<short>? parameterIndecies,
            List<string>? helperStrings
        )
        {
            Entity = entity;
            MinimumSqlLength = minimumSqlLength;
            ParameterIndecies = parameterIndecies;
            HelperStrings = helperStrings;
        }
    }
}
