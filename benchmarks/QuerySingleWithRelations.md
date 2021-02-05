``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                                  Method |           Job |       Runtime |     Mean |     Error |    StdDev |   Median | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------------- |-------------- |-------------- |---------:|----------:|----------:|---------:|------:|--------:|------:|------:|------:|----------:|
|                  EfCoreQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 7.353 ms | 0.1433 ms | 0.1864 ms | 7.380 ms |  1.00 |    0.00 |     - |     - |     - |  29.68 KB |
|  EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 6.781 ms | 0.1329 ms | 0.1906 ms | 6.755 ms |  0.92 |    0.03 |     - |     - |     - |  32.98 KB |
|                 VenflowQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 5.925 ms | 0.1411 ms | 0.4161 ms | 5.699 ms |  0.88 |    0.04 |     - |     - |     - |   8.87 KB |
| VenflowQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 5.629 ms | 0.1018 ms | 0.1524 ms | 5.570 ms |  0.77 |    0.03 |     - |     - |     - |   8.83 KB |
|       RecommendedDapperQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 6.047 ms | 0.1190 ms | 0.1113 ms | 6.054 ms |  0.83 |    0.02 |     - |     - |     - |   7.77 KB |
|            CustomDapperQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 5.625 ms | 0.1043 ms | 0.1624 ms | 5.558 ms |  0.77 |    0.03 |     - |     - |     - |   7.23 KB |
|                                         |               |               |          |           |           |          |       |         |       |       |       |           |
|                  EfCoreQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 6.281 ms | 0.0994 ms | 0.0882 ms | 6.315 ms |  1.00 |    0.00 |     - |     - |     - |  16.48 KB |
|  EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 5.936 ms | 0.1183 ms | 0.1876 ms | 5.879 ms |  0.96 |    0.04 |     - |     - |     - |  23.37 KB |
|                 VenflowQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 6.116 ms | 0.0961 ms | 0.0802 ms | 6.124 ms |  0.97 |    0.02 |     - |     - |     - |   8.52 KB |
| VenflowQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 5.592 ms | 0.1067 ms | 0.1228 ms | 5.580 ms |  0.89 |    0.02 |     - |     - |     - |   8.48 KB |
|       RecommendedDapperQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 5.977 ms | 0.0513 ms | 0.0455 ms | 5.967 ms |  0.95 |    0.02 |     - |     - |     - |   7.79 KB |
|            CustomDapperQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 5.714 ms | 0.1141 ms | 0.2382 ms | 5.620 ms |  0.96 |    0.03 |     - |     - |     - |   7.25 KB |
