``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                                 Method | QueryCount |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |     Gen 0 |     Gen 1 |    Gen 2 | Allocated |
|--------------------------------------- |----------- |-----------:|----------:|----------:|-----------:|------:|--------:|----------:|----------:|---------:|----------:|
|                  **EfCoreQueryBatchAsync** |         **10** |   **7.795 ms** | **0.1525 ms** | **0.1815 ms** |   **7.744 ms** |  **1.00** |    **0.00** |         **-** |         **-** |        **-** |     **42 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |         10 |   7.813 ms | 0.1397 ms | 0.2047 ms |   7.824 ms |  1.01 |    0.03 |         - |         - |        - |     79 KB |
|                 VenflowQueryBatchAsync |         10 |   7.242 ms | 0.1395 ms | 0.1165 ms |   7.276 ms |  0.92 |    0.03 |         - |         - |        - |     29 KB |
| VenflowQueryBatchNoChangeTrackingAsync |         10 |   6.952 ms | 0.1325 ms | 0.3124 ms |   6.843 ms |  0.94 |    0.03 |         - |         - |        - |     29 KB |
|       RecommendedDapperQueryBatchAsync |         10 |   6.843 ms | 0.1361 ms | 0.2420 ms |   6.788 ms |  0.88 |    0.05 |         - |         - |        - |     30 KB |
|            CustomDapperQueryBatchAsync |         10 |   6.933 ms | 0.1318 ms | 0.3080 ms |   6.884 ms |  0.92 |    0.05 |         - |         - |        - |     29 KB |
|                                        |            |            |           |           |            |       |         |           |           |          |           |
|                  **EfCoreQueryBatchAsync** |        **100** |   **8.621 ms** | **0.1677 ms** | **0.2708 ms** |   **8.587 ms** |  **1.00** |    **0.00** |         **-** |         **-** |        **-** |    **316 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |        100 |   9.880 ms | 0.1964 ms | 0.2484 ms |   9.813 ms |  1.14 |    0.04 |   15.6250 |         - |        - |    687 KB |
|                 VenflowQueryBatchAsync |        100 |   7.632 ms | 0.1515 ms | 0.3630 ms |   7.616 ms |  0.90 |    0.04 |         - |         - |        - |    228 KB |
| VenflowQueryBatchNoChangeTrackingAsync |        100 |   8.007 ms | 0.1587 ms | 0.2173 ms |   8.009 ms |  0.93 |    0.04 |         - |         - |        - |    224 KB |
|       RecommendedDapperQueryBatchAsync |        100 |   7.803 ms | 0.1417 ms | 0.1740 ms |   7.758 ms |  0.90 |    0.04 |         - |         - |        - |    246 KB |
|            CustomDapperQueryBatchAsync |        100 |   8.119 ms | 0.1596 ms | 0.1961 ms |   8.078 ms |  0.94 |    0.03 |         - |         - |        - |    236 KB |
|                                        |            |            |           |           |            |       |         |           |           |          |           |
|                  **EfCoreQueryBatchAsync** |       **1000** |  **23.122 ms** | **0.4561 ms** | **0.6966 ms** |  **23.260 ms** |  **1.00** |    **0.00** |   **93.7500** |         **-** |        **-** |  **3,051 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |       1000 |  33.871 ms | 0.6537 ms | 0.8726 ms |  34.018 ms |  1.47 |    0.07 |  214.2857 |   71.4286 |        - |  6,748 KB |
|                 VenflowQueryBatchAsync |       1000 |  14.220 ms | 0.2834 ms | 0.6455 ms |  14.088 ms |  0.63 |    0.03 |   62.5000 |   31.2500 |        - |  2,205 KB |
| VenflowQueryBatchNoChangeTrackingAsync |       1000 |  14.269 ms | 0.2837 ms | 0.2913 ms |  14.205 ms |  0.61 |    0.02 |   62.5000 |   31.2500 |        - |  2,166 KB |
|       RecommendedDapperQueryBatchAsync |       1000 |  18.602 ms | 0.3617 ms | 0.4306 ms |  18.578 ms |  0.80 |    0.03 |   62.5000 |   31.2500 |        - |  2,392 KB |
|            CustomDapperQueryBatchAsync |       1000 |  18.834 ms | 0.3703 ms | 0.6289 ms |  18.780 ms |  0.82 |    0.03 |   62.5000 |   31.2500 |        - |  2,299 KB |
|                                        |            |            |           |           |            |       |         |           |           |          |           |
|                  **EfCoreQueryBatchAsync** |      **10000** | **177.409 ms** | **3.4612 ms** | **4.6206 ms** | **178.370 ms** |  **1.00** |    **0.00** | **1000.0000** |         **-** |        **-** | **30,503 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |      10000 | 273.089 ms | 5.2539 ms | 5.8397 ms | 273.329 ms |  1.54 |    0.05 | 2500.0000 | 1000.0000 |        - | 67,739 KB |
|                 VenflowQueryBatchAsync |      10000 | 105.154 ms | 2.0932 ms | 4.4608 ms | 104.552 ms |  0.59 |    0.02 |  600.0000 |  400.0000 | 200.0000 | 23,703 KB |
| VenflowQueryBatchNoChangeTrackingAsync |      10000 | 110.172 ms | 2.4733 ms | 7.2925 ms | 110.147 ms |  0.58 |    0.03 |  600.0000 |  400.0000 | 200.0000 | 23,313 KB |
|       RecommendedDapperQueryBatchAsync |      10000 | 169.823 ms | 3.3491 ms | 6.9165 ms | 170.004 ms |  0.96 |    0.05 |  666.6667 |  333.3333 |        - | 26,200 KB |
|            CustomDapperQueryBatchAsync |      10000 | 160.065 ms | 3.0963 ms | 4.1335 ms | 161.179 ms |  0.90 |    0.03 |  500.0000 |  250.0000 |        - | 25,328 KB |
