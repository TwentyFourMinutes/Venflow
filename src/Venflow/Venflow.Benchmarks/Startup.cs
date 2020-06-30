using BenchmarkDotNet.Running;
using System;
using Venflow.Benchmarks.Benchmarks;

namespace Venflow.Benchmarks
{
    public class Startup
    {
        public static void Main(string[] args)
        {
            //BenchmarkSwitcher.FromAssembly(typeof(Startup).Assembly).Run(args);
            BenchmarkSwitcher.FromTypes(new[] { typeof(QueryBatchWithRelationsAsyncBenchmark) }).Run(args);
            Console.ReadKey();
        }
    }
}
