``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.7.21379.14
  [Host]   : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                                 Method | BatchCount |       Mean |     Error |    StdDev | Ratio | RatioSD |     Gen 0 |     Gen 1 |    Gen 2 | Allocated |
|--------------------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|----------:|----------:|---------:|----------:|
|                  **EfCoreQueryBatchAsync** |         **10** |   **7.527 ms** | **0.1476 ms** | **0.2772 ms** |  **1.00** |    **0.00** |         **-** |         **-** |        **-** |     **42 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |         10 |   8.069 ms | 0.1516 ms | 0.1344 ms |  1.03 |    0.05 |         - |         - |        - |     80 KB |
|                 VenflowQueryBatchAsync |         10 |   7.169 ms | 0.1424 ms | 0.2972 ms |  0.95 |    0.03 |         - |         - |        - |     29 KB |
| VenflowQueryBatchNoChangeTrackingAsync |         10 |   7.683 ms | 0.1236 ms | 0.1156 ms |  0.99 |    0.04 |         - |         - |        - |     29 KB |
|       RecommendedDapperQueryBatchAsync |         10 |   7.124 ms | 0.1321 ms | 0.1236 ms |  0.92 |    0.05 |         - |         - |        - |     30 KB |
|            CustomDapperQueryBatchAsync |         10 |   7.702 ms | 0.1182 ms | 0.1047 ms |  0.99 |    0.04 |         - |         - |        - |     29 KB |
|                                        |            |            |           |           |       |         |           |           |          |           |
|                  **EfCoreQueryBatchAsync** |        **100** |   **9.112 ms** | **0.1795 ms** | **0.3903 ms** |  **1.00** |    **0.00** |         **-** |         **-** |        **-** |    **316 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |        100 |  10.232 ms | 0.1659 ms | 0.1552 ms |  1.06 |    0.03 |   15.6250 |         - |        - |    687 KB |
|                 VenflowQueryBatchAsync |        100 |   7.866 ms | 0.1553 ms | 0.2324 ms |  0.85 |    0.04 |         - |         - |        - |    228 KB |
| VenflowQueryBatchNoChangeTrackingAsync |        100 |   8.385 ms | 0.1493 ms | 0.1396 ms |  0.87 |    0.03 |         - |         - |        - |    224 KB |
|       RecommendedDapperQueryBatchAsync |        100 |   8.239 ms | 0.1441 ms | 0.1348 ms |  0.85 |    0.04 |         - |         - |        - |    246 KB |
|            CustomDapperQueryBatchAsync |        100 |   7.905 ms | 0.1509 ms | 0.1615 ms |  0.83 |    0.03 |         - |         - |        - |    236 KB |
|                                        |            |            |           |           |       |         |           |           |          |           |
|                  **EfCoreQueryBatchAsync** |       **1000** |  **24.513 ms** | **0.4371 ms** | **0.6675 ms** |  **1.00** |    **0.00** |   **93.7500** |         **-** |        **-** |  **3,051 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |       1000 |  32.363 ms | 0.4906 ms | 0.4349 ms |  1.31 |    0.04 |  250.0000 |  125.0000 |        - |  6,748 KB |
|                 VenflowQueryBatchAsync |       1000 |  14.496 ms | 0.2893 ms | 0.4589 ms |  0.59 |    0.02 |   62.5000 |   31.2500 |        - |  2,204 KB |
| VenflowQueryBatchNoChangeTrackingAsync |       1000 |  14.276 ms | 0.2556 ms | 0.2391 ms |  0.58 |    0.02 |   62.5000 |   31.2500 |        - |  2,167 KB |
|       RecommendedDapperQueryBatchAsync |       1000 |  18.943 ms | 0.3583 ms | 0.3351 ms |  0.77 |    0.03 |   62.5000 |   31.2500 |        - |  2,392 KB |
|            CustomDapperQueryBatchAsync |       1000 |  19.638 ms | 0.3441 ms | 0.3682 ms |  0.80 |    0.03 |   62.5000 |   31.2500 |        - |  2,299 KB |
|                                        |            |            |           |           |       |         |           |           |          |           |
|                  **EfCoreQueryBatchAsync** |      **10000** | **183.596 ms** | **3.6074 ms** | **4.4302 ms** |  **1.00** |    **0.00** | **1000.0000** |         **-** |        **-** | **30,503 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |      10000 | 306.043 ms | 6.0079 ms | 7.5981 ms |  1.67 |    0.06 | 2500.0000 | 1000.0000 |        - | 67,739 KB |
|                 VenflowQueryBatchAsync |      10000 | 112.123 ms | 2.2261 ms | 5.9032 ms |  0.58 |    0.03 |  600.0000 |  400.0000 | 200.0000 | 23,702 KB |
| VenflowQueryBatchNoChangeTrackingAsync |      10000 | 109.985 ms | 2.1978 ms | 4.6358 ms |  0.59 |    0.04 |  600.0000 |  400.0000 | 200.0000 | 23,314 KB |
|       RecommendedDapperQueryBatchAsync |      10000 | 172.813 ms | 3.3935 ms | 3.7719 ms |  0.94 |    0.04 |  666.6667 |  333.3333 |        - | 26,204 KB |
|            CustomDapperQueryBatchAsync |      10000 | 166.523 ms | 3.2761 ms | 5.6511 ms |  0.91 |    0.04 |  666.6667 |  333.3333 |        - | 25,329 KB |
