using BenchmarkDotNet.Running;
using System;

namespace Venflow.Benchmarks
{
    public class Startup
    {
        public static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Startup).Assembly).Run(args);
            //BenchmarkSwitcher.FromTypes(new[] { typeof(QuerySingleAsyncBenchmark), typeof(QueryBatchAsyncBenchmark), typeof(QueryBatchWithRelationsAsyncBenchmark), typeof(QuerySingleWithRelationsAsyncBenchmark) }).Run(args);
            Console.ReadKey();
        }
    }
}
