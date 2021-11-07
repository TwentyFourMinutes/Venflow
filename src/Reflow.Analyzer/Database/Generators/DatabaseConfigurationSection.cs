using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Database.Emitters;
using Reflow.Analyzer.LambdaLinker;

namespace Reflow.Analyzer.Database
{
    internal class DatabaseConfigurationSection
        : GeneratorSection<
              LambdaLinkSection,
              DatabaseConfigurationSection.SyntaxReceiver,
              List<DatabaseConfiguration>
          >
    {
        protected override List<DatabaseConfiguration> Execute(
            GeneratorExecutionContext context,
            SyntaxReceiver syntaxReceiver,
            LambdaLinkSection previous
        )
        {
            context.Compilation.EnsureReference("Reflow", AssemblyInfo.PublicKey);

            var genericTableSymbol = context.Compilation.GetTypeByMetadataName("Reflow.Table`1");

            var configurations = new List<DatabaseConfiguration>();

            for (
                var candidateIndex = 0;
                candidateIndex < syntaxReceiver.Candidates.Count;
                candidateIndex++
            )
            {
                var candidate = syntaxReceiver.Candidates[candidateIndex];

                var members = candidate.GetMembers();

                var configuration = new DatabaseConfiguration(candidate);

                for (var memberIndex = 0; memberIndex < members.Length; memberIndex++)
                {
                    var member = members[memberIndex];

                    if (
                        member is not IPropertySymbol tableSymbol
                        || !tableSymbol.Type.OriginalDefinition.Equals(
                            genericTableSymbol,
                            SymbolEqualityComparer.Default
                        )
                    )
                        continue;

                    var table = new DatabaseTable(
                        tableSymbol,
                        ((INamedTypeSymbol)tableSymbol.Type).TypeArguments[0]
                    );

                    var entityType = ((INamedTypeSymbol)tableSymbol.Type).TypeArguments[0];

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

                        table.Columns.Add(entityPropertySymbol);
                    }

                    configuration.Tables.Add(table);

                    if (configuration.Tables.Count == 0)
                        continue;

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
