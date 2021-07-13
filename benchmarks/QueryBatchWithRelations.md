``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                                 Method | BatchCount |       Mean |     Error |    StdDev | Ratio | RatioSD |     Gen 0 |     Gen 1 |    Gen 2 | Allocated |
|--------------------------------------- |----------- |-----------:|----------:|----------:|------:|--------:|----------:|----------:|---------:|----------:|
|                  **EfCoreQueryBatchAsync** |         **10** |   **6.239 ms** | **0.1184 ms** | **0.1107 ms** |  **1.00** |    **0.00** |         **-** |         **-** |        **-** |     **42 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |         10 |   6.751 ms | 0.0308 ms | 0.0273 ms |  1.08 |    0.02 |         - |         - |        - |     80 KB |
|                 VenflowQueryBatchAsync |         10 |   5.832 ms | 0.0212 ms | 0.0235 ms |  0.94 |    0.02 |         - |         - |        - |     29 KB |
| VenflowQueryBatchNoChangeTrackingAsync |         10 |   6.376 ms | 0.0262 ms | 0.0245 ms |  1.02 |    0.02 |         - |         - |        - |     29 KB |
|       RecommendedDapperQueryBatchAsync |         10 |   5.828 ms | 0.0270 ms | 0.0429 ms |  0.94 |    0.02 |         - |         - |        - |     30 KB |
|            CustomDapperQueryBatchAsync |         10 |   6.409 ms | 0.0197 ms | 0.0185 ms |  1.03 |    0.02 |         - |         - |        - |     29 KB |
|                                        |            |            |           |           |       |         |           |           |          |           |
|                  **EfCoreQueryBatchAsync** |        **100** |   **7.423 ms** | **0.1115 ms** | **0.1369 ms** |  **1.00** |    **0.00** |   **15.6250** |         **-** |        **-** |    **316 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |        100 |   8.492 ms | 0.0697 ms | 0.0652 ms |  1.14 |    0.02 |   31.2500 |         - |        - |    686 KB |
|                 VenflowQueryBatchAsync |        100 |   7.024 ms | 0.0588 ms | 0.0550 ms |  0.94 |    0.02 |    7.8125 |         - |        - |    228 KB |
| VenflowQueryBatchNoChangeTrackingAsync |        100 | 432.548 ms | 0.5899 ms | 0.4926 ms | 58.10 |    1.27 |         - |         - |        - |    227 KB |
|       RecommendedDapperQueryBatchAsync |        100 |   7.037 ms | 0.0502 ms | 0.0470 ms |  0.95 |    0.02 |    7.8125 |         - |        - |    246 KB |
|            CustomDapperQueryBatchAsync |        100 |   6.427 ms | 0.0300 ms | 0.0295 ms |  0.86 |    0.02 |    7.8125 |         - |        - |    236 KB |
|                                        |            |            |           |           |       |         |           |           |          |           |
|                  **EfCoreQueryBatchAsync** |       **1000** |  **20.699 ms** | **0.2485 ms** | **0.2325 ms** |  **1.00** |    **0.00** |  **156.2500** |         **-** |        **-** |  **3,052 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |       1000 |  29.524 ms | 0.5765 ms | 0.5920 ms |  1.43 |    0.03 |  343.7500 |  156.2500 |        - |  6,748 KB |
|                 VenflowQueryBatchAsync |       1000 |  13.515 ms | 0.1681 ms | 0.1573 ms |  0.65 |    0.01 |  125.0000 |   78.1250 |  31.2500 |  2,205 KB |
| VenflowQueryBatchNoChangeTrackingAsync |       1000 |  12.901 ms | 0.2474 ms | 0.2849 ms |  0.62 |    0.02 |  125.0000 |   78.1250 |  31.2500 |  2,166 KB |
|       RecommendedDapperQueryBatchAsync |       1000 |  15.997 ms | 0.2224 ms | 0.2080 ms |  0.77 |    0.01 |   93.7500 |   31.2500 |        - |  2,392 KB |
|            CustomDapperQueryBatchAsync |       1000 |  16.319 ms | 0.3177 ms | 0.4349 ms |  0.78 |    0.02 |   93.7500 |   31.2500 |        - |  2,299 KB |
|                                        |            |            |           |           |       |         |           |           |          |           |
|                  **EfCoreQueryBatchAsync** |      **10000** | **149.725 ms** | **2.8207 ms** | **2.7703 ms** |  **1.00** |    **0.00** | **1500.0000** |         **-** |        **-** | **30,503 KB** |
|  EfCoreQueryBatchNoChangeTrackingAsync |      10000 | 257.982 ms | 5.0149 ms | 6.8644 ms |  1.74 |    0.06 | 3500.0000 | 1000.0000 |        - | 67,741 KB |
|                 VenflowQueryBatchAsync |      10000 |  99.178 ms | 1.9516 ms | 3.3665 ms |  0.67 |    0.03 | 1000.0000 |  600.0000 | 200.0000 | 23,705 KB |
| VenflowQueryBatchNoChangeTrackingAsync |      10000 |  98.154 ms | 1.8012 ms | 2.6401 ms |  0.66 |    0.03 | 1000.0000 |  600.0000 | 200.0000 | 23,314 KB |
|       RecommendedDapperQueryBatchAsync |      10000 | 136.419 ms | 2.7245 ms | 3.1375 ms |  0.91 |    0.03 |  750.0000 |  250.0000 |        - | 26,202 KB |
|            CustomDapperQueryBatchAsync |      10000 | 129.136 ms | 2.5342 ms | 4.3713 ms |  0.85 |    0.04 |  750.0000 |  250.0000 |        - | 25,329 KB |
