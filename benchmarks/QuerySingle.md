``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.7.21379.14
  [Host]   : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                                    Method |     Mean |    Error |   StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------------ |---------:|---------:|---------:|------:|--------:|------:|------:|------:|----------:|
|                    EfCoreQuerySingleAsync | 376.0 μs |  7.43 μs | 20.48 μs |  1.00 |    0.00 |     - |     - |     - |      5 KB |
|    EfCoreQuerySingleNoChangeTrackingAsync | 390.8 μs |  8.56 μs | 25.25 μs |  1.05 |    0.09 |     - |     - |     - |      6 KB |
| EfCoreQuerySingleRawNoChangeTrackingAsync | 495.5 μs | 11.07 μs | 32.63 μs |  1.32 |    0.11 |     - |     - |     - |     11 KB |
|                   VenflowQuerySingleAsync | 243.6 μs |  8.44 μs | 24.90 μs |  0.66 |    0.08 |     - |     - |     - |      2 KB |
|   VenflowQuerySingleNoChangeTrackingAsync | 239.7 μs |  7.90 μs | 23.30 μs |  0.64 |    0.08 |     - |     - |     - |      2 KB |
|                    RepoDbQuerySingleAsync | 280.1 μs |  5.58 μs | 12.13 μs |  0.75 |    0.04 |     - |     - |     - |      3 KB |
|                    DapperQuerySingleAsync | 242.5 μs |  6.73 μs | 19.84 μs |  0.64 |    0.07 |     - |     - |     - |      1 KB |
