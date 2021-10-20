using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reflow.Analyzer
{
    internal static class GeneratorExecutionContextExtensions
    {
        internal static void AddGeneratorSources(
            this GeneratorExecutionContext context,
            [CallerFilePath] string? fullName = null
        ) {
            var directoryName = Path.GetDirectoryName(fullName);
            var parentIndex = directoryName.LastIndexOf('\\') + 1;

            var resourceBasePath =
                "Shared\\"
                + directoryName.Substring(parentIndex, directoryName.Length - parentIndex)
                + "\\";

            var resourceNames =
                typeof(GeneratorExecutionContextExtensions).Assembly.GetManifestResourceNames()
                    .Where(x => x.StartsWith(resourceBasePath));

            foreach (var resourceName in resourceNames)
            {
                context.AddSource(
                    Path.GetFileNameWithoutExtension(resourceName)
                        + ".generated"
                        + Path.GetExtension(resourceName),
                    SourceText.From(EmbeddedResource.GetContent(resourceName, true), Encoding.UTF8)
                );
            }
        }
    }
}
