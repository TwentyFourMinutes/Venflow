``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                                    Method |           Job |       Runtime |     Mean |    Error |   StdDev |   Median | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------------ |-------------- |-------------- |---------:|---------:|---------:|---------:|------:|--------:|-------:|------:|------:|----------:|
|                    EfCoreQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 479.0 μs |  9.49 μs | 17.36 μs | 479.1 μs |  1.00 |    0.00 |      - |     - |     - |   7.95 KB |
|    EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 491.7 μs |  9.76 μs | 23.20 μs | 491.1 μs |  1.03 |    0.06 |      - |     - |     - |   9.88 KB |
| EfCoreQuerySingleRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 615.6 μs | 11.41 μs | 22.79 μs | 612.7 μs |  1.29 |    0.06 |      - |     - |     - |  17.06 KB |
|                   VenflowQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 287.4 μs |  5.69 μs | 10.69 μs | 286.4 μs |  0.60 |    0.03 |      - |     - |     - |   3.07 KB |
|   VenflowQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 275.9 μs |  5.36 μs |  6.17 μs | 276.7 μs |  0.58 |    0.02 |      - |     - |     - |   3.02 KB |
|                    RepoDbQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 294.1 μs |  5.78 μs |  9.33 μs | 294.6 μs |  0.61 |    0.03 |      - |     - |     - |   4.38 KB |
|                    DapperQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 268.1 μs |  5.27 μs |  6.27 μs | 267.6 μs |  0.56 |    0.02 |      - |     - |     - |   2.66 KB |
|                                           |               |               |          |          |          |          |       |         |        |       |       |           |
|                    EfCoreQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 455.8 μs | 11.20 μs | 32.84 μs | 456.3 μs |  1.00 |    0.00 |      - |     - |     - |   7.29 KB |
|    EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 493.0 μs |  9.84 μs | 27.42 μs | 490.3 μs |  1.09 |    0.10 |      - |     - |     - |   8.93 KB |
| EfCoreQuerySingleRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 608.6 μs | 11.64 μs | 15.53 μs | 608.8 μs |  1.34 |    0.08 | 0.4883 |     - |     - |  13.28 KB |
|                   VenflowQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 288.8 μs | 10.83 μs | 31.77 μs | 272.5 μs |  0.64 |    0.08 |      - |     - |     - |   3.02 KB |
|   VenflowQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 282.6 μs |  5.58 μs | 13.57 μs | 281.8 μs |  0.64 |    0.04 |      - |     - |     - |   2.98 KB |
|                    RepoDbQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 298.0 μs |  5.87 μs | 14.50 μs | 295.6 μs |  0.67 |    0.06 |      - |     - |     - |    4.4 KB |
|                    DapperQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 279.1 μs |  5.57 μs | 12.47 μs | 278.8 μs |  0.63 |    0.05 |      - |     - |     - |   2.68 KB |
