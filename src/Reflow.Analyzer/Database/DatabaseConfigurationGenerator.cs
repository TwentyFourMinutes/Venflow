using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Database.Emitters;
using Reflow.Internal;

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
            context.Compilation.EnsureReference("Reflow", AssemblyInfo.PublicKey);

            var candidates =
                (context.SyntaxContextReceiver as SyntaxContextReceiver)!.DatabaseCandidates;
            var databaseConfigurations = new List<DatabaseConfiguration>();

            var updatableEntites = new Dictionary<string, List<IPropertySymbol>>();

            var genericTableSymbol = context.Compilation.GetTypeByMetadataName("Reflow.Table`1");

            foreach (var databaseSymbol in candidates)
            {
                var members = databaseSymbol.GetMembers();

                var configuration = new DatabaseConfiguration(databaseSymbol.GetFullName());

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

                    var updatableProperties = new List<IPropertySymbol>();

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
                            || !entityPropertySymbol.IsVirtual
                            || entityPropertySymbol.GetMethod is null
                            || entityPropertySymbol.GetMethod.DeclaredAccessibility
                                != Accessibility.Public
                        )
                            continue;

                        updatableProperties.Add(entityPropertySymbol);
                    }

                    if (updatableProperties.Count > 0)
                        updatableEntites.Add(entityType.GetFullName(), updatableProperties);

                    configuration.Properties.Add(
                        new DatabaseTable(
                            tableSymbol.Name,
                            ((INamedTypeSymbol)tableSymbol.Type).TypeArguments[0].GetFullName()
                        )
                    );
                }

                if (configuration.Properties.Count == 0)
                    continue;

                databaseConfigurations.Add(configuration);
            }

            context.AddNamedSource("Proxies", EntityProxyEmitter.Emit(updatableEntites));

            context.AddNamedSource(
                "DatabaseInstantiater",
                DatabaseConfigurationEmitter.Emit(databaseConfigurations)
            );
        }

        private class SyntaxContextReceiver : ISyntaxContextReceiver
        {
            internal HashSet<INamedTypeSymbol> DatabaseCandidates { get; }
            internal Dictionary<
                ITypeSymbol,
                INamedTypeSymbol
            > EntityConfigurationCandidates { get; }

            internal SyntaxContextReceiver()
            {
#pragma warning disable RS1024 // Compare symbols correctly
                DatabaseCandidates = new(SymbolEqualityComparer.Default);
                EntityConfigurationCandidates = new(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly
            }

            void ISyntaxContextReceiver.OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is not ClassDeclarationSyntax)
                    return;

                var classSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(
                    context.Node
                )!;

                _ = TryGetDatbase(classSymbol) || TryGetEntityConfiguration(classSymbol);
            }

            private bool TryGetDatbase(INamedTypeSymbol classSymbol)
            {
                var potentialDatabaseType = classSymbol;

                while (true)
                {
                    potentialDatabaseType = potentialDatabaseType.BaseType;

                    if (potentialDatabaseType is null)
                        return false;

                    if (
                        potentialDatabaseType.GetFullName() is not "Reflow.Database"
                        || !potentialDatabaseType.IsReflowSymbol()
                    )
                        continue;
                    break;
                }

                DatabaseCandidates.Add(classSymbol);

                return true;
            }

            private bool TryGetEntityConfiguration(INamedTypeSymbol classSymbol)
            {
                var baseType = classSymbol.BaseType;

                if (baseType is null)
                    return false;

                if (
                    baseType.GetFullName() is not "Reflow.IEntityConfiguration"
                    || !baseType.IsReflowSymbol()
                )
                    return false;

                EntityConfigurationCandidates.Add(baseType.TypeArguments[0], classSymbol);

                return true;
            }
        }
    }
}
