using Microsoft.CodeAnalysis;
using Reflow.Analyzer.Emitters;
using Reflow.Analyzer.Operations;
using Reflow.Analyzer.Shared;

namespace Reflow.Analyzer.Sections
{
    internal class OperationEmitterSection : GeneratorSection<QueryObserverSection>
    {
        protected override NoData Execute(
            GeneratorExecutionContext context,
            NoReceiver syntaxReceiver,
            QueryObserverSection previous
        )
        {
            var databases = GetPrevious<DatabaseConfigurationSection>().Data;
            var commandData = GetPrevious<CommandObserverSection>().Data;
            var operations = new List<Models.IOperation>();

            for (var databaseIndex = 0; databaseIndex < databases.Count; databaseIndex++)
            {
                var database = databases[databaseIndex];

                if (previous.Data.TryGetValue(database.Symbol, out var fluentCalls))
                {
                    for (
                        var fluentCallIndex = 0;
                        fluentCallIndex < fluentCalls.Count;
                        fluentCallIndex++
                    )
                    {
                        var fluentCall = fluentCalls[fluentCallIndex];

                        var query = Query.Construct(database, fluentCall);
                        database.Queries.Add(query);
                        operations.Add(query);
                    }
                }

                if (commandData.TryGetValue(database.Symbol, out var commands))
                {
                    for (var commandIndex = 0; commandIndex < commands.Count; commandIndex++)
                    {
                        var command = commands[commandIndex];

                        switch (command.Type)
                        {
                            case Command.CommandType.Insert:
                                var insert = Insert.Construct(database, command);
                                database.Inserts.Add(insert);
                                break;
                            case Command.CommandType.Update:
                                var update = Update.Construct(database, command);
                                database.Updates.Add(update);
                                break;
                            case Command.CommandType.Delete:
                                var delete = Delete.Construct(database, command);
                                database.Deletes.Add(delete);
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                    }
                }

                AddSource(
                    database.Symbol.GetFullName().Replace('.', '_'),
                    QueryParserEmitter.Emit(database)
                );

                AddSource(
                    database.Symbol.GetFullName().Replace('.', '_') + "_Inserters",
                    InsertParserEmitter.Emit(database)
                );

                AddSource(
                    database.Symbol.GetFullName().Replace('.', '_') + "_Updates",
                    UpdateEmitter.Emit(database)
                );

                AddSource(
                    database.Symbol.GetFullName().Replace('.', '_') + "_Deletes",
                    DeleteEmitter.Emit(database)
                );
            }

            return default;
        }
    }
}
