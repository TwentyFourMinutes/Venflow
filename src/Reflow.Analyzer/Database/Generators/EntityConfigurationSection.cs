using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Database.Emitters;

namespace Reflow.Analyzer.Database
{
    internal class EntityConfigurationSection
        : GeneratorSection<DatabaseConfigurationSection, EntityConfigurationSection.SyntaxReceiver>
    {
        protected override NoData Execute(
            GeneratorExecutionContext context,
            SyntaxReceiver syntaxReceiver,
            DatabaseConfigurationSection previous
        )
        {
            var entityProxies = new Dictionary<ITypeSymbol, List<IPropertySymbol>>(
                SymbolEqualityComparer.Default
            );

            for (
                var configurationIndex = 0;
                configurationIndex < previous.Data.Count;
                configurationIndex++
            )
            {
                var configuration = previous.Data[configurationIndex];

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
                EntityConfigurationEmitter.Emit(previous.Data.SelectMany(x => x.Tables).ToList())
            );

            return default;
        }

        internal class SyntaxReceiver : ISyntaxContextReceiver
        {
            internal Dictionary<ITypeSymbol, INamedTypeSymbol> Candidates { get; }

            internal SyntaxReceiver()
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
