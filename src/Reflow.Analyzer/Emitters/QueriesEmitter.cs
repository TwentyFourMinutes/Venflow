using System.Data.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.CodeGenerator;
using Reflow.Analyzer.Models;
using Reflow.Analyzer.Sections.LambdaSorter;
using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

namespace Reflow.Analyzer.Emitters
{
    internal static class QueriesEmitter
    {
        internal static SourceText Emit(Database database, List<Query> queries)
        {
            var className = $"__{database.Symbol.GetFullName().Replace('.', '_')}";
            var members = new List<MethodDeclarationSyntax>(queries.Count);
            var existingParsers = new Dictionary<Query, MethodDefinitionSyntax>();

            var parserIndex = 0;

            var cases = new List<SwitchSectionSyntax>();
            var caseStatements = new List<StatementSyntax>();

            for (var queryIndex = 0; queryIndex < queries.Count; queryIndex++)
            {
                var query = queries[queryIndex];

                if (query.Type.HasFlag(QueryType.Single))
                {
                    if (query.Type.HasFlag(QueryType.WithRelations)) { }
                    else
                    {
                        if (existingParsers.TryGetValue(query, out var methodDefinition))
                        {
                            throw new NotImplementedException();
                        }
                        else
                        {
                            var entity = database.Entities[query.Entity];

                            for (
                                var columnIndex = 0;
                                columnIndex < entity.Columns.Count;
                                columnIndex++
                            )
                            {
                                var column = entity.Columns[columnIndex];

                                if (columnIndex == 0)
                                {
                                    caseStatements.Add(
                                        AssignLocal(
                                            Variable("entity"),
                                            Instance(Type(entity.EntitySymbol))
                                        )
                                    );
                                }

                                caseStatements.Add(
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

                                caseStatements.Add(Break());

                                cases.Add(Case(Constant(columnIndex), caseStatements));
                                caseStatements.Clear();
                            }

                            methodDefinition = new MethodDefinitionSyntax(
                                className,
                                "Parser_" + parserIndex++
                            );

                            members.Add(
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
                                        Local("columnCount ", Var())
                                            .WithInitializer(
                                                AccessMember(Variable("columns"), "Length")
                                            ),
                                        For(
                                            Local("columnIndex", Type(typeof(int)))
                                                .WithInitializer(Constant(0)),
                                            LessThen(
                                                Variable("columnIndex"),
                                                Variable("columnCount")
                                            ),
                                            Increment(Variable("columnIndex")),
                                            Switch(
                                                AccessElement(
                                                    Variable("columns"),
                                                    Variable("columnIndex")
                                                ),
                                                cases
                                            )
                                        ),
                                        Return(Variable("entity"))
                                    )
                            );
                        }
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

                cases.Clear();
            }

            var test = File("Reflow.Queries")
                .WithMembers(
                    Class(className, CSharpModifiers.Public | CSharpModifiers.Static)
                        .WithMembers(members)
                )
                .GetText()
                .ToString();

            return File("Reflow.Queries")
                .WithMembers(
                    Class(className, CSharpModifiers.Public | CSharpModifiers.Static)
                        .WithMembers(members)
                )
                .GetText();
        }
    }
}
