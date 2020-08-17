``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.388 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.7.20366.6
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4084.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT


```
|                                    Method |           Job |       Runtime |     Mean |   Error |   StdDev |   Median | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------------ |-------------- |-------------- |---------:|--------:|---------:|---------:|------:|--------:|-------:|------:|------:|----------:|
|                    EfCoreQuerySingleAsync |      .NET 4.8 |      .NET 4.8 | 309.7 μs | 6.12 μs | 12.77 μs | 312.0 μs |  1.00 |    0.00 | 4.3945 |     - |     - |  14.45 KB |
|    EfCoreQuerySingleNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 | 332.6 μs | 3.65 μs |  3.05 μs | 333.3 μs |  1.06 |    0.04 | 5.3711 |     - |     - |  17.02 KB |
| EfCoreQuerySingleRawNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 | 429.3 μs | 8.33 μs |  8.91 μs | 429.8 μs |  1.37 |    0.06 | 7.8125 |     - |     - |  24.26 KB |
|                   VenflowQuerySingleAsync |      .NET 4.8 |      .NET 4.8 | 194.1 μs | 3.33 μs |  3.11 μs | 194.4 μs |  0.62 |    0.02 | 2.1973 |     - |     - |   7.35 KB |
|   VenflowQuerySingleNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 | 194.0 μs | 3.80 μs |  3.56 μs | 194.6 μs |  0.62 |    0.03 | 2.1973 |     - |     - |    7.3 KB |
|                    RepoDbQuerySingleAsync |      .NET 4.8 |      .NET 4.8 | 218.9 μs | 2.99 μs |  2.65 μs | 219.6 μs |  0.70 |    0.03 | 2.1973 |     - |     - |   6.82 KB |
|                    DapperQuerySingleAsync |      .NET 4.8 |      .NET 4.8 | 189.4 μs | 3.49 μs |  3.43 μs | 189.2 μs |  0.60 |    0.03 | 1.7090 |     - |     - |    5.6 KB |
|                                           |               |               |          |         |          |          |       |         |        |       |       |           |
|                    EfCoreQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 255.8 μs | 5.05 μs |  9.60 μs | 258.7 μs |  1.00 |    0.00 | 2.4414 |     - |     - |   7.92 KB |
|    EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 282.6 μs | 6.60 μs | 19.45 μs | 286.7 μs |  1.06 |    0.10 | 2.9297 |     - |     - |   9.85 KB |
| EfCoreQuerySingleRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 360.0 μs | 7.10 μs | 11.05 μs | 361.8 μs |  1.41 |    0.07 | 5.3711 |     - |     - |   16.8 KB |
|                   VenflowQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 167.6 μs | 3.26 μs |  5.63 μs | 165.9 μs |  0.66 |    0.03 | 1.2207 |     - |     - |   3.99 KB |
|   VenflowQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 165.0 μs | 3.21 μs |  3.70 μs | 164.3 μs |  0.64 |    0.03 | 1.2207 |     - |     - |   3.95 KB |
|                    RepoDbQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 214.1 μs | 2.41 μs |  2.25 μs | 214.6 μs |  0.83 |    0.02 | 0.9766 |     - |     - |   3.52 KB |
|                    DapperQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 159.6 μs | 2.58 μs |  2.29 μs | 159.2 μs |  0.62 |    0.02 | 0.7324 |     - |     - |   2.54 KB |
|                                           |               |               |          |         |          |          |       |         |        |       |       |           |
|                    EfCoreQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 270.6 μs | 5.40 μs | 14.41 μs | 273.5 μs |  1.00 |    0.00 | 1.9531 |     - |     - |   7.38 KB |
|    EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 265.4 μs | 5.27 μs | 10.89 μs | 265.9 μs |  1.01 |    0.06 | 2.9297 |     - |     - |   9.02 KB |
| EfCoreQuerySingleRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 350.7 μs | 6.98 μs |  7.17 μs | 352.5 μs |  1.40 |    0.07 | 3.9063 |     - |     - |   13.4 KB |
|                   VenflowQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 166.3 μs | 3.30 μs |  5.86 μs | 163.7 μs |  0.64 |    0.05 | 1.2207 |     - |     - |   3.94 KB |
|   VenflowQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 161.9 μs | 2.29 μs |  2.14 μs | 161.7 μs |  0.65 |    0.04 | 1.2207 |     - |     - |    3.9 KB |
|                    RepoDbQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 205.7 μs | 3.67 μs |  4.08 μs | 205.1 μs |  0.82 |    0.05 | 0.9766 |     - |     - |   3.51 KB |
|                    DapperQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 168.4 μs | 3.31 μs |  3.94 μs | 168.6 μs |  0.67 |    0.04 | 0.7324 |     - |     - |   2.52 KB |
