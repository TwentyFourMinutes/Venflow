``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                                  Method |           Job |       Runtime |     Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------------- |-------------- |-------------- |---------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                  EfCoreQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 7.300 ms | 0.1352 ms | 0.1265 ms |  1.00 |    0.00 |     - |     - |     - |  29.56 KB |
|  EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 7.335 ms | 0.1380 ms | 0.1290 ms |  1.01 |    0.02 |     - |     - |     - |  32.88 KB |
|                 VenflowQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 7.053 ms | 0.1166 ms | 0.0973 ms |  0.96 |    0.02 |     - |     - |     - |   8.88 KB |
| VenflowQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 6.696 ms | 0.1257 ms | 0.2101 ms |  0.93 |    0.04 |     - |     - |     - |   8.72 KB |
|       RecommendedDapperQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 7.057 ms | 0.1379 ms | 0.1354 ms |  0.97 |    0.03 |     - |     - |     - |   7.78 KB |
|            CustomDapperQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 6.462 ms | 0.1276 ms | 0.1310 ms |  0.88 |    0.02 |     - |     - |     - |   7.24 KB |
|                                         |               |               |          |           |           |       |         |       |       |       |           |
|                  EfCoreQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 7.346 ms | 0.1340 ms | 0.1188 ms |  1.00 |    0.00 |     - |     - |     - |  16.38 KB |
|  EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 6.856 ms | 0.1337 ms | 0.1642 ms |  0.93 |    0.03 |     - |     - |     - |  23.46 KB |
|                 VenflowQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 7.049 ms | 0.1143 ms | 0.1014 ms |  0.96 |    0.02 |     - |     - |     - |   8.52 KB |
| VenflowQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 6.494 ms | 0.1255 ms | 0.1395 ms |  0.89 |    0.03 |     - |     - |     - |   8.48 KB |
|       RecommendedDapperQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 7.071 ms | 0.1116 ms | 0.1044 ms |  0.96 |    0.02 |     - |     - |     - |   7.79 KB |
|            CustomDapperQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 6.479 ms | 0.1294 ms | 0.1081 ms |  0.88 |    0.02 |     - |     - |     - |   7.25 KB |
