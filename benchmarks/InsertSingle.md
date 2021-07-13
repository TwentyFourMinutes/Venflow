``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                   Method |       Mean |    Error |   StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-----------:|---------:|---------:|------:|--------:|------:|------:|------:|----------:|
|  EFCoreInsertSingleAsync | 1,125.6 μs | 21.79 μs | 24.22 μs |  1.00 |    0.00 |     - |     - |     - |     16 KB |
| VenflowInsertSingleAsync |   879.4 μs | 17.29 μs | 30.74 μs |  0.78 |    0.03 |     - |     - |     - |      4 KB |
|  RepoDbInsertSingleAsync |   678.3 μs | 13.41 μs | 26.78 μs |  0.60 |    0.03 |     - |     - |     - |      3 KB |
