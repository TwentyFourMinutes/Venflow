``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                                 Method |           Job |       Runtime | QueryCount |       Mean |     Error |     StdDev |     Median | Ratio | RatioSD |     Gen 0 |     Gen 1 |    Gen 2 |   Allocated |
|--------------------------------------- |-------------- |-------------- |----------- |-----------:|----------:|-----------:|-----------:|------:|--------:|----------:|----------:|---------:|------------:|
|                  **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |         **10** |   **9.116 ms** | **0.2500 ms** |  **0.7173 ms** |   **9.274 ms** |  **1.00** |    **0.00** |         **-** |         **-** |        **-** |     **58.5 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |   9.278 ms | 0.1840 ms |  0.3590 ms |   9.266 ms |  1.05 |    0.13 |         - |         - |        - |    75.94 KB |
|                 VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |   8.686 ms | 0.2908 ms |  0.8573 ms |   8.676 ms |  0.96 |    0.13 |         - |         - |        - |    33.13 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |   8.444 ms | 0.2625 ms |  0.7490 ms |   8.540 ms |  0.93 |    0.12 |         - |         - |        - |    32.74 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |   8.524 ms | 0.1684 ms |  0.4495 ms |   8.477 ms |  0.95 |    0.10 |         - |         - |        - |    35.83 KB |
|            CustomDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |   8.518 ms | 0.1874 ms |  0.5497 ms |   8.505 ms |  0.94 |    0.12 |         - |         - |        - |    35.35 KB |
|                                        |               |               |            |            |           |            |            |       |         |           |           |          |             |
|                  EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |   9.158 ms | 0.1824 ms |  0.4118 ms |   9.123 ms |  1.00 |    0.00 |         - |         - |        - |    44.08 KB |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |   9.562 ms | 0.1901 ms |  0.4805 ms |   9.558 ms |  1.05 |    0.09 |         - |         - |        - |   100.82 KB |
|                 VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |   8.716 ms | 0.1569 ms |  0.3818 ms |   8.735 ms |  0.96 |    0.06 |         - |         - |        - |    33.12 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |   8.438 ms | 0.2114 ms |  0.5963 ms |   8.516 ms |  0.92 |    0.10 |         - |         - |        - |    32.73 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |   8.292 ms | 0.2792 ms |  0.7966 ms |   8.467 ms |  0.93 |    0.08 |         - |         - |        - |    35.85 KB |
|            CustomDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |   8.606 ms | 0.1721 ms |  0.3436 ms |   8.521 ms |  0.94 |    0.06 |         - |         - |        - |    35.38 KB |
|                                        |               |               |            |            |           |            |            |       |         |           |           |          |             |
|                  **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |        **100** |  **11.164 ms** | **0.3544 ms** |  **1.0338 ms** |  **11.317 ms** |  **1.00** |    **0.00** |         **-** |         **-** |        **-** |   **320.95 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |  11.730 ms | 0.2313 ms |  0.2664 ms |  11.758 ms |  0.96 |    0.02 |   15.6250 |         - |        - |   477.93 KB |
|                 VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |   9.567 ms | 0.2944 ms |  0.8681 ms |   9.652 ms |  0.86 |    0.11 |         - |         - |        - |   252.09 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |   9.301 ms | 0.2985 ms |  0.8707 ms |   9.440 ms |  0.84 |    0.08 |         - |         - |        - |   248.35 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |   9.518 ms | 0.1855 ms |  0.2888 ms |   9.519 ms |  0.79 |    0.05 |         - |         - |        - |   291.84 KB |
|            CustomDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |   9.643 ms | 0.3340 ms |  0.9849 ms |   9.567 ms |  0.87 |    0.09 |         - |         - |        - |   288.01 KB |
|                                        |               |               |            |            |           |            |            |       |         |           |           |          |             |
|                  EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |  10.904 ms | 0.2154 ms |  0.4201 ms |  10.775 ms |  1.00 |    0.00 |         - |         - |        - |   307.07 KB |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |  12.888 ms | 0.0908 ms |  0.0805 ms |  12.868 ms |  1.13 |    0.04 |   31.2500 |         - |        - |   863.41 KB |
|                 VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |   9.103 ms | 0.1186 ms |  0.1110 ms |   9.122 ms |  0.80 |    0.03 |         - |         - |        - |   254.17 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |  10.041 ms | 0.1374 ms |  0.1285 ms |  10.026 ms |  0.88 |    0.03 |         - |         - |        - |   250.35 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |   7.397 ms | 0.1478 ms |  0.3366 ms |   7.280 ms |  0.68 |    0.02 |         - |         - |        - |   291.88 KB |
|            CustomDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |   7.897 ms | 0.1268 ms |  0.1186 ms |   7.925 ms |  0.70 |    0.03 |         - |         - |        - |   288.06 KB |
|                                        |               |               |            |            |           |            |            |       |         |           |           |          |             |
|                  **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |       **1000** |  **24.922 ms** | **0.3378 ms** |  **0.2821 ms** |  **24.869 ms** |  **1.00** |    **0.00** |   **93.7500** |         **-** |        **-** |  **2940.76 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  31.033 ms | 0.5962 ms |  0.6379 ms |  31.007 ms |  1.25 |    0.03 |  156.2500 |   62.5000 |        - |   4490.1 KB |
|                 VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  14.048 ms | 0.2669 ms |  0.3074 ms |  14.009 ms |  0.57 |    0.01 |  125.0000 |   93.7500 |  46.8750 |  2429.53 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  14.530 ms | 0.2702 ms |  0.2256 ms |  14.474 ms |  0.58 |    0.01 |  125.0000 |   93.7500 |  46.8750 |  2390.87 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  21.039 ms | 0.3835 ms |  0.3588 ms |  20.871 ms |  0.85 |    0.02 |  125.0000 |   62.5000 |  31.2500 |  2833.11 KB |
|            CustomDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  21.011 ms | 0.2799 ms |  0.2481 ms |  21.022 ms |  0.84 |    0.01 |  125.0000 |   62.5000 |  31.2500 |  2801.24 KB |
|                                        |               |               |            |            |           |            |            |       |         |           |           |          |             |
|                  EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  27.687 ms | 0.9144 ms |  2.6961 ms |  28.878 ms |  1.00 |    0.00 |   93.7500 |         - |        - |   2925.7 KB |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  42.233 ms | 0.8383 ms |  2.0563 ms |  42.342 ms |  1.51 |    0.15 |  285.7143 |  142.8571 |        - |  8481.96 KB |
|                 VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  17.103 ms | 0.3167 ms |  0.3111 ms |  17.086 ms |  0.57 |    0.03 |   93.7500 |   62.5000 |  31.2500 |  2452.93 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  16.331 ms | 0.3234 ms |  0.5404 ms |  16.166 ms |  0.55 |    0.03 |   93.7500 |   62.5000 |  31.2500 |  2413.61 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  24.628 ms | 0.4801 ms |  0.4715 ms |  24.520 ms |  0.83 |    0.04 |   93.7500 |   31.2500 |        - |  2833.24 KB |
|            CustomDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  21.587 ms | 0.3733 ms |  0.5233 ms |  21.491 ms |  0.73 |    0.03 |   62.5000 |         - |        - |  2801.33 KB |
|                                        |               |               |            |            |           |            |            |       |         |           |           |          |             |
|                  **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |      **10000** | **205.179 ms** | **3.6215 ms** |  **7.6389 ms** | **202.961 ms** |  **1.00** |    **0.00** | **1000.0000** |         **-** |        **-** | **29238.97 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 269.404 ms | 5.2626 ms |  6.6556 ms | 269.096 ms |  1.29 |    0.07 | 1500.0000 |  500.0000 |        - | 44992.41 KB |
|                 VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 118.250 ms | 1.5249 ms |  1.3518 ms | 118.227 ms |  0.56 |    0.03 |  800.0000 |  400.0000 | 200.0000 | 25934.97 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 126.545 ms | 4.3499 ms | 12.8257 ms | 129.497 ms |  0.63 |    0.04 |  500.0000 |  250.0000 |        - | 25567.51 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 178.325 ms | 3.5462 ms |  7.9315 ms | 178.862 ms |  0.87 |    0.04 |  750.0000 |  250.0000 |        - | 30792.27 KB |
|            CustomDapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 171.330 ms | 3.4060 ms |  5.3027 ms | 170.154 ms |  0.83 |    0.05 |  666.6667 |  333.3333 |        - | 30330.05 KB |
|                                        |               |               |            |            |           |            |            |       |         |           |           |          |             |
|                  EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 174.709 ms | 3.4879 ms |  3.8768 ms | 174.961 ms |  1.00 |    0.00 | 1000.0000 |         - |        - | 29220.22 KB |
|  EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 409.604 ms | 7.9727 ms | 13.0995 ms | 409.673 ms |  2.34 |    0.08 | 3000.0000 | 1000.0000 |        - | 85052.82 KB |
|                 VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 136.581 ms | 3.5093 ms | 10.3473 ms | 134.489 ms |  0.84 |    0.05 |  500.0000 |  250.0000 |        - |  26169.6 KB |
| VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 160.127 ms | 3.1891 ms |  4.4706 ms | 159.755 ms |  0.92 |    0.03 |  800.0000 |  600.0000 | 200.0000 | 25780.43 KB |
|       RecommendedDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 212.047 ms | 4.2056 ms |  6.1645 ms | 212.776 ms |  1.22 |    0.05 |  666.6667 |  333.3333 |        - | 30792.48 KB |
|            CustomDapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 195.083 ms | 3.7400 ms |  4.9928 ms | 195.361 ms |  1.12 |    0.05 |  666.6667 |  333.3333 |        - | 30330.25 KB |
