using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Reflow.Keys.Analyzer.Diagnostic.Tests
{
    public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, MSTestVerifier>
        {
            public Test()
            {
                SolutionTransforms.Add(
                    (solution, projectId) =>
                    {
                        var compilationOptions = solution.GetProject(projectId)!.CompilationOptions;
                        compilationOptions = compilationOptions!.WithSpecificDiagnosticOptions(
                            compilationOptions.SpecificDiagnosticOptions.SetItems(
                                CSharpVerifierHelper.NullableWarnings
                            )
                        );
                        solution = solution.WithProjectCompilationOptions(
                            projectId,
                            compilationOptions
                        );

                        return solution;
                    }
                );
            }

            protected override ParseOptions CreateParseOptions()
            {
                return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(
                    LanguageVersion.Preview
                );
            }
        }
    }
}
