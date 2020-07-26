using BenchmarkDotNet.Running;
using System;
using Venflow.Benchmarks.Benchmarks.QueryBenchmarks;

namespace Venflow.Benchmarks
{
    public class Startup
    {
        public static void Main(string[] args)
        {
            //BenchmarkSwitcher.FromAssembly(typeof(Startup).Assembly).Run(args);
            BenchmarkSwitcher.FromTypes(new[] { typeof(QuerySingleAsyncBenchmark), typeof(QueryBatchAsyncBenchmark) }).Run(args);
            Console.ReadKey();
        }
    }
}
