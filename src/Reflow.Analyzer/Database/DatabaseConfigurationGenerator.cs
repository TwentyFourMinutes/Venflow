using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reflow.Analyzer.Database
{
    [Generator(LanguageNames.CSharp)]
    internal class DatabaseConfigurationGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var candidates = (context.SyntaxContextReceiver as SyntaxContextReceiver)!.Candidates;
            var databaseConfigurations = new List<DatabaseConfiguration>();

            foreach (var databaseSymbol in candidates)
            {
                var members = databaseSymbol.GetMembers();

                var configuration = new DatabaseConfiguration(databaseSymbol.GetFullName());

                for (var memberIndex = 0; memberIndex < members.Length; memberIndex++)
                {
                    var member = members[memberIndex];

                    if (member is not IPropertySymbol propertySymbol)
                        continue;

                    configuration.Properties.Add(
                        new DatabaseTable(
                            propertySymbol.Name,
                            ((INamedTypeSymbol)propertySymbol.Type).TypeArguments[0].GetFullName()
                        )
                    );
                }

                if (configuration.Properties.Count == 0)
                    continue;

                databaseConfigurations.Add(configuration);
            }

            context.AddTemplatedSource(
                "Database/Resources/DatabaseConfigurations.sbncs",
                new { Configurations = databaseConfigurations }
            );
        }

        private class SyntaxContextReceiver : ISyntaxContextReceiver
        {
            internal HashSet<INamedTypeSymbol> Candidates { get; }

            internal SyntaxContextReceiver()
            {
#pragma warning disable RS1024 // Compare symbols correctly
                Candidates = new(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly
            }

            void ISyntaxContextReceiver.OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is not ClassDeclarationSyntax)
                    return;

                var classSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(
                    context.Node
                )!;

                var potentialDatabaseType = classSymbol;

                while (true)
                {
                    potentialDatabaseType = potentialDatabaseType.BaseType;

                    if (potentialDatabaseType is null)
                        return;

                    if (
                        potentialDatabaseType.Name is not "Database"
                        || potentialDatabaseType.ContainingNamespace.Name is not "Reflow"
                    )
                        continue;

                    var assemblyIdentity = potentialDatabaseType.ContainingAssembly.Identity;

                    if (
                        assemblyIdentity.Name is not "Reflow"
                        || !assemblyIdentity.PublicKey.SequenceEqual(AssemblyInfo.PublicKey)
                    )
                        continue;
                    break;
                }

                Candidates.Add(classSymbol);
            }
        }
    }
}
