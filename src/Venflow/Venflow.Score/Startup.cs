using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Venflow.Score
{
    public static class Startup
    {
        private static readonly string[] _ormNames = new[] { "EFCore", "Venflow", "RepoDb", "Dapper" };

        private static readonly Dictionary<string, Orm> _orms = new Dictionary<string, Orm>(_ormNames.Length);

        public static void Main()
        {
            string rootDirectory;

            while (true)
            {
                Console.WriteLine("Enter the full path to the root directory of all the .csv benchmark results.");
                rootDirectory = Console.ReadLine();

                if (!Directory.Exists(rootDirectory))
                {
                    Console.WriteLine("This directory doesn't exist.");

                    continue;
                }

                break;
            }

            foreach (var ormName in _ormNames)
            {
                _orms.Add(ormName, new Orm(ormName));
            }

            CalculateOrmResults(ReadBenchResultFiles(rootDirectory));

            var index = 0;

            foreach (var orm in _orms.Values.OrderBy(x => x.AllocScore + x.BenchScore))
            {
                Console.WriteLine($"#{++index} {orm.OrmName} - {Math.Round(orm.AllocScore + orm.BenchScore, 3)}. Mean Score: {Math.Round(orm.BenchScore, 3)}, Allocation Score: {Math.Round(orm.AllocScore, 3) }.");
            }

            Console.ReadKey();
        }

        public static IEnumerable<List<BenchResult>> ReadBenchResultFiles(string rootDirectory)
        {
            var benchResults = new List<BenchResult>();
            string? lastJobName = null;

            foreach (var fullFileName in Directory.EnumerateFiles(rootDirectory, "*.csv", SearchOption.TopDirectoryOnly))
            {
                using var reader = new StreamReader(fullFileName);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                foreach (var benchResult in csv.GetRecords<BenchResult>())
                {
                    if (lastJobName is { } &&
                       benchResult.Job != lastJobName)
                    {
                        yield return benchResults;

                        benchResults = new List<BenchResult>();
                    }

                    lastJobName = benchResult.Job;

                    benchResults.Add(benchResult);
                }

                yield return benchResults;
            }
        }

        public static void CalculateOrmResults(IEnumerable<List<BenchResult>> batchBenchResults)
        {
            foreach (var batchBenchResult in batchBenchResults)
            {
                var lowestTime = double.MaxValue;
                var highestTime = double.MinValue;
                var lowestAlloc = double.MaxValue;
                var highestAlloc = double.MinValue;

                foreach (var benchResult in batchBenchResult)
                {
                    var benchOrmName = _ormNames.FirstOrDefault(x => benchResult.Method.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) != -1);

                    if (benchOrmName is null ||
                        !_orms.TryGetValue(benchOrmName, out var orm))
                    {
                        throw new InvalidOperationException($"No ORM name in method '{benchResult.Method}' found.");
                    }

                    var benchTime = benchResult.Mean.GetNanoSecondTime();
                    var benchAlloc = benchResult.Allocated.GetAllocation();

                    orm.AddBenchScore(benchTime, benchAlloc);

                    if (lowestTime > benchTime)
                        lowestTime = benchTime;

                    if (highestTime < benchTime)
                        highestTime = benchTime;

                    if (lowestAlloc > benchAlloc)
                        lowestAlloc = benchAlloc;

                    if (highestAlloc < benchAlloc)
                        highestAlloc = benchAlloc;
                }

                foreach (var orm in _orms.Values)
                {
                    if (!orm.HasCurrentBench)
                    {
                        //orm.AddBenchScore(highestTime, highestAlloc);
                        orm.AddBenchScore(lowestTime, lowestAlloc);
                    }

                    orm.FinishBench(lowestTime, lowestAlloc);
                }
            }
        }
    }
}
