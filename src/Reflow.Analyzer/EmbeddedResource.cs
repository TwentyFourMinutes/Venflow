using System;
using System.IO;

namespace Reflow.Analyzer
{
    internal static class EmbeddedResource
    {
        internal static string GetContent(string relativePath, bool isFullPath = false)
        {
            var assembly = typeof(EmbeddedResource).Assembly;

            var baseName = isFullPath ? null : assembly.GetName().Name;
            var resourceName = isFullPath
                ? relativePath
                : relativePath.TrimStart('.')
                      .Replace(Path.DirectorySeparatorChar, '.')
                      .Replace(Path.AltDirectorySeparatorChar, '.');

            using var stream = assembly.GetManifestResourceStream(
                isFullPath ? resourceName : baseName + "." + resourceName
            );

            if (stream == null)
                throw new NotSupportedException();

            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
