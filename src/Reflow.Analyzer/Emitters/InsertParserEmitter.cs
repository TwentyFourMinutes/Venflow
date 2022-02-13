using System.Data.Common;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.CodeGenerator;
using Reflow.Analyzer.Models;
using Reflow.Analyzer.Operations;
using Reflow.Analyzer.Shared;
using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

namespace Reflow.Analyzer.Emitters
{
    internal class InsertParserEmitter
    {
        private ushort _inserterIndex;

        private readonly string _className;
        private readonly Database _database;
        private readonly List<Insert> _inserts;
        private readonly List<MethodDeclarationSyntax> _inserters;

        //private readonly Dictionary<Command<EntityRelationHolder>, MethodLocation> _inserterCache;

        private InsertParserEmitter(Database database, List<Insert> inserts)
        {
            _database = database;
            _inserts = inserts;
            _className = $"__{database.Symbol.GetFullName().Replace('.', '_')}";
            _inserters = new(inserts.Count);
            //_inserterCache = new(commands.Count, CommandEqualityComparer.Default);
        }

        private SourceText Build()
        {
            for (var commandIndex = 0; commandIndex < _inserts.Count; commandIndex++)
            {
                var insert = _inserts[commandIndex];

                //if (_inserterCache.TryGetValue(command, out var methodLocation))
                //{
                //    continue;
                //}

                if (insert.Command.OperationType.HasFlag(OperationType.Single))
                {
                    BuildSingleNoRelationInserter(insert);
                }
                else if (insert.Command.OperationType.HasFlag(OperationType.Many)) { }
                else
                {
                    throw new InvalidOperationException();
                }
                //((QueryLinkData)command.FluentCall.LambdaLink.Data!).Location = methodLocation;

                //_inserterCache.Add(command, methodLocation);
            }

            return File("Reflow.Inserters")
                .WithMembers(
                    Class(_className, CSharpModifiers.Internal | CSharpModifiers.Static)
                        .WithMembers(_inserters)
                )
                .GetText();
        }

        private MethodLocation BuildSingleNoRelationInserter(Insert insert)
        {
            var entity = insert.VirtualEntities[0].Entity;

            var methodDefinition = new MethodLocation(
                "Reflow.Inserters." + _className,
                "Inserter_" + _inserterIndex++
            );
            var statements = new StatementSyntax[entity.Columns.Count];

            var commandText = new StringBuilder();

            commandText.Append("INSERT INTO \"").Append(entity.TableName).Append("\" (");

            for (var columnIndex = 1; columnIndex < entity.Columns.Count; columnIndex++)
            {
                var column = entity.Columns[columnIndex];

                commandText.Append('"').Append(column.ColumnName).Append("\", ");
            }

            commandText.Length -= 2;

            commandText.Append(") VALUES (");

            for (var columnIndex = 1; columnIndex < entity.Columns.Count; columnIndex++)
            {
                var column = entity.Columns[columnIndex];

                var parameterName = "@p" + column.ColumnName;

                commandText.Append(parameterName).Append(", ");

                statements[columnIndex - 1] = Statement(
                    Invoke(
                        Variable("parameters"),
                        nameof(DbParameterCollection.Add),
                        Instance(Type("Npgsql.NpgsqlParameter"))
                            .WithArguments(
                                Constant(parameterName),
                                AccessMember(Variable("entity"), column.PropertyName)
                            )
                    )
                );
            }

            commandText.Length -= 2;

            var primaryKey = entity.Columns[0];

            commandText.Append(") RETURNING \"").Append(primaryKey.ColumnName).Append("\";");

            statements[statements.Length - 1] = AssignMember(
                Variable("entity"),
                primaryKey.PropertyName,
                Cast(
                    Type(primaryKey.Type),
                    Await(Invoke(Variable("command"), nameof(DbCommand.ExecuteScalarAsync)))
                )
            );

            _inserters.Add(
                Method(
                        methodDefinition.MethodName,
                        Type<Task>(),
                        CSharpModifiers.Internal | CSharpModifiers.Static | CSharpModifiers.Async
                    )
                    .WithParameters(
                        Parameter("command", Type<DbCommand>()),
                        Parameter("entity", Type(insert.Command.Entity))
                    )
                    .WithStatements(
                        Concat(
                            Concat(
                                Local("parameters", Var())
                                    .WithInitializer(
                                        AccessMember(
                                            Variable("command"),
                                            nameof(DbCommand.Parameters)
                                        )
                                    ),
                                AssignMember(
                                    Variable("command"),
                                    nameof(DbCommand.CommandText),
                                    Constant(commandText.ToString())
                                )
                            ),
                            statements
                        )
                    )
            );

            return methodDefinition;
        }

        internal static SourceText Emit(Database database)
        {
            return new InsertParserEmitter(database, database.Inserts).Build();
        }

        //private class CommandEqualityComparer : IEqualityComparer<Command<EntityRelationHolder>>
        //{
        //    internal static CommandEqualityComparer Default = new();

        //    private CommandEqualityComparer() { }

        //    bool IEqualityComparer<Command>.Equals(Command<EntityRelationHolder> x, Command<EntityRelationHolder> y)
        //    {
        //        return x.Entity.Equals(y.Entity, SymbolEqualityComparer.Default) && x.OperationType == y.OperationType;
        //    }

        //    int IEqualityComparer<Command>.GetHashCode(Command<EntityRelationHolder> obj)
        //    {
        //        var hashCode = new HashCode();

        //        hashCode.Add(obj.Entity);
        //        hashCode.Add(obj.OperationType);

        //        return hashCode.ToHashCode();
        //    }
        //}
    }
}
