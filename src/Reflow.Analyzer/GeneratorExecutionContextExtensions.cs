using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reflow.Analyzer
{
    internal static class GeneratorExecutionContextExtensions
    {
        internal static void AddNamedSource(
            this GeneratorExecutionContext context,
            string name,
            SourceText sourceText
        )
        {
            context.AddSource(name + ".generated.cs", sourceText);
        }
    }
}
