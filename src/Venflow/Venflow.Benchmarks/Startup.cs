using BenchmarkDotNet.Running;
using System;

namespace Venflow.Benchmarks
{
    public class Startup
    {
        public static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Startup).Assembly).Run(args);
            Console.ReadKey();
        }
    }
}
