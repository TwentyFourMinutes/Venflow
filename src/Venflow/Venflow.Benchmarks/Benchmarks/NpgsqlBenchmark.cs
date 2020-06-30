//using BenchmarkDotNet.Attributes;
//using Npgsql;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using Venflow.Benchmarks.Benchmarks.Models;

//namespace Venflow.Benchmarks.Benchmarks
//{
//    public class NpgsqlBenchmark : BenchmarkBase
//    {
//        [GlobalSetup]
//        public override Task Setup()
//        {
//            return base.Setup();
//        }

//        [Benchmark]
//        public async Task RegularJoin()
//        {
//            var sb = new StringBuilder();

//            sb.Append(@"SELECT * FROM ""Persons""
//                        JOIN ""Emails"" Emails on ""Persons"".""Id"" = Emails.""PersonId"";");


//            using var command1 = new NpgsqlCommand(sb.ToString(), VenflowDbConnection.Connection);

//            using (var reader = await command1.ExecuteReaderAsync())
//            {
//                var map = new Dictionary<int, Person2>();
//                var entries = new List<Person2>();

//                while (await reader.ReadAsync())
//                {
//                    var id = reader.GetFieldValue<int>(0);

//                    if (map.TryGetValue(id, out var person))
//                    {
//                        person.Emails.Add(new Email2
//                        {
//                            Id = reader.GetFieldValue<int>(2),
//                            Address = reader.GetFieldValue<string>(3),
//                            PersonId = id,
//                        });
//                    }
//                    else
//                    {
//                        person = new Person2 { Id = id, Name = reader.GetFieldValue<string>(1), Emails = new List<Email2>() };

//                        entries.Add(person);
//                        map.Add(id, person);
//                    }
//                }
//            }
//        }

//        [Benchmark]
//        public async Task BetterJoin()
//        {
//            var sb = new StringBuilder();

//            sb.Append(@"SELECT * FROM ""Persons"";
//                        SELECT Emails.* FROM ""Persons""
//                        JOIN ""Emails"" Emails on ""Persons"".""Id"" = Emails.""PersonId"";");


//            using var command1 = new NpgsqlCommand(sb.ToString(), VenflowDbConnection.Connection);

//            using (var reader = await command1.ExecuteReaderAsync())
//            {
//                var map = new Dictionary<int, Person2>();
//                var entries = new List<Person2>();

//                while (await reader.ReadAsync())
//                {
//                    var id = reader.GetFieldValue<int>(0);

//                    var person = new Person2 { Id = id, Name = reader.GetFieldValue<string>(1), Emails = new List<Email2>() };

//                    entries.Add(person);
//                    map.Add(id, person);
//                }

//                await reader.NextResultAsync();

//                while (await reader.ReadAsync())
//                {
//                    var personId = reader.GetFieldValue<int>(2);
//                    var email = new Email2 { Id = reader.GetFieldValue<int>(0), Address = reader.GetFieldValue<string>(1), PersonId = personId };
//                    map[personId].Emails.Add(email);
//                }
//            }
//        }

//        [GlobalCleanup]
//        public override Task Cleanup()
//        {
//            return base.Cleanup();
//        }
//    }
//}
