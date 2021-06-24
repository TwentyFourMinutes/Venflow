using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Venflow.Generators;
using Xunit;

namespace Venflow.Tests.GeneratorTests
{
    public class KeyGeneration
    {
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(string))]
        [InlineData(typeof(Guid))]
        public void GenerateKey(Type underlyingKeyType)
        {
            var inputCompilation = CreateCompilation(
$@"using Venflow;

namespace MyCode
{{
    public class Program
    {{
        public static void Main(string[] args)
        {{
        }}
    }}

    [GeneratedKey(typeof({underlyingKeyType.FullName}))]
    public partial struct Key<T> {{ }}
}}", typeof(IKey).Assembly, typeof(System.Text.Json.JsonSerializer).Assembly, typeof(Venflow.NewtonsoftJson.NewtonsoftJsonKeyConverter).Assembly);

            var generator = new KeyGenerator();

            var driver = (GeneratorDriver)CSharpGeneratorDriver.Create(generator);

            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            Assert.Empty(diagnostics);
            Assert.Equal(3, outputCompilation.SyntaxTrees.Count());
            Assert.Empty(outputCompilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error));

            var runResult = driver.GetRunResult();

            Assert.Equal(2, runResult.GeneratedTrees.Count());
            Assert.Empty(runResult.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error));

            var generatorResult = runResult.Results.First();

            Assert.Equal(generator, generatorResult.Generator);
            Assert.Empty(generatorResult.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error));
            Assert.Equal(2, generatorResult.GeneratedSources.Count());
            Assert.Null(generatorResult.Exception);
        }

        private static Compilation CreateCompilation(string source, params Assembly[] references)
        {
            var loadedAssemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
            var effectiveReferences = new List<Assembly>();

            foreach (var assembly in references.Union(new[] { typeof(Binder).Assembly, typeof(IConvertible).Assembly }))
            {
                var referencedAssemblies = assembly.GetReferencedAssemblies();

                effectiveReferences.Add(assembly);

                foreach (var referencedAssemblyName in referencedAssemblies)
                {
                    var referencedAssembly = loadedAssemblies.FirstOrDefault(x => x.FullName == referencedAssemblyName.FullName);

                    if (referencedAssembly is null)
                    {
                        referencedAssembly = Assembly.Load(referencedAssemblyName);

                        loadedAssemblies.Add(referencedAssembly);

                        effectiveReferences.Add(referencedAssembly);
                    }
                    else if (!effectiveReferences.Any(x => x.FullName == referencedAssembly.FullName))
                    {
                        effectiveReferences.Add(referencedAssembly);
                    }
                }
            }

            return CSharpCompilation.Create("compilation", new[] { CSharpSyntaxTree.ParseText(source) }, effectiveReferences.Select(x => MetadataReference.CreateFromFile(x.Location)), new CSharpCompilationOptions(OutputKind.ConsoleApplication, nullableContextOptions: NullableContextOptions.Enable));
        }
    }
}