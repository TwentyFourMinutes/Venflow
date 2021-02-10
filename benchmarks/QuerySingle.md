``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                                    Method |           Job |       Runtime |     Mean |    Error |   StdDev |   Median | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------------ |-------------- |-------------- |---------:|---------:|---------:|---------:|------:|--------:|-------:|------:|------:|----------:|
|                    EfCoreQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 318.7 μs | 13.15 μs | 38.77 μs | 296.4 μs |  1.00 |    0.00 |      - |     - |     - |   7.97 KB |
|    EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 383.9 μs |  7.50 μs | 13.71 μs | 383.6 μs |  1.20 |    0.15 | 0.4883 |     - |     - |  10.25 KB |
| EfCoreQuerySingleRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 469.7 μs |  5.71 μs |  5.06 μs | 470.2 μs |  1.50 |    0.14 | 0.4883 |     - |     - |  17.23 KB |
|                   VenflowQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 203.2 μs |  4.24 μs | 12.49 μs | 202.4 μs |  0.65 |    0.09 |      - |     - |     - |   3.07 KB |
|   VenflowQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 209.2 μs |  4.16 μs | 11.39 μs | 209.0 μs |  0.66 |    0.08 |      - |     - |     - |   3.03 KB |
|                    RepoDbQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 221.2 μs |  4.42 μs | 11.56 μs | 220.3 μs |  0.69 |    0.09 |      - |     - |     - |   4.38 KB |
|                    DapperQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 204.3 μs |  4.07 μs | 11.56 μs | 201.7 μs |  0.64 |    0.08 |      - |     - |     - |   2.67 KB |
|                                           |               |               |          |          |          |          |       |         |        |       |       |           |
|                    EfCoreQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 325.8 μs |  8.17 μs | 24.10 μs | 328.2 μs |  1.00 |    0.00 |      - |     - |     - |    7.3 KB |
|    EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 343.1 μs |  8.69 μs | 25.63 μs | 342.3 μs |  1.06 |    0.12 |      - |     - |     - |   8.62 KB |
| EfCoreQuerySingleRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 441.0 μs |  8.53 μs | 12.23 μs | 441.8 μs |  1.34 |    0.11 | 0.4883 |     - |     - |  12.76 KB |
|                   VenflowQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 200.6 μs |  5.51 μs | 16.23 μs | 199.9 μs |  0.62 |    0.07 |      - |     - |     - |   2.96 KB |
|   VenflowQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 202.6 μs |  5.67 μs | 16.71 μs | 203.3 μs |  0.63 |    0.07 |      - |     - |     - |   2.62 KB |
|                    RepoDbQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 213.7 μs |  4.25 μs | 11.43 μs | 213.1 μs |  0.66 |    0.06 |      - |     - |     - |   4.28 KB |
|                    DapperQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 196.1 μs |  4.29 μs | 12.64 μs | 195.7 μs |  0.61 |    0.06 |      - |     - |     - |   2.49 KB |
