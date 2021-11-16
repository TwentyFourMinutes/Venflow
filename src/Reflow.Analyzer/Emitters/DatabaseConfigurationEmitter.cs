using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.CodeGenerator;
using Reflow.Analyzer.Models;
using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

namespace Reflow.Analyzer.Emitters
{
    internal static class DatabaseConfigurationEmitter
    {
        internal static SourceText Emit(IList<Database> databases)
        {
            var configurationEntries = new SyntaxList<InitializerExpressionSyntax>();

            for (var databaseIndex = 0; databaseIndex < databases.Count; databaseIndex++)
            {
                var database = databases[databaseIndex];

                const string databaseParameter = "baseDatabase";
                const string databaseLocal = "database";

                var statements = new SyntaxList<StatementSyntax>().Add(
                    Local(databaseLocal, Var())
                        .WithInitializer(Cast(Type(database.Symbol), Variable(databaseParameter)))
                );

                foreach (var entity in database.Entities.Values)
                {
                    statements = statements.Add(
                        AssignMember(
                            databaseLocal,
                            entity.PropertySymbol,
                            Instance(GenericType("Reflow.Table", Type(entity.EntitySymbol)))
                                .WithArguments(Variable(databaseLocal))
                        )
                    );
                }

                configurationEntries = configurationEntries.Add(
                    DictionaryEntry(
                        TypeOf(Type(database.Symbol)),
                        Instance(Type("Reflow.DatabaseConfiguration"))
                            .WithArguments(Lambda(databaseParameter).WithStatements(statements))
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
                    typeof(Dictionary<, >),
                    Type(typeof(Type)),
                    Type("Reflow.DatabaseConfiguration")
                );
        }
    }
}
