using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Testing;

namespace Reflow.Keys.Analyzer.Diagnostic.Tests
{
    public static partial class CSharpCodeRefactoringVerifier<TCodeRefactoring>
        where TCodeRefactoring : CodeRefactoringProvider, new()
    {
        /// <inheritdoc cref="CodeRefactoringVerifier{TCodeRefactoring, TTest, TVerifier}.VerifyRefactoringAsync(string, string)"/>
        public static Task VerifyRefactoringAsync(string source, string fixedSource)
        {
            return VerifyRefactoringAsync(
                source,
                DiagnosticResult.EmptyDiagnosticResults,
                fixedSource
            );
        }

        /// <inheritdoc cref="CodeRefactoringVerifier{TCodeRefactoring, TTest, TVerifier}.VerifyRefactoringAsync(string, DiagnosticResult, string)"/>
        public static Task VerifyRefactoringAsync(
            string source,
            DiagnosticResult expected,
            string fixedSource
        )
        {
            return VerifyRefactoringAsync(source, new[] { expected }, fixedSource);
        }

        /// <inheritdoc cref="CodeRefactoringVerifier{TCodeRefactoring, TTest, TVerifier}.VerifyRefactoringAsync(string, DiagnosticResult[], string)"/>
        public static Task VerifyRefactoringAsync(
            string source,
            DiagnosticResult[] expected,
            string fixedSource
        )
        {
            var test = new Test { TestCode = source, FixedCode = fixedSource, };

            test.ExpectedDiagnostics.AddRange(expected);
            return test.RunAsync(CancellationToken.None);
        }
    }
}
