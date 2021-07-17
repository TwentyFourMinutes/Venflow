using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Venflow.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    public class MiscBenchmarks : BenchmarkBase
    {
        private readonly int[] array = Array.Empty<int>();

        [GlobalSetup]
        public override Task Setup()
        {
            return base.Setup();
        }

        [Benchmark]
        public Span<int> SpanOverhead()
        {
            return array.AsSpan();
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}
