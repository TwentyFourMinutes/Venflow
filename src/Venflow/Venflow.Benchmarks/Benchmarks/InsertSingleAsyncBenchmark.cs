using BenchmarkDotNet.Attributes;
using RepoDb;
using System.Threading.Tasks;
using Venflow.Benchmarks.Benchmarks.Models;
using Venflow.Commands;

namespace Venflow.Benchmarks.Benchmarks
{
    public class InsertSingleAsyncBenchmark : BenchmarkBase
    {
        //public InsertCommand<Person> Command { get; set; }

        //[GlobalSetup]
        //public override async Task Setup()
        //{
        //    await base.Setup();

        //    Command = VenflowDbConnection.CompileInsertCommand<Person>();
        //}

        //[Benchmark]
        //public Task VenflowInsertSingleAsync()
        //{
        //    return VenflowDbConnection.InsertAsync(Command, new Person { Name = "VenflowInsertSingleAsync" }, true);
        //}

        //[Benchmark]
        //public Task RepoDbInsertSingleAsync()
        //{
        //    return VenflowDbConnection.Connection.InsertAsync(new Person { Name = "RepoDbInsertSingleAsync" });
        //}

        //[GlobalCleanup]
        //public override Task Cleanup()
        //{
        //    Command.Dispose();

        //    return base.Cleanup();
        //}
    }
}
