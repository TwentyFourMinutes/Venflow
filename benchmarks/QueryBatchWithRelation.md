``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.388 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.7.20366.6
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4084.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT


```
|                                 Method |           Job |       Runtime | QueryCount |      Mean |    Error |   StdDev | Ratio | RatioSD |      Gen 0 |     Gen 1 |    Gen 2 |   Allocated |
|--------------------------------------- |-------------- |-------------- |----------- |----------:|---------:|---------:|------:|--------:|-----------:|----------:|---------:|------------:|
|                  **EfCoreQueryBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |         **10** |  **12.35 ms** | **0.097 ms** | **0.081 ms** |  **1.00** |    **0.00** |    **15.6250** |         **-** |        **-** |     **60.5 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |         10 |  12.53 ms | 0.237 ms | 0.222 ms |  1.01 |    0.02 |    15.6250 |         - |        - |     76.5 KB |
|                 VenflowQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |         10 |  12.00 ms | 0.173 ms | 0.153 ms |  0.97 |    0.01 |          - |         - |        - |    30.75 KB |
| VenflowQueryBatchNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |         10 |  12.00 ms | 0.174 ms | 0.163 ms |  0.97 |    0.01 |          - |         - |        - |    30.38 KB |
|       RecommendedDapperQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |         10 |  12.05 ms | 0.109 ms | 0.097 ms |  0.98 |    0.01 |    15.6250 |         - |        - |    49.63 KB |
|            CustomDapperQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |         10 |  12.05 ms | 0.156 ms | 0.146 ms |  0.98 |    0.01 |    15.6250 |         - |        - |       49 KB |
|                                        |               |               |            |           |          |          |       |         |            |           |          |             |
|                  EfCoreQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |  12.24 ms | 0.168 ms | 0.149 ms |  1.00 |    0.00 |    15.6250 |         - |        - |    52.56 KB |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |  12.29 ms | 0.161 ms | 0.143 ms |  1.00 |    0.02 |    15.6250 |         - |        - |    67.23 KB |
|                 VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |  11.86 ms | 0.062 ms | 0.058 ms |  0.97 |    0.01 |          - |         - |        - |    24.45 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |  11.99 ms | 0.188 ms | 0.157 ms |  0.98 |    0.02 |          - |         - |        - |    24.14 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |  11.95 ms | 0.190 ms | 0.178 ms |  0.98 |    0.02 |          - |         - |        - |    42.77 KB |
|            CustomDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |  11.90 ms | 0.176 ms | 0.165 ms |  0.97 |    0.02 |          - |         - |        - |    42.17 KB |
|                                        |               |               |            |           |          |          |       |         |            |           |          |             |
|                  EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |  12.25 ms | 0.135 ms | 0.126 ms |  1.00 |    0.00 |          - |         - |        - |    39.19 KB |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |  12.20 ms | 0.093 ms | 0.082 ms |  0.99 |    0.01 |    15.6250 |         - |        - |    52.41 KB |
|                 VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |  11.89 ms | 0.107 ms | 0.100 ms |  0.97 |    0.01 |          - |         - |        - |    23.28 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |  11.87 ms | 0.107 ms | 0.090 ms |  0.97 |    0.01 |          - |         - |        - |    22.96 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |  11.91 ms | 0.148 ms | 0.124 ms |  0.97 |    0.01 |          - |         - |        - |    42.78 KB |
|            CustomDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |  11.89 ms | 0.145 ms | 0.136 ms |  0.97 |    0.01 |          - |         - |        - |    42.17 KB |
|                                        |               |               |            |           |          |          |       |         |            |           |          |             |
|                  **EfCoreQueryBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |        **100** |  **14.42 ms** | **0.119 ms** | **0.111 ms** |  **1.00** |    **0.00** |   **109.3750** |         **-** |        **-** |   **347.13 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |        100 |  15.07 ms | 0.087 ms | 0.082 ms |  1.04 |    0.01 |   156.2500 |   46.8750 |        - |   509.66 KB |
|                 VenflowQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |        100 |  12.75 ms | 0.096 ms | 0.085 ms |  0.88 |    0.01 |    46.8750 |   15.6250 |        - |    193.9 KB |
| VenflowQueryBatchNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |        100 |  12.70 ms | 0.085 ms | 0.066 ms |  0.88 |    0.01 |    46.8750 |   15.6250 |        - |   189.83 KB |
|       RecommendedDapperQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |        100 |  12.69 ms | 0.084 ms | 0.070 ms |  0.88 |    0.01 |   140.6250 |   46.8750 |        - |      501 KB |
|            CustomDapperQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |        100 |  12.66 ms | 0.089 ms | 0.083 ms |  0.88 |    0.01 |   140.6250 |   31.2500 |        - |   496.11 KB |
|                                        |               |               |            |           |          |          |       |         |            |           |          |             |
|                  EfCoreQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |  13.64 ms | 0.204 ms | 0.191 ms |  1.00 |    0.00 |    93.7500 |         - |        - |   307.02 KB |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |  13.96 ms | 0.189 ms | 0.168 ms |  1.02 |    0.02 |   140.6250 |   46.8750 |        - |   455.23 KB |
|                 VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |  12.56 ms | 0.077 ms | 0.069 ms |  0.92 |    0.01 |    31.2500 |         - |        - |   158.92 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |  12.58 ms | 0.102 ms | 0.090 ms |  0.92 |    0.02 |    46.8750 |   15.6250 |        - |   155.23 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |  12.56 ms | 0.146 ms | 0.122 ms |  0.92 |    0.02 |   140.6250 |   15.6250 |        - |   453.99 KB |
|            CustomDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |  12.53 ms | 0.127 ms | 0.119 ms |  0.92 |    0.02 |   125.0000 |   31.2500 |        - |   450.23 KB |
|                                        |               |               |            |           |          |          |       |         |            |           |          |             |
|                  EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |  13.42 ms | 0.104 ms | 0.093 ms |  1.00 |    0.00 |    93.7500 |         - |        - |   293.37 KB |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |  13.86 ms | 0.145 ms | 0.135 ms |  1.03 |    0.01 |   140.6250 |   46.8750 |        - |   439.84 KB |
|                 VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |  12.53 ms | 0.106 ms | 0.099 ms |  0.93 |    0.01 |    31.2500 |         - |        - |    158.1 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |  12.54 ms | 0.107 ms | 0.100 ms |  0.93 |    0.01 |    46.8750 |   15.6250 |        - |   154.37 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |  12.54 ms | 0.121 ms | 0.113 ms |  0.93 |    0.01 |   140.6250 |   15.6250 |        - |   454.01 KB |
|            CustomDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |  12.50 ms | 0.103 ms | 0.091 ms |  0.93 |    0.01 |   125.0000 |   31.2500 |        - |   450.23 KB |
|                                        |               |               |            |           |          |          |       |         |            |           |          |             |
|                  **EfCoreQueryBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |       **1000** |  **36.37 ms** | **0.572 ms** | **0.507 ms** |  **1.00** |    **0.00** |  **1071.4286** |         **-** |        **-** |  **3300.64 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |       1000 |  45.01 ms | 0.551 ms | 0.489 ms |  1.24 |    0.03 |   833.3333 |  250.0000 |        - |  5000.06 KB |
|                 VenflowQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |       1000 |  21.74 ms | 0.280 ms | 0.262 ms |  0.60 |    0.01 |   312.5000 |  156.2500 |  31.2500 |  1965.91 KB |
| VenflowQueryBatchNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |       1000 |  22.70 ms | 0.296 ms | 0.277 ms |  0.62 |    0.01 |   312.5000 |  125.0000 |  31.2500 |  1927.14 KB |
|       RecommendedDapperQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |       1000 |  28.64 ms | 0.460 ms | 0.430 ms |  0.79 |    0.01 |   937.5000 |  343.7500 |  93.7500 |  5299.88 KB |
|            CustomDapperQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |       1000 |  27.55 ms | 0.226 ms | 0.189 ms |  0.76 |    0.01 |   906.2500 |  312.5000 |  93.7500 |  5255.54 KB |
|                                        |               |               |            |           |          |          |       |         |            |           |          |             |
|                  EfCoreQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  29.29 ms | 0.280 ms | 0.262 ms |  1.00 |    0.00 |   937.5000 |         - |        - |  2933.62 KB |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  36.42 ms | 0.683 ms | 0.671 ms |  1.24 |    0.02 |   714.2857 |  285.7143 |        - |  4473.53 KB |
|                 VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  19.71 ms | 0.323 ms | 0.302 ms |  0.67 |    0.01 |   281.2500 |  125.0000 |  31.2500 |  1639.93 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  19.72 ms | 0.244 ms | 0.228 ms |  0.67 |    0.01 |   250.0000 |   93.7500 |  31.2500 |  1600.45 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  21.90 ms | 0.312 ms | 0.292 ms |  0.75 |    0.01 |   843.7500 |  312.5000 |  93.7500 |  4808.81 KB |
|            CustomDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  21.92 ms | 0.329 ms | 0.308 ms |  0.75 |    0.01 |   843.7500 |  343.7500 |  93.7500 |   4776.9 KB |
|                                        |               |               |            |           |          |          |       |         |            |           |          |             |
|                  EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  28.13 ms | 0.377 ms | 0.334 ms |  1.00 |    0.00 |   937.5000 |         - |        - |  2916.97 KB |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  33.99 ms | 0.338 ms | 0.282 ms |  1.21 |    0.02 |   666.6667 |  200.0000 |        - |  4456.01 KB |
|                 VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  19.60 ms | 0.144 ms | 0.128 ms |  0.70 |    0.01 |   250.0000 |  125.0000 |  31.2500 |   1642.8 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  19.81 ms | 0.155 ms | 0.145 ms |  0.70 |    0.01 |   250.0000 |  125.0000 |  31.2500 |  1603.12 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  21.04 ms | 0.242 ms | 0.214 ms |  0.75 |    0.01 |   812.5000 |  281.2500 |  62.5000 |  4808.81 KB |
|            CustomDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  21.21 ms | 0.400 ms | 0.374 ms |  0.75 |    0.01 |   781.2500 |  281.2500 |  62.5000 |  4776.76 KB |
|                                        |               |               |            |           |          |          |       |         |            |           |          |             |
|                  **EfCoreQueryBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |      **10000** | **234.36 ms** | **2.407 ms** | **2.010 ms** |  **1.00** |    **0.00** | **10333.3333** |         **-** |        **-** | **32854.02 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |      10000 | 330.81 ms | 6.112 ms | 5.717 ms |  1.42 |    0.03 |  8000.0000 | 3000.0000 |        - | 50080.62 KB |
|                 VenflowQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |      10000 | 128.71 ms | 1.829 ms | 1.621 ms |  0.55 |    0.01 |  2500.0000 |  750.0000 |        - | 21337.94 KB |
| VenflowQueryBatchNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |      10000 | 129.78 ms | 2.501 ms | 2.457 ms |  0.55 |    0.01 |  2500.0000 |  750.0000 |        - | 20939.63 KB |
|       RecommendedDapperQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |      10000 | 232.55 ms | 2.078 ms | 1.842 ms |  0.99 |    0.01 |  7666.6667 | 2666.6667 |        - | 55699.22 KB |
|            CustomDapperQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |      10000 | 226.07 ms | 3.631 ms | 3.219 ms |  0.97 |    0.02 |  7666.6667 | 2666.6667 |        - | 55059.14 KB |
|                                        |               |               |            |           |          |          |       |         |            |           |          |             |
|                  EfCoreQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 165.58 ms | 2.151 ms | 2.012 ms |  1.00 |    0.00 |  9500.0000 |         - |        - | 29270.45 KB |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 243.91 ms | 4.214 ms | 3.942 ms |  1.47 |    0.03 |  7333.3333 | 2000.0000 |        - |  45017.8 KB |
|                 VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 |  84.88 ms | 1.547 ms | 1.372 ms |  0.51 |    0.01 |  2166.6667 |  833.3333 | 166.6667 | 18265.39 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 |  89.75 ms | 1.228 ms | 1.089 ms |  0.54 |    0.01 |  2000.0000 |  833.3333 | 166.6667 | 17878.11 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 167.01 ms | 1.849 ms | 1.729 ms |  1.01 |    0.01 |  7000.0000 | 2750.0000 |        - | 50772.42 KB |
|            CustomDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 155.92 ms | 1.415 ms | 1.254 ms |  0.94 |    0.02 |  7000.0000 | 2750.0000 |        - |    50312 KB |
|                                        |               |               |            |           |          |          |       |         |            |           |          |             |
|                  EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 153.65 ms | 1.375 ms | 1.286 ms |  1.00 |    0.00 |  9500.0000 |         - |        - | 29253.81 KB |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 220.45 ms | 3.117 ms | 2.916 ms |  1.43 |    0.02 |  7000.0000 | 1333.3333 |        - | 45001.04 KB |
|                 VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 103.25 ms | 1.286 ms | 1.140 ms |  0.67 |    0.01 |  2142.8571 |  857.1429 | 285.7143 | 18261.66 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 |  82.83 ms | 1.513 ms | 1.415 ms |  0.54 |    0.01 |  2000.0000 |  833.3333 | 166.6667 | 17889.66 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 160.20 ms | 2.407 ms | 2.252 ms |  1.04 |    0.02 |  7000.0000 | 2750.0000 |        - | 50772.81 KB |
|            CustomDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 153.15 ms | 2.786 ms | 4.084 ms |  1.00 |    0.03 |  7000.0000 | 2750.0000 |        - |  50310.3 KB |
