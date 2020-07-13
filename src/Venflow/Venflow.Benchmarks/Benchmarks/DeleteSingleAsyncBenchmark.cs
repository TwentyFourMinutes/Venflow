//using BenchmarkDotNet.Attributes;
//using RepoDb;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Venflow.Benchmarks.Benchmarks.Models;

//namespace Venflow.Benchmarks.Benchmarks
//{
//    [MemoryDiagnoser]
//    [SimpleJob(launchCount: 1, warmupCount: 50, targetCount: 100, invocationCount: 256)]
//    public class DeleteSingleAsyncBenchmark : BenchmarkBase
//    {
//        private Person _toDeleteVenflow;
//        private Person _toDeleteRepoDb;

//        [GlobalSetup]
//        public override async Task Setup()
//        {
//            await base.Setup();

//            IterationSetup();

//            await VenflowDbConnection.Connection.DeleteAsync(_toDeleteRepoDb);
//            await VenflowDbConnection.DeleteSingleAsync(_toDeleteVenflow);
//        }

//        // The IterationSetup can spoil results, as noted in their documentation see: https://benchmarkdotnet.org/articles/features/setup-and-cleanup.html#sample-introsetupcleanupiteration.
//        // However we don't really have any other proper way of creating the amount of entities required to delete.
//        [IterationSetup]
//        public void IterationSetup()
//        {
//            _toDeleteVenflow = new Person { Name = "toDeleteVenflow" };
//            _toDeleteRepoDb = new Person { Name = "toDeleteRepoDb" };

//            VenflowDbConnection.InsertBatchAsync(new List<Person> { _toDeleteVenflow, _toDeleteRepoDb }).GetAwaiter().GetResult();
//        }

//        [Benchmark]
//        public Task VenflowDeleteSingleAsync()
//        {
//            return VenflowDbConnection.DeleteSingleAsync(_toDeleteVenflow);
//        }

//        [Benchmark]
//        public Task RepoDbDeleteSingleAsync()
//        {
//            return VenflowDbConnection.Connection.DeleteAsync(_toDeleteRepoDb);
//        }

//        [GlobalCleanup]
//        public override Task Cleanup()
//        {
//            return base.Cleanup();
//        }
//    }
//}
