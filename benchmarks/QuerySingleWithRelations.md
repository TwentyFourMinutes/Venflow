``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                                  Method |           Job |       Runtime |     Mean |     Error |    StdDev |   Median | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------------- |-------------- |-------------- |---------:|----------:|----------:|---------:|------:|--------:|------:|------:|------:|----------:|
|                  EfCoreQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 6.657 ms | 0.1278 ms | 0.1750 ms | 6.630 ms |  1.00 |    0.00 |     - |     - |     - |  29.52 KB |
|  EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 7.193 ms | 0.1203 ms | 0.1005 ms | 7.188 ms |  1.07 |    0.03 |     - |     - |     - |  33.65 KB |
|                 VenflowQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 6.281 ms | 0.1216 ms | 0.1137 ms | 6.261 ms |  0.94 |    0.03 |     - |     - |     - |   8.76 KB |
| VenflowQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 7.019 ms | 0.1353 ms | 0.1610 ms | 7.001 ms |  1.05 |    0.04 |     - |     - |     - |   8.72 KB |
|       RecommendedDapperQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 6.251 ms | 0.0808 ms | 0.0756 ms | 6.240 ms |  0.93 |    0.03 |     - |     - |     - |   7.77 KB |
|            CustomDapperQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 6.786 ms | 0.0563 ms | 0.0440 ms | 6.785 ms |  1.01 |    0.03 |     - |     - |     - |   7.25 KB |
|                                         |               |               |          |           |           |          |       |         |       |       |       |           |
|                  EfCoreQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 6.640 ms | 0.1147 ms | 0.1646 ms | 6.609 ms |  1.00 |    0.00 |     - |     - |     - |  16.49 KB |
|  EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 7.285 ms | 0.1407 ms | 0.1564 ms | 7.233 ms |  1.10 |    0.03 |     - |     - |     - |  22.59 KB |
|                 VenflowQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 6.282 ms | 0.0774 ms | 0.0951 ms | 6.281 ms |  0.95 |    0.03 |     - |     - |     - |   8.52 KB |
| VenflowQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 6.491 ms | 0.1285 ms | 0.3665 ms | 6.375 ms |  1.05 |    0.04 |     - |     - |     - |   8.48 KB |
|       RecommendedDapperQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 6.355 ms | 0.1020 ms | 0.0954 ms | 6.374 ms |  0.96 |    0.03 |     - |     - |     - |   7.79 KB |
|            CustomDapperQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 7.049 ms | 0.1110 ms | 0.1363 ms | 7.056 ms |  1.06 |    0.03 |     - |     - |     - |   7.25 KB |
