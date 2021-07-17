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
|  EFCoreInsertSingleAsync | 1,110.5 μs | 21.87 μs | 47.53 μs |  1.00 |    0.00 |     - |     - |     - |     16 KB |
| VenflowInsertSingleAsync |   835.6 μs | 16.40 μs | 21.89 μs |  0.76 |    0.03 |     - |     - |     - |      4 KB |
|  RepoDbInsertSingleAsync |   646.3 μs | 12.92 μs | 36.86 μs |  0.59 |    0.05 |     - |     - |     - |      3 KB |
