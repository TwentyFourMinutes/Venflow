using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Venflow.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    public class MiscBenchmarks : BenchmarkBase
    {
        private readonly Type _type = typeof(List<int>);

        [GlobalSetup]
        public override Task Setup()
        {
            return base.Setup();
        }

        [Benchmark]
        public List<int> InstantiateWithActivator()
        {
            return (List<int>) Activator.CreateInstance(_type);
        }

        [Benchmark]
        public List<int> InstantiateWithGenericActivator()
        {
            return BaseInstantiateWithGenericActivator<List<int>>();
        }

        public T BaseInstantiateWithGenericActivator<T>() where T : class, new()
        {
            return new T();
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}
