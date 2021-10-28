using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Database.Emitters;

namespace Reflow.Analyzer.Database
{
    internal class DatabaseConfigurationGenerator
        : IGroupableSourceGenerator<DatabaseGeneratorGroup.Data>
    {
        public ISyntaxContextReceiver Initialize() => new SyntaxContextReceiver();

        public void Execute(
            GeneratorExecutionContext context,
            ISyntaxContextReceiver syntaxReceiver,
            DatabaseGeneratorGroup.Data data
        )
        {
            context.Compilation.EnsureReference("Reflow", AssemblyInfo.PublicKey);

            var candidates = (syntaxReceiver as SyntaxContextReceiver)!.Candidates;

            var genericTableSymbol = context.Compilation.GetTypeByMetadataName("Reflow.Table`1");

            for (var candidateIndex = 0; candidateIndex < candidates.Count; candidateIndex++)
            {
                var candidate = candidates[candidateIndex];

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

                    data.Configurations.Add(configuration);
                }
            }

            context.AddNamedSource(
                "DatabaseInstantiater",
                DatabaseConfigurationEmitter.Emit(data.Configurations)
            );
        }

        private class SyntaxContextReceiver : ISyntaxContextReceiver
        {
            internal List<INamedTypeSymbol> Candidates { get; }

            internal SyntaxContextReceiver()
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
