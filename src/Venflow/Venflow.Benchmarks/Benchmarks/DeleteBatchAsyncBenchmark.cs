//using BenchmarkDotNet.Attributes;
//using RepoDb;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Venflow.Benchmarks.Benchmarks.Models;

//namespace Venflow.Benchmarks.Benchmarks
//{
//    [MemoryDiagnoser]
//    [SimpleJob(launchCount: 1, warmupCount: 50, targetCount: 100, invocationCount: 128)]
//    public class DeleteBatchAsyncBenchmark : BenchmarkBase
//    {
//        private List<Person> _toDeleteVenflow;
//        private List<Person> _toDeleteRepoDb;

//        [GlobalSetup]
//        public override async Task Setup()
//        {
//            await base.Setup();

//            IterationSetup();

//            await VenflowDbConnection.Connection.DeleteAllAsync(_toDeleteRepoDb);
//            await VenflowDbConnection.DeleteBatchAsync(_toDeleteVenflow);
//        }

//        // The IterationSetup can spoil results, as noted in their documentation see: https://benchmarkdotnet.org/articles/features/setup-and-cleanup.html#sample-introsetupcleanupiteration.
//        // However we don't really have any other proper way of creating the amount of entities required to delete.
//        [IterationSetup]
//        public void IterationSetup()
//        {
//            _toDeleteVenflow = new List<Person>();
//            _toDeleteRepoDb = new List<Person>();

//            var toDelete = new List<Person>();

//            for (int i = 0; i < 100; i++)
//            {
//                var toDeleteVen = new Person { Name = "toDeleteVenflow" + i.ToString() };
//                _toDeleteVenflow.Add(toDeleteVen);
//                toDelete.Add(toDeleteVen);

//                var toDeleteRepo = new Person { Name = "toDeleteRepoDb" + i.ToString() };
//                _toDeleteRepoDb.Add(toDeleteRepo);
//                toDelete.Add(toDeleteRepo);
//            }

//            VenflowDbConnection.InsertBatchAsync(toDelete).GetAwaiter().GetResult();
//        }

//        [Benchmark]
//        public Task VenflowDeleteBatchAsync()
//        {
//            return VenflowDbConnection.DeleteBatchAsync(_toDeleteVenflow);
//        }

//        [Benchmark]
//        public Task RepoDbDeleteBatchAsync()
//        {
//            return VenflowDbConnection.Connection.DeleteAllAsync(_toDeleteRepoDb);
//        }

//        [GlobalCleanup]
//        public override Task Cleanup()
//        {
//            return base.Cleanup();
//        }
//    }
//}
