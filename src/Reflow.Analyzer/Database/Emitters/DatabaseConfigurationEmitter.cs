using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Internal;
using static Reflow.Internal.CSharpCodeGenerator;

namespace Reflow.Analyzer.Database.Emitters
{
    internal static class DatabaseConfigurationEmitter
    {
        internal static SourceText Emit(IList<DatabaseConfiguration> configurations)
        {
            var configurationEntries = new SyntaxList<InitializerExpressionSyntax>();

            for (
                var configurationIndex = 0;
                configurationIndex < configurations.Count;
                configurationIndex++
            )
            {
                var configuration = configurations[configurationIndex];

                const string databaseParameter = "baseDatabase";
                const string databaseLocal = "database";

                var statements = new SyntaxList<StatementSyntax>().Add(
                    Local(databaseLocal, Var())
                        .WithInitializer(
                            Cast(Type(configuration.Type), Variable(databaseParameter))
                        )
                );

                for (
                    var propertyIndex = 0;
                    propertyIndex < configuration.Tables.Count;
                    propertyIndex++
                )
                {
                    var property = configuration.Tables[propertyIndex];

                    statements = statements.Add(
                        AssignMember(
                            databaseLocal,
                            property.Type,
                            Instance(GenericType("Reflow.Table", Type(property.EntityType)))
                                .WithArguments(IdentifierName(databaseLocal))
                        )
                    );
                }

                configurationEntries = configurationEntries.Add(
                    DictionaryEntry(
                        TypeOf(Type(configuration.Type)),
                        Instance(Type("Reflow.DatabaseConfiguration"))
                            .WithArguments(Lambda(databaseParameter).WithStatements(statements))
                    )
                );
            }

            var dictionaryType = GenericType(
                typeof(Dictionary<, >),
                Type(typeof(Type)),
                Type("Reflow.DatabaseConfiguration")
            );

            return File("Reflow")
                .WithMembers(
                    Class("DatabaseConfigurations", CSharpModifiers.Public | CSharpModifiers.Static)
                        .WithMembers(
                            Field(
                                    "Configurations",
                                    dictionaryType,
                                    CSharpModifiers.Public | CSharpModifiers.Static
                                )
                                .WithInitializer(
                                    Instance(dictionaryType)
                                        .WithArguments(Constant(configurations.Count))
                                        .WithInitializer(
                                            DictionaryInitializer(configurationEntries)
                                        )
                                )
                        )
                )
                .GetText();
        }
    }
}
