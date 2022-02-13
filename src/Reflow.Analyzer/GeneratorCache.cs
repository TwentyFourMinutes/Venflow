using Microsoft.CodeAnalysis.Diagnostics;
using Newtonsoft.Json;

namespace Reflow.Analyzer
{
    internal class GeneratorCache
    {
        private readonly AnalyzerConfigOptions _config;
        private readonly string _cacheDirectory;

        internal GeneratorCache(AnalyzerConfigOptions config)
        {
            _config = config;

            if (config.TryGetValue("build_property.projectdir", out var projectDir))
            {
                _cacheDirectory = Path.Combine(projectDir, "obj/ReflowCache");

                if (!Directory.Exists(_cacheDirectory))
                {
                    Directory.CreateDirectory(_cacheDirectory);
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        internal T? GetData<T>(string name) where T : class
        {
            var dataFilePath = Path.Combine(_cacheDirectory, name);

            if (!File.Exists(dataFilePath))
                return default;

            var serializer = JsonSerializer.CreateDefault(
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
            );

            using var reader = new JsonTextReader(new StreamReader(File.OpenRead(dataFilePath)));

            return serializer.Deserialize<T>(reader);
        }

        internal void SetData<T>(string name, T data) where T : class
        {
            var dataFilePath = Path.Combine(_cacheDirectory, name);

            var serializer = JsonSerializer.CreateDefault(
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
            );
            using var writer = new JsonTextWriter(
                new StreamWriter(
                    File.Exists(dataFilePath)
                      ? File.OpenWrite(dataFilePath)
                      : File.Create(dataFilePath)
                )
            );

            serializer.Serialize(writer, data);
        }
    }
}
