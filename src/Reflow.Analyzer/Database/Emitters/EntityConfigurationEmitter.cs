using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Internal;
using static Reflow.Internal.CSharpCodeGenerator;

namespace Reflow.Analyzer.Database.Emitters
{
    internal static class EntityConfigurationEmitter
    {
        internal static SourceText Emit(List<DatabaseTable> tables)
        {
            var entityConfigurations = new SyntaxList<InitializerExpressionSyntax>();

            for (var tableIndex = 0; tableIndex < tables.Count; tableIndex++)
            {
                var table = tables[tableIndex];

                entityConfigurations = entityConfigurations.Add(
                    DictionaryEntry(
                        TypeOf(Type(table.EntityType)),
                        ArrayInitializer(
                            Array(Type(typeof(string))),
                            table.Columns.Select(x => Constant(x.Name)) // TODO: Requires actual name
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
                GenericType(
                    typeof(Dictionary<, >),
                    Type(typeof(Type)),
                    Array(Type(typeof(string)))
                );
        }
    }
}
