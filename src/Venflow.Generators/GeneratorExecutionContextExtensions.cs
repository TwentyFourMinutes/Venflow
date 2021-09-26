using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Venflow.Generators
{
    internal static class GeneratorExecutionContextExtensions
    {
        internal static Compilation AddResourceSource(this GeneratorExecutionContext context, string fileName, bool addToSyntaxTree = false)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream("Venflow.Generators.Properties." + fileName + ".cs");

            var sourceText = SourceText.From(stream, Encoding.UTF8, canBeEmbedded: true);

            context.AddSource(fileName, sourceText);

            if (addToSyntaxTree)
            {
                return context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(sourceText, (context.Compilation as CSharpCompilation)!.SyntaxTrees[0].Options as CSharpParseOptions));
            }

            return context.Compilation;
        }
    }
}
