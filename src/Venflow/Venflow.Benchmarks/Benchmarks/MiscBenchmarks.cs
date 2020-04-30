using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using Venflow.Benchmarks.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks
{
    public class MiscBenchmarks
    {
        HashSet<Person> personsHS;
        List<Person> personsL;

        [GlobalSetup]
        public void Setup()
        {
            personsHS = new HashSet<Person>();
            personsL = new List<Person>();
        }

        [Benchmark]
        public void List()
        {
            personsL.Clear();
            for (int i = 0; i < 20000; i++)
            {
                personsL.Add(new Person());
            }
        }

        [Benchmark]
        public void HashSet()
        {
            personsHS.Clear();
            for (int i = 0; i < 20000; i++)
            {
                personsHS.Add(new Person());
            }
        }
    }
}
