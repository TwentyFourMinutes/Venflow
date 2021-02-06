``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                                 Method |           Job |       Runtime | QueryCount |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |     Gen 0 |     Gen 1 |    Gen 2 |   Allocated |
|--------------------------------------- |-------------- |-------------- |----------- |-----------:|----------:|----------:|-----------:|------:|--------:|----------:|----------:|---------:|------------:|
|                  **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |         **10** |   **6.942 ms** | **0.1324 ms** | **0.3041 ms** |   **6.816 ms** |  **1.00** |    **0.00** |         **-** |         **-** |        **-** |    **58.65 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |   7.516 ms | 0.1449 ms | 0.1833 ms |   7.524 ms |  1.04 |    0.07 |         - |         - |        - |    75.94 KB |
|                 VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |   6.375 ms | 0.0613 ms | 0.0819 ms |   6.368 ms |  0.89 |    0.05 |         - |         - |        - |    33.13 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |   6.958 ms | 0.1080 ms | 0.0958 ms |   6.941 ms |  0.94 |    0.02 |         - |         - |        - |    32.77 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |   6.450 ms | 0.0894 ms | 0.0837 ms |   6.460 ms |  0.87 |    0.02 |         - |         - |        - |    35.83 KB |
|            CustomDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |   7.010 ms | 0.1043 ms | 0.0924 ms |   7.009 ms |  0.94 |    0.02 |         - |         - |        - |    35.35 KB |
|                                        |               |               |            |            |           |           |            |       |         |           |           |          |             |
|                  EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |   6.977 ms | 0.1349 ms | 0.3383 ms |   6.827 ms |  1.00 |    0.00 |         - |         - |        - |    43.91 KB |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |   7.519 ms | 0.1341 ms | 0.1254 ms |   7.460 ms |  1.00 |    0.02 |         - |         - |        - |   101.78 KB |
|                 VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |   6.361 ms | 0.0780 ms | 0.0868 ms |   6.347 ms |  0.86 |    0.04 |         - |         - |        - |    33.11 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |   6.922 ms | 0.1043 ms | 0.0924 ms |   6.925 ms |  0.93 |    0.02 |         - |         - |        - |    32.72 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |   6.373 ms | 0.1126 ms | 0.1106 ms |   6.322 ms |  0.85 |    0.02 |         - |         - |        - |    35.85 KB |
|            CustomDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |   7.242 ms | 0.1428 ms | 0.2093 ms |   7.274 ms |  1.01 |    0.07 |         - |         - |        - |    35.37 KB |
|                                        |               |               |            |            |           |           |            |       |         |           |           |          |             |
|                  **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |        **100** |   **8.915 ms** | **0.1137 ms** | **0.1063 ms** |   **8.922 ms** |  **1.00** |    **0.00** |   **15.6250** |         **-** |        **-** |   **321.44 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |  10.149 ms | 0.1952 ms | 0.2397 ms |  10.177 ms |  1.13 |    0.02 |   15.6250 |         - |        - |   477.95 KB |
|                 VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |   7.843 ms | 0.0688 ms | 0.0537 ms |   7.842 ms |  0.88 |    0.01 |         - |         - |        - |   252.06 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |   7.044 ms | 0.1310 ms | 0.1225 ms |   7.070 ms |  0.79 |    0.01 |         - |         - |        - |   248.29 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |   7.722 ms | 0.0940 ms | 0.0785 ms |   7.744 ms |  0.86 |    0.01 |   15.6250 |         - |        - |   291.84 KB |
|            CustomDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |   7.713 ms | 0.1531 ms | 0.1572 ms |   7.703 ms |  0.87 |    0.02 |   15.6250 |         - |        - |   288.01 KB |
|                                        |               |               |            |            |           |           |            |       |         |           |           |          |             |
|                  EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |   8.222 ms | 0.1017 ms | 0.1323 ms |   8.210 ms |  1.00 |    0.00 |   15.6250 |         - |        - |   306.22 KB |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |  10.133 ms | 0.1974 ms | 0.1847 ms |  10.122 ms |  1.23 |    0.03 |   46.8750 |   15.6250 |        - |   863.04 KB |
|                 VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |   7.695 ms | 0.1496 ms | 0.1469 ms |   7.726 ms |  0.93 |    0.02 |         - |         - |        - |   254.13 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |   7.132 ms | 0.1359 ms | 0.1205 ms |   7.175 ms |  0.86 |    0.01 |         - |         - |        - |   250.26 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |   7.900 ms | 0.1401 ms | 0.1242 ms |   7.885 ms |  0.96 |    0.02 |   15.6250 |         - |        - |   291.88 KB |
|            CustomDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |   7.733 ms | 0.1396 ms | 0.1306 ms |   7.769 ms |  0.94 |    0.02 |   15.6250 |         - |        - |   288.05 KB |
|                                        |               |               |            |            |           |           |            |       |         |           |           |          |             |
|                  **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |       **1000** |  **24.740 ms** | **0.4632 ms** | **0.4333 ms** |  **24.701 ms** |  **1.00** |    **0.00** |  **156.2500** |         **-** |        **-** |  **2940.86 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  30.815 ms | 0.6141 ms | 0.5744 ms |  30.822 ms |  1.25 |    0.03 |  218.7500 |   93.7500 |        - |  4490.22 KB |
|                 VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  14.493 ms | 0.2815 ms | 0.4545 ms |  14.440 ms |  0.59 |    0.02 |  125.0000 |   93.7500 |  31.2500 |  2430.12 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  15.004 ms | 0.2813 ms | 0.2631 ms |  15.050 ms |  0.61 |    0.02 |  125.0000 |   93.7500 |  31.2500 |  2390.84 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  21.744 ms | 0.2312 ms | 0.2050 ms |  21.774 ms |  0.88 |    0.02 |  156.2500 |   62.5000 |  31.2500 |   2833.1 KB |
|            CustomDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  20.833 ms | 0.4020 ms | 0.4785 ms |  20.852 ms |  0.84 |    0.03 |  156.2500 |   62.5000 |  31.2500 |  2801.22 KB |
|                                        |               |               |            |            |           |           |            |       |         |           |           |          |             |
|                  EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  23.619 ms | 0.4543 ms | 0.4027 ms |  23.711 ms |  1.00 |    0.00 |  156.2500 |         - |        - |  2925.63 KB |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  38.985 ms | 0.7609 ms | 0.9345 ms |  38.961 ms |  1.64 |    0.04 |  461.5385 |  230.7692 |        - |  8482.51 KB |
|                 VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  14.640 ms | 0.2479 ms | 0.2197 ms |  14.664 ms |  0.62 |    0.02 |  125.0000 |   93.7500 |  31.2500 |   2452.7 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  14.463 ms | 0.2867 ms | 0.3301 ms |  14.461 ms |  0.61 |    0.02 |  125.0000 |   93.7500 |  31.2500 |  2413.89 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  15.767 ms | 0.4597 ms | 1.3189 ms |  15.557 ms |  0.72 |    0.06 |         - |         - |        - |  2833.68 KB |
|            CustomDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  18.988 ms | 0.3315 ms | 0.3101 ms |  19.014 ms |  0.80 |    0.02 |  125.0000 |   31.2500 |        - |  2801.27 KB |
|                                        |               |               |            |            |           |           |            |       |         |           |           |          |             |
|                  **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |      **10000** | **194.241 ms** | **3.8389 ms** | **5.3816 ms** | **196.426 ms** |  **1.00** |    **0.00** | **1333.3333** |         **-** |        **-** | **29238.34 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 272.004 ms | 5.3983 ms | 7.2065 ms | 274.425 ms |  1.40 |    0.05 | 2000.0000 |  500.0000 |        - | 44991.23 KB |
|                 VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 124.557 ms | 1.7748 ms | 1.4820 ms | 124.914 ms |  0.64 |    0.03 | 1000.0000 |  600.0000 | 200.0000 |  25934.4 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 126.425 ms | 2.4508 ms | 3.6682 ms | 127.145 ms |  0.65 |    0.02 | 1000.0000 |  600.0000 | 200.0000 | 25543.99 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 184.979 ms | 3.6978 ms | 4.5412 ms | 182.990 ms |  0.96 |    0.04 | 1000.0000 |  333.3333 |        - | 30792.27 KB |
|            CustomDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 179.234 ms | 3.5654 ms | 5.2261 ms | 178.329 ms |  0.92 |    0.04 | 1000.0000 |  333.3333 |        - | 30330.05 KB |
|                                        |               |               |            |            |           |           |            |       |         |           |           |          |             |
|                  EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 180.566 ms | 3.3841 ms | 3.1655 ms | 181.104 ms |  1.00 |    0.00 | 1333.3333 |         - |        - | 29221.78 KB |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 337.881 ms | 5.8937 ms | 9.5173 ms | 337.257 ms |  1.87 |    0.07 | 4500.0000 | 1000.0000 |        - | 85053.84 KB |
|                 VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 113.694 ms | 2.2188 ms | 1.9669 ms | 112.851 ms |  0.63 |    0.01 | 1000.0000 |  600.0000 | 200.0000 | 26170.44 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 117.473 ms | 2.3460 ms | 3.9197 ms | 116.746 ms |  0.65 |    0.02 | 1000.0000 |  600.0000 | 200.0000 | 25789.41 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 172.886 ms | 3.4456 ms | 4.9416 ms | 172.669 ms |  0.95 |    0.03 | 1000.0000 |  333.3333 |        - | 30792.71 KB |
|            CustomDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 191.558 ms | 3.7249 ms | 4.2896 ms | 191.565 ms |  1.06 |    0.03 | 1250.0000 |  500.0000 |        - | 30330.38 KB |
