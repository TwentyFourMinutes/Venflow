using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Database.Emitters;

namespace Reflow.Analyzer.Database
{
    internal class EntityConfigurationGenerator
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

            var entityProxies = new Dictionary<ITypeSymbol, List<IPropertySymbol>>(
                SymbolEqualityComparer.Default
            );

            for (
                var configurationIndex = 0;
                configurationIndex < data.Configurations.Count;
                configurationIndex++
            )
            {
                var configuration = data.Configurations[configurationIndex];

                for (var tableIndex = 0; tableIndex < configuration.Tables.Count; tableIndex++)
                {
                    var table = configuration.Tables[tableIndex];

                    //if (!candidates.TryGetValue(table.EntityType, out var tableConfiguration))
                    //    continue;

                    var updatableProperties = new List<IPropertySymbol>();

                    for (var columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
                    {
                        var column = table.Columns[columnIndex];

                        if (column.IsVirtual)
                        {
                            updatableProperties.Add(column);
                        }
                    }

                    entityProxies.Add(table.EntityType, updatableProperties);
                }
            }

            context.AddNamedSource("EntityProxies", EntityProxyEmitter.Emit(entityProxies));
            context.AddNamedSource(
                "EntityConfigurations",
                EntityConfigurationEmitter.Emit(
                    data.Configurations.SelectMany(x => x.Tables).ToList()
                )
            );
        }

        private class SyntaxContextReceiver : ISyntaxContextReceiver
        {
            internal Dictionary<ITypeSymbol, INamedTypeSymbol> Candidates { get; }

            internal SyntaxContextReceiver()
            {
                Candidates = new(SymbolEqualityComparer.Default);
            }

            void ISyntaxContextReceiver.OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is not ClassDeclarationSyntax)
                    return;

                var configurationSymbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(
                    context.Node
                )!;

                for (
                    var interfaceIndex = 0;
                    interfaceIndex < configurationSymbol.Interfaces.Length;
                    interfaceIndex++
                )
                {
                    var interfaceSymbol = configurationSymbol.Interfaces[interfaceIndex];

                    if (
                        interfaceSymbol.GetFullName()
                            is not "Reflow.Modeling.IEntityConfiguration`1"
                        || !interfaceSymbol.IsReflowSymbol()
                    )
                        continue;

                    Candidates.Add(interfaceSymbol.TypeArguments[0], configurationSymbol);
                    break;
                }
            }
        }
    }
}
