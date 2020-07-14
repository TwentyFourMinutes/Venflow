using BenchmarkDotNet.Attributes;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Venflow.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    public class MiscBenchmarks : BenchmarkBase
    {
        [GlobalSetup]
        public override Task Setup()
        {
            return base.Setup();
        }

        [Benchmark]
        public (string, string, int) GetStackTrace()
        {
            var frame = new StackTrace(1, true).GetFrame(0);

            return (frame.GetFileName(), frame.GetMethod().Name, frame.GetFileLineNumber());
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}
