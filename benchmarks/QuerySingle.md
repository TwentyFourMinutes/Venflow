``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                                    Method |     Mean |    Error |   StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------------ |---------:|---------:|---------:|------:|--------:|------:|------:|------:|----------:|
|                    EfCoreQuerySingleAsync | 403.3 μs | 12.02 μs | 35.24 μs |  1.00 |    0.00 |     - |     - |     - |      5 KB |
|    EfCoreQuerySingleNoChangeTrackingAsync | 430.5 μs | 11.90 μs | 35.09 μs |  1.08 |    0.14 |     - |     - |     - |      7 KB |
| EfCoreQuerySingleRawNoChangeTrackingAsync | 541.4 μs | 13.86 μs | 40.87 μs |  1.35 |    0.15 |     - |     - |     - |     11 KB |
|                   VenflowQuerySingleAsync | 256.7 μs |  6.38 μs | 18.81 μs |  0.64 |    0.07 |     - |     - |     - |      2 KB |
|   VenflowQuerySingleNoChangeTrackingAsync | 260.8 μs |  6.51 μs | 19.09 μs |  0.65 |    0.07 |     - |     - |     - |      2 KB |
|                    RepoDbQuerySingleAsync | 281.9 μs |  5.61 μs | 13.44 μs |  0.71 |    0.07 |     - |     - |     - |      3 KB |
|                    DapperQuerySingleAsync | 254.9 μs |  5.86 μs | 17.20 μs |  0.64 |    0.08 |     - |     - |     - |      1 KB |
