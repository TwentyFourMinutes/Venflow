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
        protected override NoData Execute(
            GeneratorExecutionContext context,
            SyntaxReceiver syntaxReceiver,
            DatabaseConfigurationSection previous
        )
        {
            var configurations = syntaxReceiver.Candidates;
            var entities = new List<Entity>();
            var entityProxies = new Dictionary<ITypeSymbol, List<Column>>(
                SymbolEqualityComparer.Default
            );
            var abosluteRelationIndex = 0;

            for (var databaseIndex = 0; databaseIndex < previous.Data.Count; databaseIndex++)
            {
                var database = previous.Data[databaseIndex];

                for (
                    var entitySymbolIndex = 0;
                    entitySymbolIndex < database.EntitySymbols.Count;
                    entitySymbolIndex++
                )
                {
                    var (propertySymbol, entitySymbol) = database.EntitySymbols[entitySymbolIndex];

                    configurations.TryGetValue(entitySymbol, out var configurationData);

                    var entity = Entity.Construct(
                        configurationData.SemanticModel,
                        propertySymbol,
                        entitySymbol,
                        configurationData.BlockSyntax
                    );

                    var updatableProperties = new List<Column>();

                    for (var columnIndex = 0; columnIndex < entity.Columns.Count; columnIndex++)
                    {
                        var column = entity.Columns[columnIndex];

                        if (column.IsUpdatable)
                        {
                            updatableProperties.Add(column);
                        }
                    }

                    entities.Add(entity);
                    database.Entities.Add(entity.Symbol, entity);
                    entityProxies.Add(entity.Symbol, updatableProperties);
                }

                foreach (var entity in database.Entities.Values)
                {
                    if (entity.Relations.Count == 0)
                        continue;

                    for (
                        var relationIndex = 0;
                        relationIndex < entity.Relations.Count;
                        relationIndex++
                    )
                    {
                        var relation = entity.Relations[relationIndex];

                        if (relation.IsProcessed)
                            break;

                        relation.Id = abosluteRelationIndex++;
                        relation.IsProcessed = true;

                        if (
                            !database.Entities.TryGetValue(
                                relation.RightEntitySymbol,
                                out var rightEntity
                            )
                        )
                        {
                            throw new InvalidOperationException();
                        }

                        var navigationColumn = relation.RightNavigationProperty is not null
                            ? rightEntity.Columns.FirstOrDefault(
                                  x => x.PropertyName == relation.RightNavigationProperty.Name
                              )
                            : null;

                        if (navigationColumn is not null)
                            rightEntity.Columns.Remove(navigationColumn);

                        rightEntity.Relations.Add(relation.CreateMirror());
                    }
                }

                database.EntitySymbols.Clear();
            }

            context.AddNamedSource("EntityProxies", EntityProxyEmitter.Emit(entityProxies));
            context.AddNamedSource("EntityData", EntityDataEmitter.Emit(entities));

            return default;
        }

        internal class SyntaxReceiver : ISyntaxContextReceiver
        {
            internal Dictionary<
                ITypeSymbol,
                (SemanticModel SemanticModel, BlockSyntax? BlockSyntax)
            > Candidates { get; }

            internal SyntaxReceiver()
            {
                Candidates = new(SymbolEqualityComparer.Default);
            }

            void ISyntaxContextReceiver.OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (
                    context.Node is not ClassDeclarationSyntax classSyntax
                    || classSyntax.BaseList is null
                )
                    return;

                var baseInterface = classSyntax.BaseList.Types.FirstOrDefault(
                    x =>
                        x.Type is SimpleNameSyntax nameSyntax
                        && nameSyntax.Identifier.ValueText.EndsWith("IEntityConfiguration")
                );

                if (baseInterface is null)
                    return;

                var interfaceSymbol = (INamedTypeSymbol)context.SemanticModel.GetSymbolInfo(
                    baseInterface.Type
                ).Symbol!;

                if (
                    interfaceSymbol.GetFullName() is not "Reflow.Modeling.IEntityConfiguration"
                    || !interfaceSymbol.IsReflowSymbol()
                )
                    return;

                var configureMethod = (
                    (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(classSyntax)!
                )
                    .GetMembers()
                    .Single(x => x.Name.EndsWith("Configure"));

                Candidates.Add(
                    interfaceSymbol.TypeArguments[0],
                    (
                        context.SemanticModel,
                        (
                            (MethodDeclarationSyntax)classSyntax.FindNode(
                                configureMethod.Locations[0].SourceSpan
                            )
                        ).Body!
                    )
                );
            }
        }
    }
}
