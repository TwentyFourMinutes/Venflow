using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;

namespace Reflow.Analyzer
{
    internal static class GeneratorExecutionContextExtensions
    {
        internal static void AddTemplatedSource(
            this GeneratorExecutionContext context,
            string relativePath,
            object model
        )
        {
            var template = Template.Parse(
                EmbeddedResource.GetContent(relativePath),
                Path.GetFileName(relativePath)
            );

            context.AddSource(
                Path.GetFileNameWithoutExtension(relativePath) + ".generated.cs",
                SourceText.From(template.Render(model), Encoding.UTF8)
            );
        }
    }
}
