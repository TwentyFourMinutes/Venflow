``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                                   Method | BatchCount |        Mean |     Error |    StdDev | Ratio | RatioSD |    Gen 0 |   Gen 1 |   Gen 2 | Allocated |
|----------------------------------------- |----------- |------------:|----------:|----------:|------:|--------:|---------:|--------:|--------:|----------:|
|                    **EfCoreQueryBatchAsync** |         **10** |    **454.8 μs** |  **12.62 μs** |  **37.21 μs** |  **1.00** |    **0.00** |        **-** |       **-** |       **-** |      **9 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |         10 |    508.0 μs |  10.17 μs |  29.98 μs |  1.13 |    0.12 |        - |       - |       - |     10 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |         10 |    473.3 μs |  11.83 μs |  34.50 μs |  1.05 |    0.12 |        - |       - |       - |     13 KB |
|                   VenflowQueryBatchAsync |         10 |    277.9 μs |   6.20 μs |  18.27 μs |  0.62 |    0.07 |        - |       - |       - |      3 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |         10 |    272.2 μs |   5.62 μs |  16.38 μs |  0.60 |    0.07 |        - |       - |       - |      3 KB |
|                    RepoDbQueryBatchAsync |         10 |    286.7 μs |   5.86 μs |  17.10 μs |  0.64 |    0.06 |        - |       - |       - |      3 KB |
|                    DapperQueryBatchAsync |         10 |    273.0 μs |   6.76 μs |  19.82 μs |  0.60 |    0.07 |        - |       - |       - |      3 KB |
|                                          |            |             |           |           |       |         |          |         |         |           |
|                    **EfCoreQueryBatchAsync** |        **100** |    **555.1 μs** |  **10.94 μs** |  **26.64 μs** |  **1.00** |    **0.00** |   **0.9766** |       **-** |       **-** |     **32 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |        100 |    582.4 μs |  11.60 μs |  25.45 μs |  1.05 |    0.06 |   0.9766 |       - |       - |     36 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |        100 |    558.1 μs |  11.13 μs |  26.66 μs |  1.01 |    0.07 |   0.9766 |       - |       - |     39 KB |
|                   VenflowQueryBatchAsync |        100 |    349.5 μs |   7.56 μs |  22.28 μs |  0.64 |    0.05 |   0.4883 |       - |       - |     16 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |        100 |    338.1 μs |   6.71 μs |  19.03 μs |  0.61 |    0.05 |        - |       - |       - |     12 KB |
|                    RepoDbQueryBatchAsync |        100 |    357.3 μs |  11.04 μs |  32.56 μs |  0.64 |    0.08 |        - |       - |       - |     12 KB |
|                    DapperQueryBatchAsync |        100 |    392.9 μs |   7.84 μs |  22.86 μs |  0.71 |    0.06 |        - |       - |       - |     14 KB |
|                                          |            |             |           |           |       |         |          |         |         |           |
|                    **EfCoreQueryBatchAsync** |       **1000** |  **1,380.0 μs** |  **25.70 μs** |  **47.64 μs** |  **1.00** |    **0.00** |   **9.7656** |       **-** |       **-** |    **264 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |       1000 |  1,517.6 μs |  29.82 μs |  48.16 μs |  1.10 |    0.05 |   9.7656 |  1.9531 |       - |    289 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |       1000 |  1,460.1 μs |  29.11 μs |  42.67 μs |  1.06 |    0.05 |   7.8125 |       - |       - |    292 KB |
|                   VenflowQueryBatchAsync |       1000 |    993.7 μs |  19.81 μs |  33.64 μs |  0.72 |    0.03 |   3.9063 |       - |       - |    136 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |       1000 |    914.8 μs |  18.02 μs |  26.42 μs |  0.66 |    0.03 |   2.9297 |  0.9766 |       - |     97 KB |
|                    RepoDbQueryBatchAsync |       1000 |    965.0 μs |  19.08 μs |  31.87 μs |  0.70 |    0.04 |   1.9531 |       - |       - |     97 KB |
|                    DapperQueryBatchAsync |       1000 |  1,310.6 μs |  25.88 μs |  41.79 μs |  0.95 |    0.04 |   3.9063 |       - |       - |    119 KB |
|                                          |            |             |           |           |       |         |          |         |         |           |
|                    **EfCoreQueryBatchAsync** |      **10000** |  **8,644.5 μs** | **171.99 μs** | **191.17 μs** |  **1.00** |    **0.00** |  **93.7500** |       **-** |       **-** |  **2,684 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |      10000 | 13,610.4 μs | 271.07 μs | 278.37 μs |  1.58 |    0.05 | 125.0000 | 78.1250 | 31.2500 |  2,920 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |      10000 | 13,481.9 μs | 265.99 μs | 345.86 μs |  1.56 |    0.05 | 125.0000 | 78.1250 | 31.2500 |  2,923 KB |
|                   VenflowQueryBatchAsync |      10000 |  7,835.6 μs | 149.29 μs | 188.81 μs |  0.91 |    0.02 |  46.8750 | 15.6250 |       - |  1,431 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |      10000 |  6,965.2 μs | 137.14 μs | 134.69 μs |  0.81 |    0.03 |  39.0625 | 23.4375 |  7.8125 |  1,040 KB |
|                    RepoDbQueryBatchAsync |      10000 |  7,180.1 μs | 142.73 μs | 146.57 μs |  0.83 |    0.03 |  39.0625 | 23.4375 |  7.8125 |  1,041 KB |
|                    DapperQueryBatchAsync |      10000 | 11,002.0 μs | 208.22 μs | 194.77 μs |  1.27 |    0.04 |  31.2500 | 15.6250 |       - |  1,274 KB |
