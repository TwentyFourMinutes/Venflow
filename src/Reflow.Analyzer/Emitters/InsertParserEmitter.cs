using System.Data.Common;
using System.Text;
using Microsoft.CodeAnalysis;
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

        private readonly Dictionary<Insert, MethodLocation> _inserterCache;

        private InsertParserEmitter(Database database, List<Insert> inserts)
        {
            _database = database;
            _inserts = inserts;
            _className = $"__{database.Symbol.GetFullName().Replace('.', '_')}";
            _inserters = new();
            _inserterCache = new(InsertEqualityComparer.Default);
        }

        private SourceText Build()
        {
            for (var insertIndex = 0; insertIndex < _inserts.Count; insertIndex++)
            {
                var insert = _inserts[insertIndex];

                if (_inserterCache.TryGetValue(insert, out var methodLocation))
                {
                    _database.Inserts.RemoveAt(insertIndex);

                    continue;
                }

                if (insert.Command.OperationType.HasFlag(OperationType.Single))
                {
                    methodLocation = BuildSingleNoRelationInserter(insert);
                }
                else if (insert.Command.OperationType.HasFlag(OperationType.Many)) { }
                else
                {
                    throw new InvalidOperationException();
                }

                insert.Command.Location = methodLocation;

                _inserterCache.Add(insert, methodLocation);
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

        private class InsertEqualityComparer : IEqualityComparer<Insert>
        {
            internal static InsertEqualityComparer Default = new();

            private InsertEqualityComparer() { }

            bool IEqualityComparer<Insert>.Equals(Insert x, Insert y)
            {
                return x.Command.OperationType == y.Command.OperationType
                    && x.Command.Entity.Equals(y.Command.Entity, SymbolEqualityComparer.Default);
            }

            int IEqualityComparer<Insert>.GetHashCode(Insert obj)
            {
                var hashCode = new HashCode();

                hashCode.Add(obj.Command.Entity);
                hashCode.Add(obj.Command.OperationType);

                return hashCode.ToHashCode();
            }
        }
    }
}
