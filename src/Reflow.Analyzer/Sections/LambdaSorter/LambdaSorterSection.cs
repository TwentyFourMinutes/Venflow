﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Emitters;

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
                }

                context.AddNamedSource(
                    database.Symbol.GetFullName().Replace('.', '_'),
                    QueriesEmitter.Emit(database, queries)
                );

                queries.Clear();
            }

            return default;
        }
    }
}
