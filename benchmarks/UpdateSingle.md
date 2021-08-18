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
|  EFCoreUpdateSingleAsync | 1,022.1 μs | 20.24 μs | 42.25 μs |  1.00 |    0.00 |     - |     - |     - |     13 KB |
| VenflowUpdateSingleAsync |   815.8 μs | 15.98 μs | 31.55 μs |  0.80 |    0.04 |     - |     - |     - |      4 KB |
|  RepoDbUpdateSingleAsync |   671.5 μs | 13.37 μs | 37.50 μs |  0.66 |    0.05 |     - |     - |     - |      7 KB |
