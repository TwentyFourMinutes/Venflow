``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                                    Method |           Job |       Runtime |     Mean |   Error |   StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------------ |-------------- |-------------- |---------:|--------:|---------:|------:|--------:|-------:|------:|------:|----------:|
|                    EfCoreQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 387.0 μs | 7.68 μs | 21.28 μs |  1.00 |    0.00 |      - |     - |     - |   7.96 KB |
|    EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 415.1 μs | 8.25 μs | 18.11 μs |  1.07 |    0.07 | 0.4883 |     - |     - |  10.41 KB |
| EfCoreQuerySingleRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 498.4 μs | 9.72 μs | 12.30 μs |  1.28 |    0.08 |      - |     - |     - |  17.75 KB |
|                   VenflowQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 228.1 μs | 4.50 μs |  7.52 μs |  0.59 |    0.04 |      - |     - |     - |   3.07 KB |
|   VenflowQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 225.5 μs | 4.40 μs |  6.03 μs |  0.58 |    0.04 |      - |     - |     - |   3.02 KB |
|                    RepoDbQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 245.9 μs | 4.88 μs | 11.50 μs |  0.63 |    0.04 |      - |     - |     - |   3.54 KB |
|                    DapperQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 225.9 μs | 4.29 μs |  8.15 μs |  0.58 |    0.03 |      - |     - |     - |   2.69 KB |
|                                           |               |               |          |         |          |       |         |        |       |       |           |
|                    EfCoreQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 395.3 μs | 8.82 μs | 26.01 μs |  1.00 |    0.00 |      - |     - |     - |    7.3 KB |
|    EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 432.1 μs | 7.80 μs |  7.30 μs |  1.05 |    0.06 | 0.4883 |     - |     - |  10.14 KB |
| EfCoreQuerySingleRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 479.8 μs | 8.92 μs | 13.07 μs |  1.19 |    0.09 |      - |     - |     - |  12.77 KB |
|                   VenflowQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 224.7 μs | 4.49 μs |  9.67 μs |  0.56 |    0.05 |      - |     - |     - |   3.02 KB |
|   VenflowQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 223.4 μs | 4.37 μs | 11.12 μs |  0.56 |    0.04 |      - |     - |     - |   2.84 KB |
|                    RepoDbQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 248.6 μs | 4.93 μs | 13.51 μs |  0.63 |    0.06 |      - |     - |     - |   4.41 KB |
|                    DapperQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 223.1 μs | 4.44 μs |  9.36 μs |  0.56 |    0.04 |      - |     - |     - |   2.66 KB |
