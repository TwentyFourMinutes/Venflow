``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                                 Method | BatchCount |       Mean |     Error |    StdDev | Ratio | RatioSD |     Gen 0 |     Gen 1 |    Gen 2 | Allocated |
|--------------------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|----------:|----------:|---------:|----------:|
|                  **EfCoreQueryBatchAsync** |         **10** |   **8.350 ms** | **0.1636 ms** | **0.2346 ms** |  **1.00** |    **0.00** |         **-** |         **-** |        **-** |     **42 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |         10 |   8.887 ms | 0.1735 ms | 0.2701 ms |  1.06 |    0.04 |         - |         - |        - |     80 KB |
|                 VenflowQueryBatchAsync |         10 |   7.536 ms | 0.1454 ms | 0.1616 ms |  0.90 |    0.04 |         - |         - |        - |     29 KB |
| VenflowQueryBatchNoChangeTrackingAsync |         10 |   8.341 ms | 0.1499 ms | 0.2505 ms |  1.00 |    0.03 |         - |         - |        - |     29 KB |
|       RecommendedDapperQueryBatchAsync |         10 |   7.724 ms | 0.1434 ms | 0.3266 ms |  0.95 |    0.05 |         - |         - |        - |     30 KB |
|            CustomDapperQueryBatchAsync |         10 |   8.209 ms | 0.1266 ms | 0.1122 ms |  0.99 |    0.03 |         - |         - |        - |     29 KB |
|                                        |            |            |           |           |       |         |           |           |          |           |
|                  **EfCoreQueryBatchAsync** |        **100** |   **9.758 ms** | **0.1868 ms** | **0.3774 ms** |  **1.00** |    **0.00** |         **-** |         **-** |        **-** |    **316 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |        100 |  10.606 ms | 0.2037 ms | 0.4959 ms |  1.10 |    0.05 |   15.6250 |         - |        - |    687 KB |
|                 VenflowQueryBatchAsync |        100 |   8.921 ms | 0.1664 ms | 0.1634 ms |  0.89 |    0.04 |         - |         - |        - |    228 KB |
| VenflowQueryBatchNoChangeTrackingAsync |        100 |   8.312 ms | 0.1633 ms | 0.1881 ms |  0.83 |    0.04 |         - |         - |        - |    224 KB |
|       RecommendedDapperQueryBatchAsync |        100 |   9.100 ms | 0.1771 ms | 0.1819 ms |  0.90 |    0.03 |         - |         - |        - |    246 KB |
|            CustomDapperQueryBatchAsync |        100 |   9.072 ms | 0.1672 ms | 0.1482 ms |  0.91 |    0.04 |         - |         - |        - |    236 KB |
|                                        |            |            |           |           |       |         |           |           |          |           |
|                  **EfCoreQueryBatchAsync** |       **1000** |  **27.005 ms** | **0.5383 ms** | **0.6808 ms** |  **1.00** |    **0.00** |   **93.7500** |         **-** |        **-** |  **3,051 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |       1000 |  36.797 ms | 0.6916 ms | 0.6792 ms |  1.37 |    0.04 |  214.2857 |   71.4286 |        - |  6,747 KB |
|                 VenflowQueryBatchAsync |       1000 |  15.379 ms | 0.2908 ms | 0.4695 ms |  0.57 |    0.02 |   62.5000 |   31.2500 |        - |  2,205 KB |
| VenflowQueryBatchNoChangeTrackingAsync |       1000 |  15.946 ms | 0.2967 ms | 0.3047 ms |  0.59 |    0.02 |   62.5000 |   31.2500 |        - |  2,166 KB |
|       RecommendedDapperQueryBatchAsync |       1000 |  20.953 ms | 0.3780 ms | 0.3157 ms |  0.78 |    0.02 |   62.5000 |   31.2500 |        - |  2,392 KB |
|            CustomDapperQueryBatchAsync |       1000 |  21.178 ms | 0.4137 ms | 0.7245 ms |  0.78 |    0.03 |   62.5000 |   31.2500 |        - |  2,299 KB |
|                                        |            |            |           |           |       |         |           |           |          |           |
|                  **EfCoreQueryBatchAsync** |      **10000** | **202.793 ms** | **4.0124 ms** | **5.0744 ms** |  **1.00** |    **0.00** | **1000.0000** |         **-** |        **-** | **30,502 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |      10000 | 308.693 ms | 5.9840 ms | 6.6512 ms |  1.53 |    0.05 | 2500.0000 | 1000.0000 |        - | 67,740 KB |
|                 VenflowQueryBatchAsync |      10000 | 114.688 ms | 2.2844 ms | 4.7177 ms |  0.56 |    0.03 |  600.0000 |  400.0000 | 200.0000 | 23,703 KB |
| VenflowQueryBatchNoChangeTrackingAsync |      10000 | 118.345 ms | 2.3255 ms | 4.8542 ms |  0.59 |    0.03 |  600.0000 |  400.0000 | 200.0000 | 23,314 KB |
|       RecommendedDapperQueryBatchAsync |      10000 | 189.369 ms | 3.7531 ms | 7.9982 ms |  0.91 |    0.04 |  666.6667 |  333.3333 |        - | 26,200 KB |
|            CustomDapperQueryBatchAsync |      10000 | 180.887 ms | 3.5556 ms | 7.7295 ms |  0.91 |    0.05 |  666.6667 |  333.3333 |        - | 25,329 KB |
