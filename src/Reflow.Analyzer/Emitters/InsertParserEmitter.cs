using System.Data;
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

                if (_inserterCache.TryGetValue(insert, out _))
                {
                    _database.Inserts.RemoveAt(insertIndex);

                    continue;
                }

                MethodLocation methodLocation;

                if (insert.Command.OperationType.HasFlag(OperationType.Single))
                {
                    methodLocation = BuildSingleNoRelationInserter(insert);
                }
                else if (insert.Command.OperationType.HasFlag(OperationType.Many))
                {
                    methodLocation = BuildManyNoRelationInserter(insert);
                }
                else
                {
                    throw new InvalidOperationException();
                }

                insert.Command.Location = methodLocation;

                _inserterCache.Add(insert, methodLocation);
            }

            var a = File("Reflow.Inserters")
                .WithMembers(
                    Class(_className, CSharpModifiers.Internal | CSharpModifiers.Static)
                        .WithMembers(_inserters)
                )
                .GetText()
                .ToString();

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

        private MethodLocation BuildManyNoRelationInserter(Insert insert)
        {
            var entity = insert.VirtualEntities[0].Entity;

            var methodDefinition = new MethodLocation(
                "Reflow.Inserters." + _className,
                "Inserter_" + _inserterIndex++
            );

            var partialCommandText = new StringBuilder("INSERT INTO \"");

            partialCommandText.Append(entity.TableName);
            partialCommandText.Append("\" (");

            var argumentsInitializers = new StatementSyntax[(entity.Columns.Count - 1) * 3];

            var argumentsIndex = 0;

            for (var columnIndex = 1; columnIndex < entity.Columns.Count; columnIndex++)
            {
                var column = entity.Columns[columnIndex];

                partialCommandText.Append('"').Append(column.ColumnName).Append("\", ");

                argumentsInitializers[argumentsIndex++] = Statement(
                    AssignLocal(
                        Variable("name"),
                        Add(Constant("@p" + column.ColumnName), Variable("absoluteIndex"))
                    )
                );
                argumentsInitializers[argumentsIndex++] = Statement(
                    Invoke(
                        Variable("parameters"),
                        nameof(DbParameterCollection.Add),
                        Instance(GenericType("Npgsql.NpgsqlParameter", Type(column.Type)))
                            .WithArguments(
                                Variable("name"),
                                AccessMember(Variable("entity"), column.PropertyName)
                            )
                    )
                );

                ExpressionSyntax firstParameterSyntax =
                    argumentsIndex == 2
                        ? Invoke(
                              Variable("commandText"),
                              nameof(StringBuilder.Append),
                              Constant('(')
                          )
                        : Variable("commandText");

                ExpressionSyntax lastParameterSyntax =
                    argumentsIndex + 1 == (entity.Columns.Count - 1) * 3
                        ? Constant("), ")
                        : Constant(", ");

                argumentsInitializers[argumentsIndex++] = Statement(
                    Invoke(
                        Invoke(
                            firstParameterSyntax,
                            nameof(StringBuilder.Append),
                            Variable("name")
                        ),
                        nameof(StringBuilder.Append),
                        lastParameterSyntax
                    )
                );
            }

            partialCommandText.Length -= 2;
            partialCommandText.Append(") VALUES ");

            var totalColumns = entity.Columns.Count - 1;

            _inserters.Add(
                Method(
                        methodDefinition.MethodName,
                        Type<Task>(),
                        CSharpModifiers.Internal | CSharpModifiers.Static | CSharpModifiers.Async
                    )
                    .WithParameters(
                        Parameter("command", Type<DbCommand>()),
                        Parameter(
                            "entities",
                            GenericType(typeof(IList<>), Type(insert.Command.Entity))
                        )
                    )
                    .WithStatements(
                        Local("parameters", Var())
                            .WithInitializer(
                                AccessMember(Variable("command"), nameof(DbCommand.Parameters))
                            ),
                        Local("commandText", Var())
                            .WithInitializer(Instance(Type<StringBuilder>())),
                        Local("totalEntities", Var())
                            .WithInitializer(AccessMember(Variable("entities"), "Count")),
                        Local("absoluteIndex", Var()).WithInitializer(Constant(0)),
                        While(
                            NotEqual(Variable("absoluteIndex"), Variable("totalEntities")),
                            Statement(
                                Invoke(
                                    Variable("commandText"),
                                    nameof(StringBuilder.Append),
                                    Constant(partialCommandText.ToString())
                                )
                            ),
                            Local("available", Var())
                                .WithInitializer(
                                    Add(
                                        Divide(
                                            Invoke(
                                                Type(typeof(Math)),
                                                nameof(Math.Min),
                                                Multiply(
                                                    Parenthesis(
                                                        Substract(
                                                            Variable("totalEntities"),
                                                            Variable("absoluteIndex")
                                                        )
                                                    ),
                                                    Constant(totalColumns)
                                                ),
                                                Constant(ushort.MaxValue)
                                            ),
                                            Constant(totalColumns)
                                        ),
                                        Variable("absoluteIndex")
                                    )
                                ),
                            For(
                                LessThen(Variable("absoluteIndex"), Variable("available")),
                                Increment(Variable("absoluteIndex")),
                                Concat(
                                    Concat(
                                        Local("entity", Var())
                                            .WithInitializer(
                                                AccessElement(
                                                    Variable("entities"),
                                                    Variable("absoluteIndex")
                                                )
                                            ),
                                        Local("name", Type<string>())
                                    ),
                                    argumentsInitializers
                                )
                            ),
                            AssignMember(
                                Variable("commandText"),
                                nameof(StringBuilder.Length),
                                Substract(
                                    AccessMember(
                                        Variable("commandText"),
                                        nameof(StringBuilder.Length)
                                    ),
                                    Constant(2)
                                )
                            ),
                            Statement(
                                Invoke(
                                    Variable("commandText"),
                                    nameof(StringBuilder.Append),
                                    Constant(" RETURNING \"" + entity.Columns[0].ColumnName + "\";")
                                )
                            )
                        ),
                        AssignMember(
                            Variable("command"),
                            nameof(DbCommand.CommandText),
                            Invoke(Variable("commandText"), nameof(StringBuilder.ToString))
                        ),
                        Local("reader", Var())
                            .WithInitializer(
                                Await(
                                    Invoke(
                                        Variable("command"),
                                        nameof(DbCommand.ExecuteReaderAsync),
                                        Conditional(
                                            LessOrEqualThen(
                                                Variable("absoluteIndex"),
                                                Constant(ushort.MaxValue)
                                            ),
                                            EnumMember(CommandBehavior.SequentialAccess),
                                            BitwiseOr(
                                                EnumMember(CommandBehavior.SingleResult),
                                                EnumMember(CommandBehavior.SequentialAccess)
                                            )
                                        ),
                                        Default()
                                    )
                                )
                            ),
                        For(
                            Local("entityIndex", Var()).WithInitializer(Constant(0)),
                            LessThen(Variable("entityIndex"), Variable("totalEntities")),
                            Increment(Variable("entityIndex")),
                            If(
                                And(
                                    NotEqual(Variable("entityIndex"), Constant(0)),
                                    Equal(
                                        Modulo(
                                            Variable("entityIndex"),
                                            Constant(ushort.MaxValue / 2)
                                        ),
                                        Constant(0)
                                    )
                                ),
                                Statement(
                                    Await(
                                        Invoke(
                                            Variable("reader"),
                                            nameof(DbDataReader.NextResultAsync),
                                            Default()
                                        )
                                    )
                                )
                            ),
                            Statement(
                                Await(
                                    Invoke(
                                        Variable("reader"),
                                        nameof(DbDataReader.ReadAsync),
                                        Default()
                                    )
                                )
                            ),
                            AssignMember(
                                AccessElement(Variable("entities"), Variable("entityIndex")),
                                entity.Columns[0].ColumnName,
                                Invoke(
                                    Variable("reader"),
                                    GenericName(
                                        nameof(DbDataReader.GetFieldValue),
                                        Type(entity.Columns[0].Type)
                                    ),
                                    Constant(0)
                                )
                            )
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
