using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.CodeGenerator;
using Reflow.Analyzer.Models;
using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

namespace Reflow.Analyzer.Emitters
{
    internal static class EntityDataEmitter
    {
        internal static SourceText Emit(List<Entity> entities)
        {
            var entityData = new InitializerExpressionSyntax[entities.Count];

            for (var entityIndex = 0; entityIndex < entities.Count; entityIndex++)
            {
                var entity = entities[entityIndex];

                entityData[entityIndex] = DictionaryEntry(
                    TypeOf(Type(entity.EntitySymbol)),
                    Instance(Type("Reflow.Entity"))
                        .WithArguments(
                            Instance(
                                    GenericType(
                                        typeof(Dictionary<,>),
                                        Type(typeof(string)),
                                        Type("Reflow.Column")
                                    )
                                )
                                .WithArguments(Constant(entity.Columns.Count))
                                .WithInitializer(
                                    DictionaryInitializer(
                                        entity.Columns.Select(
                                            (x, i) =>
                                                DictionaryEntry(
                                                    Constant(x.Symbol.Name),
                                                    Instance(Type("Reflow.Column"))
                                                        .WithArguments(
                                                            Constant(x.Symbol.Name),
                                                            Constant(i)
                                                        )
                                                )
                                        )
                                    )
                                )
                        )
                );
            }

            return File("Reflow")
                .WithMembers(
                    Class("EntityData", CSharpModifiers.Internal | CSharpModifiers.Static)
                        .WithMembers(
                            Field(
                                    "Data",
                                    DictionaryType(),
                                    CSharpModifiers.Internal | CSharpModifiers.Static
                                )
                                .WithInitializer(
                                    Instance(DictionaryType())
                                        .WithArguments(Constant(entityData.Length))
                                        .WithInitializer(DictionaryInitializer(entityData))
                                )
                        )
                )
                .GetText();

            static TypeSyntax DictionaryType() =>
                GenericType(typeof(Dictionary<,>), Type(typeof(Type)), Type("Reflow.Entity"));
        }
    }
}
