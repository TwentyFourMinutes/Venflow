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

                    var entity = new Entity(
                        entitySymbol,
                        ((INamedTypeSymbol)entitySymbol.Type).TypeArguments[0]
                    );

                    var entityType = ((INamedTypeSymbol)entitySymbol.Type).TypeArguments[0];

                    var entityMembers = entityType.GetMembers();

                    for (
                        var entityMemberIndex = 0;
                        entityMemberIndex < entityMembers.Length;
                        entityMemberIndex++
                    )
                    {
                        var entityMember = entityMembers[entityMemberIndex];

                        if (
                            entityMember is not IPropertySymbol entityPropertySymbol
                            || entityPropertySymbol.DeclaredAccessibility != Accessibility.Public
                            || entityPropertySymbol.GetMethod is null
                            || entityPropertySymbol.GetMethod.DeclaredAccessibility
                                != Accessibility.Public
                        )
                            continue;

                        entity.Columns.Add(new Column(entityPropertySymbol));
                    }

                    configuration.Entities.Add(entity.EntitySymbol, entity);

                    configurations.Add(configuration);
                }
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
                        break;

                    if (
                        potentialDatabaseType.GetFullName() is not "Reflow.Database"
                        || !potentialDatabaseType.IsReflowSymbol()
                    )
                        continue;
                    break;
                }

                Candidates.Add(classSymbol);
            }
        }
    }
}
