using System.Data.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.CodeGenerator;
using Reflow.Analyzer.Models;
using Reflow.Analyzer.Sections.LambdaSorter;
using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

namespace Reflow.Analyzer.Emitters
{
    internal class QueryParserEmitter
    {
        private ushort _parserIndex;

        private readonly string _className;
        private readonly Database _database;
        private readonly List<Query> _queries;
        private readonly List<MethodDeclarationSyntax> _parsers;
        private readonly Dictionary<Query, MethodLocation> _parserCache;
        private readonly List<SwitchSectionSyntax> _cases;
        private readonly List<StatementSyntax> _caseStatements;

        private QueryParserEmitter(Database database, List<Query> queries)
        {
            _database = database;
            _queries = queries;
            _className = $"__{database.Symbol.GetFullName().Replace('.', '_')}";
            _parsers = new(queries.Count);
            _parserCache = new(queries.Count, QueryEqualityComparer.Default);
            _cases = new();
            _caseStatements = new();
        }

        private SourceText Build()
        {
            for (var queryIndex = 0; queryIndex < _queries.Count; queryIndex++)
            {
                var query = _queries[queryIndex];

                if (_parserCache.TryGetValue(query, out var methodLocation))
                {
                    ((QueryLinkData)query.FluentCall.LambdaLink.Data!).Location = methodLocation;
                    continue;
                }

                if (query.Type.HasFlag(QueryType.Single))
                {
                    if (query.Type.HasFlag(QueryType.WithRelations)) { }
                    else
                    {
                        methodLocation = BuildSingleNoRelationParser(query);
                    }
                }
                else if (query.Type.HasFlag(QueryType.Many))
                {
                    if (query.Type.HasFlag(QueryType.WithRelations)) { }
                    else { }
                }
                else
                {
                    throw new InvalidOperationException();
                }

                ((QueryLinkData)query.FluentCall.LambdaLink.Data!).Location = methodLocation;

                _parserCache.Add(query, methodLocation);
                _cases.Clear();
            }

            return File("Reflow.QueryParsers")
                .WithMembers(
                    Class(_className, CSharpModifiers.Public | CSharpModifiers.Static)
                        .WithMembers(_parsers)
                )
                .GetText();
        }

        private MethodLocation BuildSingleNoRelationParser(Query query)
        {
            var entity = _database.Entities[query.Entity];

            for (var columnIndex = 0; columnIndex < entity.Columns.Count; columnIndex++)
            {
                var column = entity.Columns[columnIndex];

                if (columnIndex == 0)
                {
                    _caseStatements.Add(
                        AssignLocal(Variable("entity"), Instance(Type(entity.EntitySymbol)))
                    );
                }

                _caseStatements.Add(
                    AssignMember(
                        Variable("entity"),
                        column.Symbol,
                        Invoke(
                            Variable("reader"),
                            GenericName("GetFieldValue", Type(column.Symbol.Type)),
                            Variable("columnIndex")
                        )
                    )
                );
                _caseStatements.Add(Break());

                _cases.Add(Case(Constant(columnIndex), _caseStatements));
                _caseStatements.Clear();
            }

            var methodDefinition = new MethodLocation(
                "Reflow.QueryParsers." + _className,
                "Parser_" + _parserIndex++
            );

            _parsers.Add(
                Method(
                        methodDefinition.MethodName,
                        Type(query.Entity),
                        CSharpModifiers.Public | CSharpModifiers.Static
                    )
                    .WithParameters(
                        Parameter("reader", Type(typeof(DbDataReader))),
                        Parameter("columns", Array(Type(typeof(ushort))))
                    )
                    .WithStatements(
                        Local("entity", Type(query.Entity)).WithInitializer(Null()),
                        Local("columnCount", Var())
                            .WithInitializer(AccessMember(Variable("columns"), "Length")),
                        For(
                            Local("columnIndex", Type(typeof(int))).WithInitializer(Constant(0)),
                            LessThen(Variable("columnIndex"), Variable("columnCount")),
                            Increment(Variable("columnIndex")),
                            Switch(
                                AccessElement(Variable("columns"), Variable("columnIndex")),
                                _cases
                            )
                        ),
                        Return(Variable("entity"))
                    )
            );

            return methodDefinition;
        }
        internal static SourceText Emit(Database database, List<Query> queries)
        {
            return new QueryParserEmitter(database, queries).Build();
        }

        private class QueryEqualityComparer : IEqualityComparer<Query>
        {
            internal static QueryEqualityComparer Default = new();

            private QueryEqualityComparer() { }

            bool IEqualityComparer<Query>.Equals(Query x, Query y)
            {
                var xUsedEntities = ((QueryLinkData)x.FluentCall.LambdaLink.Data!).UsedEntities!;
                var yUsedEntities = ((QueryLinkData)y.FluentCall.LambdaLink.Data!).UsedEntities!;

                if (
                    x.TrackChanges != y.TrackChanges
                    || x.Type != y.Type
                    || xUsedEntities.Length != yUsedEntities.Length
                    || !x.Entity.Equals(y.Entity, SymbolEqualityComparer.Default)
                )
                {
                    return false;
                }

                for (var entityIndex = 0; entityIndex < xUsedEntities.Length; entityIndex++)
                {
                    if (xUsedEntities[entityIndex] != yUsedEntities[entityIndex])
                    {
                        return false;
                    }
                }

                return true;
            }

            int IEqualityComparer<Query>.GetHashCode(Query obj)
            {
                var usedEntities = ((QueryLinkData)obj.FluentCall.LambdaLink.Data!).UsedEntities!;
                var hashCode = new HashCode();

                hashCode.Add(obj.TrackChanges);
                hashCode.Add(obj.Entity);
                hashCode.Add(obj.Type);

                for (var entityIndex = 0; entityIndex < usedEntities.Length; entityIndex++)
                {
                    hashCode.Add(usedEntities[entityIndex]);
                }

                return hashCode.ToHashCode();
            }
        }
    }
}
