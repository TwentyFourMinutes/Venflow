``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.508 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-rc.1.20452.10
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4220.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT


```
|                                    Method |           Job |       Runtime |     Mean |   Error |   StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------------ |-------------- |-------------- |---------:|--------:|---------:|------:|--------:|-------:|------:|------:|----------:|
|                    EfCoreQuerySingleAsync |      .NET 4.8 |      .NET 4.8 | 303.8 μs | 5.62 μs | 12.46 μs |  1.00 |    0.00 | 4.3945 |     - |     - |  14.46 KB |
|    EfCoreQuerySingleNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 | 314.2 μs | 6.24 μs | 12.60 μs |  1.03 |    0.06 | 4.8828 |     - |     - |  16.45 KB |
| EfCoreQuerySingleRawNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 | 430.2 μs | 8.44 μs | 14.56 μs |  1.40 |    0.08 | 7.8125 |     - |     - |  24.08 KB |
|                   VenflowQuerySingleAsync |      .NET 4.8 |      .NET 4.8 | 198.5 μs | 3.93 μs |  5.89 μs |  0.65 |    0.04 | 2.1973 |     - |     - |   7.27 KB |
|   VenflowQuerySingleNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 | 206.6 μs | 4.11 μs |  6.16 μs |  0.67 |    0.03 | 2.1973 |     - |     - |   7.23 KB |
|                    RepoDbQuerySingleAsync |      .NET 4.8 |      .NET 4.8 | 221.9 μs | 3.56 μs |  3.16 μs |  0.74 |    0.02 | 2.1973 |     - |     - |   7.23 KB |
|                    DapperQuerySingleAsync |      .NET 4.8 |      .NET 4.8 | 189.0 μs | 3.44 μs |  4.36 μs |  0.62 |    0.03 | 1.7090 |     - |     - |    5.6 KB |
|                                           |               |               |          |         |          |       |         |        |       |       |           |
|                    EfCoreQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 264.5 μs | 5.27 μs |  9.64 μs |  1.00 |    0.00 | 2.4414 |     - |     - |   7.74 KB |
|    EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 268.6 μs | 5.22 μs |  9.15 μs |  1.02 |    0.04 | 2.9297 |     - |     - |   9.68 KB |
| EfCoreQuerySingleRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 366.6 μs | 7.19 μs | 12.21 μs |  1.38 |    0.06 | 5.3711 |     - |     - |  16.63 KB |
|                   VenflowQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 169.0 μs | 3.38 μs |  5.06 μs |  0.64 |    0.03 | 1.2207 |     - |     - |   3.92 KB |
|   VenflowQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 168.5 μs | 3.36 μs |  3.73 μs |  0.63 |    0.03 | 1.2207 |     - |     - |   3.88 KB |
|                    RepoDbQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 219.4 μs | 4.14 μs |  4.07 μs |  0.83 |    0.04 | 1.2207 |     - |     - |   3.74 KB |
|                    DapperQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 160.5 μs | 3.17 μs |  2.96 μs |  0.60 |    0.02 | 0.7324 |     - |     - |   2.54 KB |
|                                           |               |               |          |         |          |       |         |        |       |       |           |
|                    EfCoreQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 259.3 μs | 5.17 μs | 13.70 μs |  1.00 |    0.00 | 1.9531 |     - |     - |   7.08 KB |
|    EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 275.8 μs | 5.44 μs | 10.99 μs |  1.08 |    0.07 | 2.9297 |     - |     - |   9.07 KB |
| EfCoreQuerySingleRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 330.5 μs | 6.58 μs | 15.63 μs |  1.28 |    0.08 | 3.9063 |     - |     - |  13.09 KB |
|                   VenflowQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 177.1 μs | 3.27 μs |  4.79 μs |  0.68 |    0.03 | 1.2207 |     - |     - |   3.87 KB |
|   VenflowQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 173.7 μs | 3.47 μs |  7.54 μs |  0.68 |    0.04 | 1.2207 |     - |     - |   3.83 KB |
|                    RepoDbQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 216.7 μs | 4.28 μs |  8.75 μs |  0.85 |    0.06 | 1.2207 |     - |     - |   3.73 KB |
|                    DapperQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 173.0 μs | 3.45 μs |  6.14 μs |  0.67 |    0.05 | 0.7324 |     - |     - |   2.52 KB |
