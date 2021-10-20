using System;
using System.IO;

namespace Reflow.Analyzer
{
    internal static class EmbeddedResource
    {
        internal static string GetContent(string relativePath)
        {
            var assembly = typeof(EmbeddedResource).Assembly;

            var baseName = assembly.GetName().Name;
            var resourceName = relativePath.TrimStart('.')
                .Replace(Path.DirectorySeparatorChar, '.')
                .Replace(Path.AltDirectorySeparatorChar, '.');

            using var stream = assembly.GetManifestResourceStream(baseName + "." + resourceName);

            if (stream == null)
                throw new NotSupportedException();

            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
