``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.508 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-rc.1.20452.10
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4220.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT


```
|                                  Method |           Job |       Runtime |     Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------------- |-------------- |-------------- |---------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                  EfCoreQuerySingleAsync |      .NET 4.8 |      .NET 4.8 | 6.506 ms | 0.1265 ms | 0.1184 ms |  1.00 |    0.00 | 7.8125 |     - |     - |  35.88 KB |
|  EfCoreQuerySingleNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 | 6.495 ms | 0.1055 ms | 0.0987 ms |  1.00 |    0.03 | 7.8125 |     - |     - |  40.75 KB |
|                 VenflowQuerySingleAsync |      .NET 4.8 |      .NET 4.8 | 6.195 ms | 0.1197 ms | 0.1061 ms |  0.95 |    0.02 |      - |     - |     - |  17.13 KB |
| VenflowQuerySingleNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 | 6.271 ms | 0.1161 ms | 0.1086 ms |  0.96 |    0.02 |      - |     - |     - |     17 KB |
|       RecommendedDapperQuerySingleAsync |      .NET 4.8 |      .NET 4.8 | 6.213 ms | 0.1192 ms | 0.1325 ms |  0.96 |    0.03 |      - |     - |     - |  12.69 KB |
|            CustomDapperQuerySingleAsync |      .NET 4.8 |      .NET 4.8 | 6.222 ms | 0.1237 ms | 0.1424 ms |  0.96 |    0.03 |      - |     - |     - |  12.19 KB |
|                                         |               |               |          |           |           |       |         |        |       |       |           |
|                  EfCoreQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 6.392 ms | 0.1184 ms | 0.1108 ms |  1.00 |    0.00 | 7.8125 |     - |     - |  28.39 KB |
|  EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 6.398 ms | 0.1216 ms | 0.1137 ms |  1.00 |    0.03 | 7.8125 |     - |     - |  32.52 KB |
|                 VenflowQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 6.181 ms | 0.1173 ms | 0.1097 ms |  0.97 |    0.03 |      - |     - |     - |  12.55 KB |
| VenflowQuerySingleNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 | 6.155 ms | 0.1063 ms | 0.0994 ms |  0.96 |    0.02 |      - |     - |     - |  12.51 KB |
|       RecommendedDapperQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 6.127 ms | 0.0824 ms | 0.0731 ms |  0.96 |    0.02 |      - |     - |     - |   9.12 KB |
|            CustomDapperQuerySingleAsync | .NET Core 3.1 | .NET Core 3.1 | 6.119 ms | 0.0938 ms | 0.0831 ms |  0.96 |    0.02 |      - |     - |     - |   8.59 KB |
|                                         |               |               |          |           |           |       |         |        |       |       |           |
|                  EfCoreQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 6.374 ms | 0.0924 ms | 0.0864 ms |  1.00 |    0.00 |      - |     - |     - |  15.49 KB |
|  EfCoreQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 6.422 ms | 0.0841 ms | 0.0702 ms |  1.01 |    0.02 |      - |     - |     - |  21.81 KB |
|                 VenflowQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 6.200 ms | 0.1225 ms | 0.1146 ms |  0.97 |    0.03 |      - |     - |     - |  11.11 KB |
| VenflowQuerySingleNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 | 6.223 ms | 0.1109 ms | 0.1277 ms |  0.98 |    0.02 |      - |     - |     - |  11.07 KB |
|       RecommendedDapperQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 6.530 ms | 0.1305 ms | 0.2999 ms |  0.99 |    0.04 |      - |     - |     - |   9.13 KB |
|            CustomDapperQuerySingleAsync | .NET Core 5.0 | .NET Core 5.0 | 6.525 ms | 0.1270 ms | 0.1560 ms |  1.02 |    0.02 |      - |     - |     - |   8.59 KB |
