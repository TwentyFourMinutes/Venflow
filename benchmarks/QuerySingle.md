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
|                    EfCoreQuerySingleAsync | 399.7 μs |  7.97 μs | 22.74 μs |  1.00 |    0.00 |     - |     - |     - |      5 KB |
|    EfCoreQuerySingleNoChangeTrackingAsync | 408.1 μs | 10.26 μs | 29.60 μs |  1.02 |    0.09 |     - |     - |     - |      6 KB |
| EfCoreQuerySingleRawNoChangeTrackingAsync | 532.4 μs | 11.08 μs | 32.33 μs |  1.33 |    0.10 |     - |     - |     - |     12 KB |
|                   VenflowQuerySingleAsync | 260.0 μs |  6.04 μs | 17.71 μs |  0.65 |    0.06 |     - |     - |     - |      2 KB |
|   VenflowQuerySingleNoChangeTrackingAsync | 263.5 μs |  6.25 μs | 18.13 μs |  0.66 |    0.06 |     - |     - |     - |      2 KB |
|                    RepoDbQuerySingleAsync | 284.6 μs |  5.69 μs | 12.12 μs |  0.72 |    0.05 |     - |     - |     - |      3 KB |
|                    DapperQuerySingleAsync | 262.6 μs |  7.18 μs | 21.06 μs |  0.66 |    0.06 |     - |     - |     - |      1 KB |
