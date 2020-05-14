using BenchmarkDotNet.Running;
using System;
using Venflow.Benchmarks.Benchmarks;

namespace Venflow.Benchmarks
{

    public class Startup
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<MiscBenchmarks>();
            //BenchmarkRunner.Run<NpgsqlBenchmark>();
            //BenchmarkRunner.Run<QuerySingleAsyncBenchmark>();
            //BenchmarkRunner.Run<QueryAllAsyncBenchmark>();
            //BenchmarkRunner.Run<InsertSingleAsyncBenchmark>();

            Console.ReadKey();
        }
    }
}
