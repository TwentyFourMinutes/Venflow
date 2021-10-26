using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reflow.Internal
{
    public static class GeneratorExecutionContextExtensions
    {
        public static void AddNamedSource(
            this GeneratorExecutionContext context,
            string name,
            SourceText sourceText
        )
        {
            context.AddSource(name + ".generated.cs", sourceText);
        }
    }
}
