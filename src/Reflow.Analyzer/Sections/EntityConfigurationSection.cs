using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Reflow.Analyzer.Emitters;
using Reflow.Analyzer.Models;

namespace Reflow.Analyzer.Sections
{
    internal class EntityConfigurationSection
        : GeneratorSection<
              DatabaseConfigurationSection,
              EntityConfigurationSection.SyntaxReceiver,
              NoData
          >
    {
        [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1024:Compare symbols correctly")]
        protected override NoData Execute(
            GeneratorExecutionContext context,
            SyntaxReceiver syntaxReceiver,
            DatabaseConfigurationSection previous
        )
        {
            var entityProxies = new Dictionary<ITypeSymbol, List<Column>>(
                SymbolEqualityComparer.Default
            );

            for (
                var configurationIndex = 0;
                configurationIndex < previous.Data.Count;
                configurationIndex++
            )
            {
                var configuration = previous.Data[configurationIndex];

                foreach (var entity in configuration.Entities.Values)
                {
                    //if (!candidates.TryGetValue(table.EntityType, out var tableConfiguration))
                    //    continue;

                    var updatableProperties = new List<Column>();

                    for (var columnIndex = 0; columnIndex < entity.Columns.Count; columnIndex++)
                    {
                        var column = entity.Columns[columnIndex];

                        if (column.Symbol.IsVirtual)
                        {
                            updatableProperties.Add(column);
                        }
                    }

                    entityProxies.Add(entity.EntitySymbol, updatableProperties);
                }
            }

            context.AddNamedSource("EntityProxies", EntityProxyEmitter.Emit(entityProxies));
            context.AddNamedSource(
                "EntityConfigurations",
                EntityConfigurationEmitter.Emit(
                    previous.Data.SelectMany(x => x.Entities.Values).ToList()
                )
            );

            return default;
        }

        internal class SyntaxReceiver : ISyntaxContextReceiver
        {
            internal Dictionary<ITypeSymbol, INamedTypeSymbol> Candidates { get; }

            [System.Diagnostics.CodeAnalysis.SuppressMessage(
                "MicrosoftCodeAnalysisCorrectness",
                "RS1024:Compare symbols correctly",
                Justification = "<Pending>"
            )]
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
