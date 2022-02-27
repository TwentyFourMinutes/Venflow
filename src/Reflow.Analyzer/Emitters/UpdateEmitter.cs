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
    internal class UpdateEmitter
    {
        private ushort _updateIndex;

        private readonly string _className;
        private readonly Database _database;
        private readonly List<Update> _updates;
        private readonly List<MethodDeclarationSyntax> _updaters;

        private readonly Dictionary<Update, MethodLocation> _updatesCache;
        private readonly Dictionary<INamedTypeSymbol, MethodLocation> _baseUpdatesCache;

        private UpdateEmitter(Database database, List<Update> updates)
        {
            _database = database;
            _updates = updates;
            _className = $"__{database.Symbol.GetFullName().Replace('.', '_')}";
            _updaters = new();
            _updatesCache = new(UpdateEqualityComparer.Default);
            _baseUpdatesCache = new(SymbolEqualityComparer.Default);
        }

        private SourceText Build()
        {
            for (var updateIndex = 0; updateIndex < _updates.Count; updateIndex++)
            {
                var update = _updates[updateIndex];

                if (_updatesCache.TryGetValue(update, out _))
                {
                    _database.Updates.RemoveAt(updateIndex);

                    continue;
                }

                if (
                    !_baseUpdatesCache.TryGetValue(update.Entity.Symbol, out var baseMethodLocation)
                )
                {
                    baseMethodLocation = BuildBaseUpdate(update);

                    _baseUpdatesCache.Add(update.Entity.Symbol, baseMethodLocation);
                }

                MethodLocation methodLocation;

                if (update.Command.OperationType.HasFlag(OperationType.Single))
                {
                    methodLocation = BuildSingleUpdate(update, baseMethodLocation);
                }
                else if (update.Command.OperationType.HasFlag(OperationType.Many))
                {
                    methodLocation = BuildManyUpdate(update, baseMethodLocation);
                }
                else
                {
                    throw new InvalidOperationException();
                }

                update.Command.Location = methodLocation;

                _updatesCache.Add(update, methodLocation);
            }

            return File("Reflow.Updates")
                .WithMembers(
                    Class(_className, CSharpModifiers.Internal | CSharpModifiers.Static)
                        .WithMembers(_updaters)
                )
                .GetText();
        }

        private MethodLocation BuildSingleUpdate(Update update, MethodLocation baseMethodDefinition)
        {
            var entity = update.Entity;

            if (!entity.HasProxy)
                throw new InvalidOperationException();

            var methodDefinition = new MethodLocation(
                "Reflow.Updates." + _className,
                "Updates_" + _updateIndex++
            );

            var primaryColumn = entity.Columns[0];

            _updaters.Add(
                Method(
                        methodDefinition.MethodName,
                        Void(),
                        CSharpModifiers.Internal | CSharpModifiers.Static
                    )
                    .WithParameters(
                        Parameter("command", Type<DbCommand>()),
                        Parameter("entity", Type(update.Command.Entity))
                    )
                    .WithStatements(
                        Local("commandText", Var())
                            .WithInitializer(Instance(Type<StringBuilder>()).WithArguments()),
                        Statement(
                            Invoke(
                                Variable("commandText"),
                                nameof(StringBuilder.Append),
                                Constant($"UPDATE \"{entity.TableName}\" SET ")
                            )
                        ),
                        Local("parameters", Type<DbParameterCollection>())
                            .WithInitializer(
                                AccessMember(Variable("command"), nameof(DbCommand.Parameters))
                            ),
                        Statement(
                            Invoke(
                                Type(methodDefinition.FullTypeName),
                                baseMethodDefinition.MethodName,
                                Variable("entity"),
                                Variable("commandText"),
                                Variable("parameters"),
                                Constant(0)
                            )
                        ),
                        If(
                            Equal(
                                AccessMember(Variable("parameters"), nameof(IList.Count)),
                                Constant(0)
                            ),
                            Return()
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
                                Constant(
                                    $" WHERE \"{primaryColumn.ColumnName}\" = @p{primaryColumn.ColumnName};"
                                )
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
                                        Constant("@p" + primaryColumn.ColumnName),
                                        AccessMember(Variable("entity"), primaryColumn.PropertyName)
                                    )
                            )
                        ),
                        AssignMember(
                            "command",
                            nameof(DbCommand.CommandText),
                            Invoke(Variable("commandText"), nameof(StringBuilder.ToString))
                        )
                    )
            );

            return methodDefinition;
        }

        private MethodLocation BuildManyUpdate(Update update, MethodLocation baseMethodDefinition)
        {
            var entity = update.Entity;

            if (!entity.HasProxy)
                throw new InvalidOperationException();

            var methodDefinition = new MethodLocation(
                "Reflow.Updates." + _className,
                "Updates_" + _updateIndex++
            );

            var primaryColumn = entity.Columns[0];

            _updaters.Add(
                Method(
                        methodDefinition.MethodName,
                        Void(),
                        CSharpModifiers.Internal | CSharpModifiers.Static
                    )
                    .WithParameters(
                        Parameter("command", Type<DbCommand>()),
                        Parameter(
                            "entities",
                            GenericType(typeof(IList<>), Type(update.Command.Entity))
                        )
                    )
                    .WithStatements(
                        Local("commandText", Var())
                            .WithInitializer(Instance(Type<StringBuilder>())),
                        Local("absoluteIndex", Type<ushort>()).WithInitializer(Constant(0)),
                        For(
                            Local("entityIndex", Var()).WithInitializer(Constant(0)),
                            LessThen(
                                Variable("entityIndex"),
                                AccessMember(Variable("entities"), nameof(IList.Count))
                            ),
                            Increment(Variable("entityIndex")),
                            Local("entity", Var())
                                .WithInitializer(
                                    AccessElement(Variable("entities"), Variable("entityIndex"))
                                ),
                            Statement(
                                Invoke(
                                    Variable("commandText"),
                                    nameof(StringBuilder.Append),
                                    Constant($"UPDATE \"{entity.TableName}\" SET ")
                                )
                            ),
                            Local("parameters", Type<DbParameterCollection>())
                                .WithInitializer(
                                    AccessMember(Variable("command"), nameof(DbCommand.Parameters))
                                ),
                            Local("newAbsoluteIndex", Type<ushort>())
                                .WithInitializer(
                                    Invoke(
                                        Type(methodDefinition.FullTypeName),
                                        baseMethodDefinition.MethodName,
                                        Variable("entity"),
                                        Variable("commandText"),
                                        Variable("parameters"),
                                        Variable("absoluteIndex")
                                    )
                                ),
                            If(
                                Equal(Variable("absoluteIndex"), Variable("newAbsoluteIndex")),
                                Continue()
                            ),
                            Statement(
                                AssignLocal(Variable("absoluteIndex"), Variable("newAbsoluteIndex"))
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
                            Local("name", Var())
                                .WithInitializer(
                                    Add(
                                        Constant("@p" + primaryColumn.ColumnName),
                                        Variable("absoluteIndex")
                                    )
                                ),
                            Statement(
                                Invoke(
                                    Variable("commandText"),
                                    nameof(StringBuilder.Append),
                                    Constant($" WHERE \"{primaryColumn.ColumnName}\" = ")
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
                                            Constant("@p" + primaryColumn.ColumnName),
                                            AccessMember(
                                                Variable("entity"),
                                                primaryColumn.PropertyName
                                            )
                                        )
                                )
                            )
                        ),
                        AssignMember(
                            "command",
                            nameof(DbCommand.CommandText),
                            Invoke(Variable("commandText"), nameof(StringBuilder.ToString))
                        )
                    )
            );

            return methodDefinition;
        }

        private MethodLocation BuildBaseUpdate(Update update)
        {
            var entity = update.Entity;

            if (!entity.HasProxy)
                throw new InvalidOperationException();

            var methodDefinition = new MethodLocation(
                "Reflow.Updates." + _className,
                "BaseUpdates_" + _updateIndex++
            );

            var caseStaments = new List<SwitchSectionSyntax>();

            for (var columnIndex = 0; columnIndex < entity.Columns.Count; columnIndex++)
            {
                var column = entity.Columns[columnIndex];

                if (!column.IsUpdatable)
                    continue;

                caseStaments.Add(
                    Case(
                        ShiftLeft(Constant(1), Constant(0)),
                        Statement(
                            AssignLocal(
                                Variable("name"),
                                Add(
                                    Constant("@p" + column.ColumnName),
                                    Increment(Variable("absoluteIndex"))
                                )
                            )
                        ),
                        Statement(
                            Invoke(
                                Variable("commandText"),
                                nameof(StringBuilder.Append),
                                Constant($"\"{column.ColumnName}\" = ")
                            )
                        ),
                        Statement(
                            Invoke(
                                Variable("parameters"),
                                nameof(DbParameterCollection.Add),
                                Instance(GenericType("Npgsql.NpgsqlParameter", Type(column.Type)))
                                    .WithArguments(
                                        Variable("name"),
                                        AccessMember(Variable("proxy"), column.PropertyName)
                                    )
                            )
                        ),
                        Break()
                    )
                );
            }

            caseStaments.Add(DefaultCase(Throw(Instance(Type<InvalidOperationException>()))));

            _updaters.Add(
                Method(
                        methodDefinition.MethodName,
                        Type<ushort>(),
                        CSharpModifiers.Internal | CSharpModifiers.Static
                    )
                    .WithParameters(
                        Parameter("entity", Type(update.Command.Entity)),
                        Parameter("commandText", Type<StringBuilder>()),
                        Parameter("parameters", Type<DbParameterCollection>()),
                        Parameter("absoluteIndex", Type<ushort>())
                    )
                    .WithStatements(
                        If(
                            IsNot(Variable("entity"), Local("proxy", Type(entity.ProxyName!))),
                            Return(Variable("absoluteIndex"))
                        ),
                        Local("section", Var())
                            .WithInitializer(
                                Invoke(Variable("proxy"), "GetSectionChanges", Constant(0))
                            ),
                        While(
                            NotEqual(Variable("section"), Constant(0)),
                            Local("name", Type<string>()),
                            Switch(
                                Cast(
                                    Type<byte>(),
                                    Parenthesis(
                                        BitwiseAnd(
                                            Variable("section"),
                                            Cast(
                                                Type<byte>(),
                                                Parenthesis(
                                                    Add(
                                                        BitwiseNot(Variable("section")),
                                                        Constant(1)
                                                    )
                                                )
                                            )
                                        )
                                    )
                                ),
                                caseStaments
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
                            ),
                            Statement(
                                AssignLocal(
                                    Variable("section"),
                                    Cast(
                                        Type<byte>(),
                                        Parenthesis(
                                            BitwiseAnd(
                                                Variable("section"),
                                                Cast(
                                                    Type<byte>(),
                                                    Parenthesis(
                                                        Substract(Variable("section"), Constant(1))
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Return(Variable("absoluteIndex"))
                    )
            );

            return methodDefinition;
        }

        internal static SourceText Emit(Database database)
        {
            return new UpdateEmitter(database, database.Updates).Build();
        }

        private class UpdateEqualityComparer : IEqualityComparer<Update>
        {
            internal static UpdateEqualityComparer Default = new();

            private UpdateEqualityComparer() { }

            bool IEqualityComparer<Update>.Equals(Update x, Update y)
            {
                return x.Command.OperationType == y.Command.OperationType
                    && x.Command.Entity.Equals(y.Command.Entity, SymbolEqualityComparer.Default);
            }

            int IEqualityComparer<Update>.GetHashCode(Update obj)
            {
                var hashCode = new HashCode();

                hashCode.Add(obj.Command.Entity);
                hashCode.Add(obj.Command.OperationType);

                return hashCode.ToHashCode();
            }
        }
    }
}
