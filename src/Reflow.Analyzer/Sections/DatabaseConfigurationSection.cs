using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Emitters;
using Reflow.Analyzer.Models;

namespace Reflow.Analyzer.Sections
{
    internal class DatabaseConfigurationSection
        : GeneratorSection<
              SourceGenerator,
              DatabaseConfigurationSection.SyntaxReceiver,
              List<Database>
          >
    {
        protected override List<Database> Execute(
            GeneratorExecutionContext context,
            SyntaxReceiver syntaxReceiver,
            SourceGenerator previous
        )
        {
            var genericTableSymbol = context.Compilation.GetTypeByMetadataName("Reflow.Table`1");

            var configurations = new List<Database>();

            for (
                var candidateIndex = 0;
                candidateIndex < syntaxReceiver.Candidates.Count;
                candidateIndex++
            )
            {
                var candidate = syntaxReceiver.Candidates[candidateIndex];

                var members = candidate.GetMembers();

                var configuration = new Database(candidate);

                for (var memberIndex = 0; memberIndex < members.Length; memberIndex++)
                {
                    var member = members[memberIndex];

                    if (
                        member is not IPropertySymbol entitySymbol
                        || !entitySymbol.Type.OriginalDefinition.Equals(
                            genericTableSymbol,
                            SymbolEqualityComparer.Default
                        )
                    )
                        continue;

                    var entityType = (INamedTypeSymbol)(
                        (INamedTypeSymbol)entitySymbol.Type
                    ).TypeArguments[0];

                    configuration.EntitySymbols.Add((entitySymbol, entityType));
                }

                configurations.Add(configuration);
            }

            context.AddNamedSource(
                "DatabaseInstantiater",
                DatabaseConfigurationEmitter.Emit(configurations)
            );

            return configurations;
        }

        internal class SyntaxReceiver : ISyntaxContextReceiver
        {
            internal List<INamedTypeSymbol> Candidates { get; }

            internal SyntaxReceiver()
            {
                Candidates = new();
            }

            void ISyntaxContextReceiver.OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is not ClassDeclarationSyntax classDeclaration)
                    return;

                if (classDeclaration.BaseList is null)
                    return;

                var classSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(
                    classDeclaration
                )!;

                var potentialDatabaseType = classSymbol;

                while (true)
                {
                    potentialDatabaseType = potentialDatabaseType.BaseType;

                    if (
                        potentialDatabaseType is null
                        || potentialDatabaseType.GetFullName() is "System.Object"
                    )
                        break;

                    if (
                        potentialDatabaseType.GetFullName() is not "Reflow.Database"
                        || !potentialDatabaseType.IsReflowSymbol()
                    )
                        continue;

                    Candidates.Add(classSymbol);
                    break;
                }
            }
        }
    }
}
