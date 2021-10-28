using Microsoft.CodeAnalysis;

namespace Reflow.Analyzer.Database
{
    [Generator(LanguageNames.CSharp)]
    internal class DatabaseGeneratorGroup : SourceGeneratorGroup<DatabaseGeneratorGroup.Data>
    {
        public DatabaseGeneratorGroup()
        {
            SourceGenerators.Add(new DatabaseConfigurationGenerator());
            SourceGenerators.Add(new EntityConfigurationGenerator());
        }

        internal class Data
        {
            internal List<DatabaseConfiguration> Configurations { get; } = new();
        }
    }
}
