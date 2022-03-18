using Microsoft.CodeAnalysis;

namespace Reflow.Analyzer.Shared
{
    internal static class CompilationExtensions
    {
        internal static void EnsureReference(
            this Compilation compilation,
            string assemblyName,
            IReadOnlyList<byte> publicKey
        )
        {
            if (
                !compilation.ReferencedAssemblyNames.Any(
                    x => x.Name == assemblyName && x.PublicKey.SequenceEqual(publicKey)
                )
            )
            {
                throw new DllNotFoundException(
                    $"The assembly '{assemblyName}' could not be found, but is required for the Reflow Source Generators."
                );
            }
        }

        internal static bool HasAssemblyReference(
            this Compilation compilation,
            string name,
            IReadOnlyList<byte> publicKey
        )
        {
            return compilation.ReferencedAssemblyNames.Any(
                x => x.Name == name && x.PublicKey.SequenceEqual(publicKey)
            );
        }
    }
}
