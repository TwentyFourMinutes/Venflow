using System.Data.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.CodeGenerator;
using Reflow.Analyzer.Models;
using Reflow.Analyzer.Operations;
using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

namespace Reflow.Analyzer.Emitters
{
    internal static class DatabaseConfigurationEmitter
    {
        internal static SourceText Emit(IList<Database> databases)
        {
            var configurationEntries = new List<InitializerExpressionSyntax>();

            for (var databaseIndex = 0; databaseIndex < databases.Count; databaseIndex++)
            {
                var database = databases[databaseIndex];

                configurationEntries.Add(
                    DictionaryEntry(
                        TypeOf(Type(database.Symbol)),
                        Instance(Type("Reflow.DatabaseConfiguration"))
                            .WithArguments(
                                GetInstantiaterSytanx(database),
                                GetEntitiesSytanx(database.Entities),
                                LambdaLinksEmitter.Emit(
                                    database.Queries.OfType<Models.IOperation>()
                                ),
                                GetCommandSytanx(
                                    database.Inserts
                                        .Where(
                                            x =>
                                                x.Command.OperationType.HasFlag(
                                                    OperationType.Single
                                                )
                                        )
                                        .Select(x => x.Command),
                                    false
                                ),
                                GetCommandSytanx(
                                    database.Inserts
                                        .Where(
                                            x => x.Command.OperationType.HasFlag(OperationType.Many)
                                        )
                                        .Select(x => x.Command),
                                    false
                                ),
                                GetCommandSytanx(
                                    database.Updates
                                        .Where(
                                            x =>
                                                x.Command.OperationType.HasFlag(
                                                    OperationType.Single
                                                )
                                        )
                                        .Select(x => x.Command),
                                    true
                                ),
                                GetCommandSytanx(
                                    database.Updates
                                        .Where(
                                            x =>
                                                x.Command.OperationType.HasFlag(
                                                    OperationType.Single
                                                )
                                        )
                                        .Select(x => x.Command),
                                    true
                                ),
                                GetCommandSytanx(
                                    database.Deletes
                                        .Where(
                                            x =>
                                                x.Command.OperationType.HasFlag(
                                                    OperationType.Single
                                                )
                                        )
                                        .Select(x => x.Command),
                                    true
                                ),
                                GetCommandSytanx(
                                    database.Deletes
                                        .Where(
                                            x => x.Command.OperationType.HasFlag(OperationType.Many)
                                        )
                                        .Select(x => x.Command),
                                    true
                                )
                            )
                    )
                );
            }

            return File("Reflow")
                .WithMembers(
                    Class("DatabaseConfigurations", CSharpModifiers.Public | CSharpModifiers.Static)
                        .WithMembers(
                            Field(
                                    "Configurations",
                                    DictionaryType(),
                                    CSharpModifiers.Public | CSharpModifiers.Static
                                )
                                .WithInitializer(
                                    Instance(DictionaryType())
                                        .WithArguments(Constant(databases.Count))
                                        .WithInitializer(
                                            DictionaryInitializer(configurationEntries)
                                        )
                                )
                        )
                )
                .GetText();

            static TypeSyntax DictionaryType() =>
                GenericType(
                    typeof(Dictionary<,>),
                    Type(typeof(Type)),
                    Type("Reflow.DatabaseConfiguration")
                );
        }

        private static CSharpLambdaSyntax GetInstantiaterSytanx(Database database)
        {
            const string databaseParameter = "baseDatabase";
            const string databaseLocal = "database";

            var statements = new List<StatementSyntax>
            {
                Local(databaseLocal, Var())
                    .WithInitializer(Cast(Type(database.Symbol), Variable(databaseParameter)))
            };

            for (
                var entitySymbolIndex = 0;
                entitySymbolIndex < database.EntitySymbols.Count;
                entitySymbolIndex++
            )
            {
                var (propertySymbol, entitySymbol) = database.EntitySymbols[entitySymbolIndex];

                statements.Add(
                    AssignMember(
                        databaseLocal,
                        propertySymbol,
                        Instance(GenericType("Reflow.Table", Type(entitySymbol)))
                            .WithArguments(Variable(databaseLocal))
                    )
                );
            }

            return Lambda(databaseParameter).WithStatements(statements);
        }

        private static CSharpInstanceSyntax GetEntitiesSytanx(
            Dictionary<ITypeSymbol, Entity> entities
        )
        {
            var entityData = new InitializerExpressionSyntax[entities.Count];
            var entityIndex = 0;

            foreach (var entity in entities.Values)
            {
                entityData[entityIndex++] = DictionaryEntry(
                    TypeOf(Type(entity.Symbol)),
                    Instance(Type("Reflow.Entity"))
                        .WithArguments(
                            Instance(
                                    GenericType(
                                        typeof(Dictionary<,>),
                                        Type(typeof(string)),
                                        Type("Reflow.Column")
                                    )
                                )
                                .WithArguments(Constant(entity.Columns.Count))
                                .WithInitializer(
                                    DictionaryInitializer(
                                        entity.Columns.Select(
                                            (x, i) =>
                                                DictionaryEntry(
                                                    Constant(x.ColumnName),
                                                    Instance(Type("Reflow.Column"))
                                                        .WithArguments(
                                                            Constant(x.ColumnName),
                                                            Constant(i)
                                                        )
                                                )
                                        )
                                    )
                                )
                        )
                );
            }

            return Instance(DictionaryType())
                .WithArguments(Constant(entityData.Length))
                .WithInitializer(DictionaryInitializer(entityData));

            static TypeSyntax DictionaryType() =>
                GenericType(typeof(Dictionary<,>), Type(typeof(Type)), Type("Reflow.Entity"));
        }

        private static CSharpInstanceSyntax GetCommandSytanx(
            IEnumerable<Command> commands,
            bool isAction
        )
        {
            var insertInitializers = new List<InitializerExpressionSyntax>();

            foreach (var command in commands)
            {
                var operationType = command.OperationType.HasFlag(OperationType.Single)
                  ? Type(command.Entity)
                  : GenericType(typeof(IList<>), Type(command.Entity));

                insertInitializers.Add(
                    DictionaryEntry(
                        TypeOf(Type(command.Entity)),
                        Cast(
                            isAction
                              ? GenericType(typeof(Action<,>), Type<DbCommand>(), operationType)
                              : GenericType(
                                    typeof(Func<,,>),
                                    Type<DbCommand>(),
                                    operationType,
                                    Type<Task>()
                                ),
                            AccessMember(
                                Type(command.Location!.FullTypeName),
                                command.Location!.MethodName
                            )
                        )
                    )
                );
            }

            return Instance(DictionaryType())
                .WithArguments(Constant(insertInitializers.Count))
                .WithInitializer(DictionaryInitializer(insertInitializers));

            static TypeSyntax DictionaryType() =>
                GenericType(typeof(Dictionary<,>), Type<Type>(), Type<Delegate>());
        }
    }
}
