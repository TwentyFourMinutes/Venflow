using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.SyntaxGenerator;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Reflow.Analyzer.SyntaxGenerator.CSharpSyntaxGenerator;

namespace Reflow.Analyzer.Database.Emitters
{
    internal static class DatabaseConfigurationEmitter
    {
        internal static SourceText Emit(IList<DatabaseConfiguration> configurations)
        {
            var configurationEntries = new (ExpressionSyntax Key, ExpressionSyntax Value)[
                configurations.Count
            ];

            for (
                var configurationIndex = 0;
                configurationIndex < configurations.Count;
                configurationIndex++
            )
            {
                var configuration = configurations[configurationIndex];

                const string databaseParameter = "baseDatabase";
                const string databaseLocal = "database";

                var statements = new StatementSyntax[configuration.Properties.Count + 1];

                statements[0] = (StatementSyntax)Local(
                    Variable(
                        databaseLocal,
                        Type("var"),
                        CastExpression(
                            Type(configuration.FullDatabaseName),
                            IdentifierName(databaseParameter)
                        )
                    )
                );

                for (
                    var propertyIndex = 0;
                    propertyIndex < configuration.Properties.Count;
                    propertyIndex++
                )
                {
                    var property = configuration.Properties[propertyIndex];

                    statements[propertyIndex + 1] = AssignMember(
                        databaseLocal,
                        property.Name,
                        Instance(GenericType("Table", Type(property.FullTypeName)))
                            .WithArguments(IdentifierName(databaseLocal))
                    );
                }

                configurationEntries[configurationIndex] = (
                    Key: TypeOfExpression(Type(configuration.FullDatabaseName)),
                    Value: Instance(Type(nameof(DatabaseConfiguration)))
                        .WithArguments(Lambda(databaseParameter).WithStatements(statements))
                );
            }

            return File(
                usings: new[] { "System", "System.Collections.Generic" },
                namespaceName: "Reflow",
                members: Class(
                        name: "DatabaseConfigurations",
                        SyntaxKind.PublicKeyword,
                        SyntaxKind.StaticKeyword
                    )
                    .WithMembers(
                        Field(
                            variable: Variable(
                                name: "Configurations",
                                type: GenericType(
                                    "Dictionary",
                                    Type("Type"),
                                    Type(nameof(DatabaseConfiguration))
                                ),
                                expressionSyntax: Instance(
                                        GenericType(
                                            "Dictionary",
                                            Type("Type"),
                                            Type(nameof(DatabaseConfiguration))
                                        )
                                    )
                                    .WithArguments(Constant(configurations.Count))
                                    .WithInitializer(DictionaryInitializer(configurationEntries))
                            ),
                            SyntaxKind.PublicKeyword,
                            SyntaxKind.StaticKeyword
                        )
                    )
            );
        }
    }
}
