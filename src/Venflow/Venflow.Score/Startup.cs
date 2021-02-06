using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace Venflow.Score
{
    public static class Startup
    {
        private static readonly (string Name, string Link)[] _ormNames = new[]
        {
            ("EFCore","https://github.com/dotnet/efcore"),
            ("Venflow","https://github.com/TwentyFourMinutes/Venflow"),
            ("RepoDb","https://github.com/mikependon/RepoDb"),
            ("Dapper","https://github.com/StackExchange/Dapper")
        };

        private static readonly Dictionary<string, Orm> _orms = new Dictionary<string, Orm>(_ormNames.Length);

        public static async Task Main(string[] args)
        {
            string rootDirectory;

            while (true)
            {
                Console.WriteLine("Enter the full path to the root directory of all the .csv benchmark results.");
                rootDirectory = args.Length > 0 ? args[0] : Console.ReadLine() ?? string.Empty;

                if (!Directory.Exists(rootDirectory))
                {
                    Console.WriteLine("This directory doesn't exist.");

                    continue;
                }

                break;
            }

            foreach (var ormName in _ormNames)
            {
                _orms.Add(ormName.Name, new Orm(ormName));
            }

            if (args.Length > 2)
            {
                var benchmarkDestionationPath = args[1];

                if (!Directory.Exists(benchmarkDestionationPath))
                    throw new ArgumentException($"The directory '{benchmarkDestionationPath}' could not be found.");

                var readmePath = args[2];

                if (!File.Exists(readmePath))
                    throw new ArgumentException($"The file '{readmePath}' could not be found.");

                var fileExtensions = new string[] { ".csv", ".md" };

                foreach (var fullFileName in Directory.EnumerateFiles(rootDirectory))
                {
                    if (!fileExtensions.Any(x => fullFileName.EndsWith(x)))
                        continue;

                    var fileName = Path.GetFileName(fullFileName);

                    var reportIndex = fileName.IndexOf("-report");

                    var benchmarkNameIndex = fileName.LastIndexOf(".", reportIndex) + 1;

                    var newFileName = fileName[benchmarkNameIndex..reportIndex];

                    newFileName = newFileName.TrimEnd("Benchmark").TrimEnd("Async");

                    newFileName += Path.GetExtension(fileName);

                    File.Copy(fullFileName, Path.Combine(benchmarkDestionationPath, newFileName), true);
                }

                await CalculateOrmResults(ReadBenchResultFiles(benchmarkDestionationPath));

                var readmeFileRawContent = await File.ReadAllTextAsync(readmePath);

                var readmeFileContent = new StringBuilder(readmeFileRawContent);

                string benchmarkStartMarker = "<!--Benchmark Start-->" + Environment.NewLine;
                const string benchmarkEndMarker = "<!--Benchmark End-->";

                var benchmarkStartIndex = readmeFileRawContent.IndexOf(benchmarkStartMarker) + benchmarkStartMarker.Length;

                var benchmarkEndIndex = readmeFileRawContent.IndexOf(benchmarkEndMarker, benchmarkStartIndex);

                readmeFileContent.Remove(benchmarkStartIndex, benchmarkEndIndex - benchmarkStartIndex);

                InsertMarkdownTable(readmeFileContent, benchmarkStartIndex, new[] { "ORM Name", "Composite Score\\*", "Mean Score\\*", "Allocation Score\\*" }, _orms.Values.OrderBy(x => x.AllocScore + x.BenchScore).Select((x, index) => new string[] { $"#{++index} [{x.OrmName.Name}]({x.OrmName.Link})", Math.Round(x.AllocScore + x.BenchScore, 3).ToString(), Math.Round(x.BenchScore, 3).ToString(), Math.Round(x.AllocScore, 3).ToString() }));

                await File.WriteAllTextAsync(readmePath, readmeFileContent.ToString());
            }
            else
            {
                await CalculateOrmResults(ReadBenchResultFiles(rootDirectory));

                var index = 0;

                foreach (var orm in _orms.Values.OrderBy(x => x.AllocScore + x.BenchScore))
                {
                    Console.WriteLine($"#{++index} {orm.OrmName.Name} - {Math.Round(orm.AllocScore + orm.BenchScore, 3)}. Mean Score: {Math.Round(orm.BenchScore, 3)}, Allocation Score: {Math.Round(orm.AllocScore, 3) }.");
                }

                Console.ReadKey();
            }
        }

        private static async IAsyncEnumerable<List<BenchResult>> ReadBenchResultFiles(string rootDirectory)
        {
            var benchResults = new List<BenchResult>();
            string? lastJobName = null;

            foreach (var fullFileName in Directory.EnumerateFiles(rootDirectory, "*.csv", SearchOption.TopDirectoryOnly))
            {
                using var reader = new StreamReader(fullFileName);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                await foreach (var benchResult in csv.GetRecordsAsync<BenchResult>())
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

        private static async Task CalculateOrmResults(IAsyncEnumerable<List<BenchResult>> batchBenchResults)
        {
            await foreach (var batchBenchResult in batchBenchResults)
            {
                var lowestTime = double.MaxValue;
                var highestTime = double.MinValue;
                var lowestAlloc = double.MaxValue;
                var highestAlloc = double.MinValue;

                foreach (var benchResult in batchBenchResult)
                {
                    var benchOrmName = _ormNames.FirstOrDefault(x => benchResult.Method.IndexOf(x.Name, StringComparison.InvariantCultureIgnoreCase) != -1);

                    if (benchOrmName.Name == default ||
                        !_orms.TryGetValue(benchOrmName.Name, out var orm))
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

        private static void InsertMarkdownTable(StringBuilder stringBuilder, int index, IEnumerable<string> headers, IEnumerable<IEnumerable<string>> rows)
        {
            int columnCount = 0;

            foreach (var header in headers)
            {
                if (columnCount == 0)
                {
                    stringBuilder.Insert(index, "| ");

                    index += 2;
                }
                else
                {
                    stringBuilder.Insert(index, " | ");

                    index += 3;
                }

                stringBuilder.Insert(index, header);
                index += header.Length;

                columnCount++;
            }

            stringBuilder.Insert(index, " |" + Environment.NewLine);
            index += 2 + Environment.NewLine.Length;

            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                if (columnIndex == 0)
                {
                    stringBuilder.Insert(index, "| :- ");

                    index += 5;
                }
                else
                {
                    stringBuilder.Insert(index, "| :-: ");

                    index += 6;
                }
            }

            stringBuilder.Insert(index, "|" + Environment.NewLine);
            index += 1 + Environment.NewLine.Length;

            foreach (var row in rows)
            {
                columnCount = 0;

                foreach (var rowCotent in row)
                {
                    if (columnCount == 0)
                    {
                        stringBuilder.Insert(index, "| ");

                        index += 2;
                    }
                    else
                    {
                        stringBuilder.Insert(index, " | ");

                        index += 3;
                    }

                    stringBuilder.Insert(index, rowCotent);
                    index += rowCotent.Length;

                    columnCount++;
                }

                stringBuilder.Insert(index, " |" + Environment.NewLine);
                index += 2 + Environment.NewLine.Length;
            }
        }
    }
}
