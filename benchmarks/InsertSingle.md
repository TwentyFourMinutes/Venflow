``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.7.21379.14
  [Host]   : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                   Method |       Mean |    Error |   StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-----------:|---------:|---------:|------:|--------:|------:|------:|------:|----------:|
|  EFCoreInsertSingleAsync | 1,051.5 μs | 20.71 μs | 37.35 μs |  1.00 |    0.00 |     - |     - |     - |     16 KB |
| VenflowInsertSingleAsync |   801.8 μs | 15.86 μs | 16.29 μs |  0.77 |    0.03 |     - |     - |     - |      4 KB |
|  RepoDbInsertSingleAsync |   593.7 μs | 11.77 μs | 26.09 μs |  0.56 |    0.04 |     - |     - |     - |      3 KB |
