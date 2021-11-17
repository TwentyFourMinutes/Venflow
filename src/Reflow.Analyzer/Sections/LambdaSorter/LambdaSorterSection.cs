using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Emitters;
using Reflow.Analyzer.Models.Definitions;

namespace Reflow.Analyzer.Sections.LambdaSorter
{
    internal class LambdaSorterSection : GeneratorSection<LambdaCollectionSection>
    {
        protected override NoData Execute(
            GeneratorExecutionContext context,
            NoReceiver syntaxReceiver,
            LambdaCollectionSection previous
        )
        {
            var databases = GetPrevious<DatabaseConfigurationSection>().Data;

            var queries = new List<Query>();
            var lambdaLinks = new List<LambdaLinkDefinition>();

            for (var databaseIndex = 0; databaseIndex < databases.Count; databaseIndex++)
            {
                var database = databases[databaseIndex];
                var fluentCalls = previous.Data[database.Symbol];

                for (
                    var fluentCallIndex = 0;
                    fluentCallIndex < fluentCalls.Count;
                    fluentCallIndex++
                )
                {
                    var fluentCall = fluentCalls[fluentCallIndex];

                    switch (
                        (
                            (MemberAccessExpressionSyntax)fluentCall.Invocations[0].Expression
                        ).Name.Identifier.Text
                    ) {
                        case "Query":
                        case "QueryRaw":
                            queries.Add(Query.Construct(fluentCall));
                            break;
                        default:
                            continue;
                    }

                    lambdaLinks.Add(fluentCall.LambdaLink);
                }

                context.AddNamedSource(
                    database.Symbol.GetFullName().Replace('.', '_'),
                    QueryParserEmitter.Emit(database, queries)
                );

                queries.Clear();
            }

            context.AddNamedSource("LambdaLinks", LambdaLinksEmitter.Emit(lambdaLinks));

            return default;
        }
    }
}
