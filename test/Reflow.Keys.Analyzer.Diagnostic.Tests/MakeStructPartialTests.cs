using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = Reflow.Keys.Analyzer.Diagnostic.Tests.CSharpCodeFixVerifier<
    Reflow.Keys.Analyzer.Diagnostic.MissingPartialKeywordAnalyzer,
    Reflow.Keys.Analyzer.Diagnostic.MissingPartialKeywordCodeFix
>;

namespace Reflow.Keys.Analyzer.Diagnostic.Tests
{
    [TestClass]
    public class MakeStructPartialTests
    {
        [TestMethod]
        public Task Empty_Diagnostic()
        {
            return VerifyCS.VerifyAnalyzerAsync(string.Empty);
        }

        [TestMethod]
        public Task StructIsPartial_Diagnostic()
        {
            var test = new VerifyCS.Test
            {
                TestState = { AdditionalReferences = { typeof(GeneratedKey).Assembly }, },
                TestCode =
                    @"
using System;
using Reflow;

namespace Test
{
    [GeneratedKey<int>]
    public partial struct Key<T>
    {

    }
}
"
            };

            return test.RunAsync();
        }

        [TestMethod]
        public Task StructIsNotPartialNoAttribute_Diagnostic()
        {
            var test = new VerifyCS.Test
            {
                TestState = { AdditionalReferences = { typeof(GeneratedKey).Assembly }, },
                TestCode =
                    @"
using System;
using Reflow;

namespace Test
{
    public struct Key<T>
    {

    }
}
"
            };

            return test.RunAsync();
        }

        [TestMethod]
        public Task StructShouldBePartial_Diagnostic()
        {
            var test = new VerifyCS.Test
            {
                ExpectedDiagnostics = { VerifyCS.Diagnostic("RF1001").WithSpan(7, 6, 7, 23) },
                TestState = { AdditionalReferences = { typeof(GeneratedKey).Assembly }, },
                TestCode =
                    @"
using System;
using Reflow;

namespace Test
{
    [GeneratedKey<int>]
    public struct Key<T>
    {

    }
}
",
                FixedCode =
                    @"
using System;
using Reflow;

namespace Test
{
    [GeneratedKey<int>]
    public partial struct Key<T>
    {

    }
}
",
            };

            return test.RunAsync();
        }
    }
}
