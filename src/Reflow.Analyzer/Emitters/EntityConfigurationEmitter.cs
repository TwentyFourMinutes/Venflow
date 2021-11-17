using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.CodeGenerator;
using Reflow.Analyzer.Models;
using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

namespace Reflow.Analyzer.Emitters
{
    internal static class EntityConfigurationEmitter
    {
        internal static SourceText Emit(List<Entity> entities)
        {
            var entityConfigurations = new SyntaxList<InitializerExpressionSyntax>();

            for (var tableIndex = 0; tableIndex < entities.Count; tableIndex++)
            {
                var entity = entities[tableIndex];

                entityConfigurations = entityConfigurations.Add(
                    DictionaryEntry(
                        TypeOf(Type(entity.EntitySymbol)),
                        ArrayInitializer(
                            Array(Type(typeof(string))),
                            entity.Columns.Select(x => Constant(x.Symbol.Name)) // TODO: Requires actual name
                        )
                    )
                );
            }

            return File("Reflow")
                .WithMembers(
                    Class("EntityConfigurations", CSharpModifiers.Public)
                        .WithMembers(
                            Field("Configurations", DictionaryType())
                                .WithInitializer(
                                    Instance(DictionaryType())
                                        .WithArguments(Constant(entityConfigurations.Count))
                                        .WithInitializer(
                                            DictionaryInitializer(entityConfigurations)
                                        )
                                )
                        )
                )
                .GetText();

            static TypeSyntax DictionaryType() =>
                GenericType(typeof(Dictionary<,>), Type(typeof(Type)), Array(Type(typeof(string))));
        }
    }
}
