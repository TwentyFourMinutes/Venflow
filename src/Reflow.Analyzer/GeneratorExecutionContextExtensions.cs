using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reflow.Analyzer
{
    public static class GeneratorExecutionContextExtensions
    {
        public static void AddNamedSource(
            this GeneratorExecutionContext context,
            string name,
            SourceText sourceText
        )
        {
            var a = sourceText.ToString();
            context.AddSource(name + ".generated.cs", sourceText);
        }
    }
}
