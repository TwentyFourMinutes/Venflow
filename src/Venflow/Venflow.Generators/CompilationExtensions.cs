using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Venflow.Generators
{
    internal static class CompilationExtensions
    {
        internal static bool ContainsAssembly(this Compilation compilation, MetadataReference[] references, AssemblyTokenInfo tokenInfo)
        {
            var reference = references.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Display) && Path.GetFileNameWithoutExtension(x.Display) == tokenInfo.Name);

            if (reference is null)
                return false;

            var symbol = compilation.GetAssemblyOrModuleSymbol(reference);

            if (symbol is IAssemblySymbol assemblySymbol)
            {
                return assemblySymbol.Identity.PublicKeyToken.SequenceEqual(tokenInfo.Token);
            }
            else if (symbol is IModuleSymbol moduleSymbol)
            {
                return moduleSymbol.ContainingAssembly.Identity.PublicKeyToken.SequenceEqual(tokenInfo.Token);
            }

            return false;
        }
    }
}
