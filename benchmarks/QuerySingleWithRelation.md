``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.388 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.7.20366.6
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4084.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT


```
|                                  Method |           Job |       Runtime |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------------- |-------------- |-------------- |----------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                  EfCoreQuerySingleAsync |      .NET 4.8 |      .NET 4.8 | 12.071 ms | 0.0861 ms | 0.0719 ms |  1.00 |    0.00 |     - |     - |     - |   33.5 KB |
|  EfCoreQuerySingleNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 | 12.091 ms | 0.1123 ms | 0.0996 ms |  1.00 |    0.01 |     - |     - |     - |  35.75 KB |
|                 VenflowQuerySingleAsync |      .NET 4.8 |      .NET 4.8 |  4.181 ms | 0.0288 ms | 0.0240 ms |  0.35 |    0.00 |     - |     - |     - |     16 KB |
| VenflowQuerySingleNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |  4.218 ms | 0.0527 ms | 0.0467 ms |  0.35 |    0.00 |     - |     - |     - |     16 KB |
|       RecommendedDapperQuerySingleAsync |      .NET 4.8 |      .NET 4.8 |  4.148 ms | 0.0171 ms | 0.0152 ms |  0.34 |    0.00 |     - |     - |     - |   7.44 KB |
|            CustomDapperQuerySingleAsync |      .NET 4.8 |      .NET 4.8 |  4.175 ms | 0.0803 ms | 0.0893 ms |  0.35 |    0.01 |     - |     - |     - |      7 KB |
|                                         |               |               |           |           |           |       |         |       |       |       |           |
|                  EfCoreQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 12.054 ms | 0.1632 ms | 0.1526 ms |  1.00 |    0.00 |     - |     - |     - |  26.57 KB |
|  EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 11.970 ms | 0.1325 ms | 0.1240 ms |  0.99 |    0.02 |     - |     - |     - |  27.67 KB |
|                 VenflowQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 |  4.149 ms | 0.0389 ms | 0.0325 ms |  0.34 |    0.00 |     - |     - |     - |  11.19 KB |
| VenflowQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |  4.141 ms | 0.0353 ms | 0.0295 ms |  0.34 |    0.01 |     - |     - |     - |  11.17 KB |
|       RecommendedDapperQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 |  4.115 ms | 0.0362 ms | 0.0321 ms |  0.34 |    0.01 |     - |     - |     - |   4.02 KB |
|            CustomDapperQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 |  4.095 ms | 0.0431 ms | 0.0382 ms |  0.34 |    0.01 |     - |     - |     - |   3.87 KB |
|                                         |               |               |           |           |           |       |         |       |       |       |           |
|                  EfCoreQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 12.005 ms | 0.1279 ms | 0.1196 ms |  1.00 |    0.00 |     - |     - |     - |  14.13 KB |
|  EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 12.000 ms | 0.1029 ms | 0.0912 ms |  1.00 |    0.01 |     - |     - |     - |  15.48 KB |
|                 VenflowQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 |  4.135 ms | 0.0275 ms | 0.0244 ms |  0.34 |    0.00 |     - |     - |     - |   9.99 KB |
| VenflowQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |  4.134 ms | 0.0635 ms | 0.0563 ms |  0.34 |    0.00 |     - |     - |     - |   9.99 KB |
|       RecommendedDapperQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 |  4.125 ms | 0.0605 ms | 0.0566 ms |  0.34 |    0.01 |     - |     - |     - |   4.04 KB |
|            CustomDapperQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 |  4.126 ms | 0.0436 ms | 0.0408 ms |  0.34 |    0.00 |     - |     - |     - |   3.86 KB |
