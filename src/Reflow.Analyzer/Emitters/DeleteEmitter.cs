using System.Collections;
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
    internal class DeleteEmitter
    {
        private ushort _deleteIndex;

        private readonly string _className;
        private readonly Database _database;
        private readonly List<Delete> _deletes;
        private readonly List<MethodDeclarationSyntax> _deleters;

        private readonly Dictionary<Delete, MethodLocation> _deletersCache;

        private DeleteEmitter(Database database, List<Delete> deletes)
        {
            _database = database;
            _deletes = deletes;
            _className = $"__{database.Symbol.GetFullName().Replace('.', '_')}";
            _deleters = new();
            _deletersCache = new(DeleteEqualityComparer.Default);
        }

        private SourceText Build()
        {
            for (var deleteIndex = 0; deleteIndex < _deletes.Count; deleteIndex++)
            {
                var delete = _deletes[deleteIndex];

                if (_deletersCache.TryGetValue(delete, out _))
                {
                    _database.Deletes.RemoveAt(deleteIndex);

                    continue;
                }

                MethodLocation methodLocation;

                if (delete.Command.OperationType.HasFlag(OperationType.Single))
                {
                    methodLocation = BuildSingleDelete(delete);
                }
                else if (delete.Command.OperationType.HasFlag(OperationType.Many))
                {
                    methodLocation = BuildManyDelete(delete);
                }
                else
                {
                    throw new InvalidOperationException();
                }

                delete.Command.Location = methodLocation;

                _deletersCache.Add(delete, methodLocation);
            }

            return File("Reflow.Deletes")
                .WithMembers(
                    Class(_className, CSharpModifiers.Internal | CSharpModifiers.Static)
                        .WithMembers(_deleters)
                )
                .GetText();
        }

        private MethodLocation BuildSingleDelete(Delete delete)
        {
            var entity = delete.Entity;

            var methodDefinition = new MethodLocation(
                "Reflow.Deletes." + _className,
                "Deletes_" + _deleteIndex++
            );

            var primaryColumn = entity.Columns[0];
            var primaryArgument = "@p" + primaryColumn.ColumnName;

            var commandText =
                $"DELETE FROM \"{entity.TableName}\" WHERE \"{primaryColumn.ColumnName}\" = {primaryArgument}";

            _deleters.Add(
                Method(
                        methodDefinition.MethodName,
                        Void(),
                        CSharpModifiers.Internal | CSharpModifiers.Static
                    )
                    .WithParameters(
                        Parameter("command", Type<DbCommand>()),
                        Parameter("entity", Type(delete.Command.Entity))
                    )
                    .WithStatements(
                        AssignMember(
                            Variable("command"),
                            nameof(DbCommand.CommandText),
                            Constant(commandText)
                        ),
                        Statement(
                            Invoke(
                                AccessMember(Variable("command"), nameof(DbCommand.Parameters)),
                                nameof(DbParameterCollection.Add),
                                Instance(Type("Npgsql.NpgsqlParameter"))
                                    .WithArguments(
                                        Constant(primaryArgument),
                                        AccessMember(Variable("entity"), primaryColumn.PropertyName)
                                    )
                            )
                        )
                    )
            );

            return methodDefinition;
        }

        private MethodLocation BuildManyDelete(Delete delete)
        {
            var entity = delete.Entity;

            var methodDefinition = new MethodLocation(
                "Reflow.Deletes." + _className,
                "Deletes_" + _deleteIndex++
            );

            var primaryColumn = entity.Columns[0];

            var partialCommandText = new StringBuilder("DELETE FROM \"")
                .Append(entity.TableName)
                .Append("\" WHERE \"")
                .Append(primaryColumn.ColumnName)
                .Append("\" IN (");

            _deleters.Add(
                Method(
                        methodDefinition.MethodName,
                        Void(),
                        CSharpModifiers.Internal | CSharpModifiers.Static
                    )
                    .WithParameters(
                        Parameter("command", Type<DbCommand>()),
                        Parameter(
                            "entities",
                            GenericType(typeof(IList<>), Type(delete.Command.Entity))
                        )
                    )
                    .WithStatements(
                        Local("commandText", Var())
                            .WithInitializer(Instance(Type<StringBuilder>())),
                        Statement(
                            Invoke(
                                Variable("commandText"),
                                nameof(StringBuilder.Append),
                                Constant(partialCommandText.ToString())
                            )
                        ),
                        Local("parameters", Var())
                            .WithInitializer(
                                AccessMember(Variable("command"), nameof(DbCommand.Parameters))
                            ),
                        Local("totalEntites", Var())
                            .WithInitializer(
                                AccessMember(Variable("entities"), nameof(IList.Count))
                            ),
                        For(
                            Local("entityIndex", Var()).WithInitializer(Constant(0)),
                            LessThen(Variable("entityIndex"), Variable("totalEntites")),
                            Increment(Variable("entityIndex")),
                            Local("name", Var())
                                .WithInitializer(
                                    Add(
                                        Constant("@p" + primaryColumn.ColumnName),
                                        Variable("entityIndex")
                                    )
                                ),
                            Statement(
                                Invoke(
                                    Variable("parameters"),
                                    nameof(DbParameterCollection.Add),
                                    Instance(
                                            GenericType(
                                                "Npgsql.NpgsqlParameter",
                                                Type(primaryColumn.Type)
                                            )
                                        )
                                        .WithArguments(
                                            Variable("name"),
                                            AccessMember(
                                                AccessElement(
                                                    Variable("entities"),
                                                    Variable("entityIndex")
                                                ),
                                                primaryColumn.PropertyName
                                            )
                                        )
                                )
                            ),
                            Statement(
                                Invoke(
                                    Invoke(
                                        Variable("commandText"),
                                        nameof(StringBuilder.Append),
                                        Variable("name")
                                    ),
                                    nameof(StringBuilder.Append),
                                    Constant(", ")
                                )
                            )
                        ),
                        AssignMember(
                            Variable("commandText"),
                            nameof(StringBuilder.Length),
                            Substract(
                                AccessMember(Variable("commandText"), nameof(StringBuilder.Length)),
                                Constant(2)
                            )
                        ),
                        Statement(
                            Invoke(
                                Variable("commandText"),
                                nameof(StringBuilder.Append),
                                Constant(')')
                            )
                        ),
                        AssignMember(
                            Variable("command"),
                            nameof(DbCommand.CommandText),
                            Invoke(Variable("commandText"), nameof(StringBuilder.ToString))
                        )
                    )
            );

            return methodDefinition;
        }

        internal static SourceText Emit(Database database)
        {
            return new DeleteEmitter(database, database.Deletes).Build();
        }

        private class DeleteEqualityComparer : IEqualityComparer<Delete>
        {
            internal static DeleteEqualityComparer Default = new();

            private DeleteEqualityComparer() { }

            bool IEqualityComparer<Delete>.Equals(Delete x, Delete y)
            {
                return x.Command.OperationType == y.Command.OperationType
                    && x.Command.Entity.Equals(y.Command.Entity, SymbolEqualityComparer.Default);
            }

            int IEqualityComparer<Delete>.GetHashCode(Delete obj)
            {
                var hashCode = new HashCode();

                hashCode.Add(obj.Command.Entity);
                hashCode.Add(obj.Command.OperationType);

                return hashCode.ToHashCode();
            }
        }
    }
}
