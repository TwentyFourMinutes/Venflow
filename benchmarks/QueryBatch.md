``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.7.21379.14
  [Host]   : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                                   Method | BatchCount |        Mean |     Error |    StdDev | Ratio | RatioSD |    Gen 0 |   Gen 1 |   Gen 2 | Allocated |
|----------------------------------------- |----------- |------------:|----------:|----------:|------:|--------:|---------:|--------:|--------:|----------:|
|                    **EfCoreQueryBatchAsync** |         **10** |    **455.9 μs** |   **9.02 μs** |  **16.49 μs** |  **1.00** |    **0.00** |        **-** |       **-** |       **-** |      **9 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |         10 |    509.1 μs |  10.30 μs |  30.22 μs |  1.12 |    0.08 |        - |       - |       - |     10 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |         10 |    495.6 μs |  10.04 μs |  29.46 μs |  1.11 |    0.06 |        - |       - |       - |     13 KB |
|                   VenflowQueryBatchAsync |         10 |    260.6 μs |   7.12 μs |  21.00 μs |  0.57 |    0.06 |        - |       - |       - |      3 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |         10 |    268.3 μs |   8.91 μs |  26.28 μs |  0.59 |    0.06 |        - |       - |       - |      3 KB |
|                    RepoDbQueryBatchAsync |         10 |    275.2 μs |   7.79 μs |  22.97 μs |  0.61 |    0.05 |        - |       - |       - |      4 KB |
|                    DapperQueryBatchAsync |         10 |    274.1 μs |   7.90 μs |  23.30 μs |  0.61 |    0.06 |        - |       - |       - |      3 KB |
|                                          |            |             |           |           |       |         |          |         |         |           |
|                    **EfCoreQueryBatchAsync** |        **100** |    **534.3 μs** |  **10.04 μs** |  **24.81 μs** |  **1.00** |    **0.00** |   **0.9766** |       **-** |       **-** |     **32 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |        100 |    565.0 μs |  11.15 μs |  28.58 μs |  1.06 |    0.07 |   0.9766 |       - |       - |     36 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |        100 |    534.2 μs |  10.59 μs |  24.97 μs |  1.00 |    0.07 |   0.9766 |       - |       - |     39 KB |
|                   VenflowQueryBatchAsync |        100 |    320.3 μs |   8.20 μs |  24.06 μs |  0.60 |    0.06 |   0.4883 |       - |       - |     16 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |        100 |    320.0 μs |   9.11 μs |  26.85 μs |  0.61 |    0.06 |        - |       - |       - |     11 KB |
|                    RepoDbQueryBatchAsync |        100 |    334.6 μs |   7.46 μs |  21.75 μs |  0.63 |    0.06 |   0.4883 |       - |       - |     13 KB |
|                    DapperQueryBatchAsync |        100 |    362.7 μs |   8.10 μs |  23.88 μs |  0.68 |    0.05 |        - |       - |       - |     14 KB |
|                                          |            |             |           |           |       |         |          |         |         |           |
|                    **EfCoreQueryBatchAsync** |       **1000** |  **1,274.1 μs** |  **25.31 μs** |  **40.14 μs** |  **1.00** |    **0.00** |   **9.7656** |       **-** |       **-** |    **264 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |       1000 |  1,402.3 μs |  27.38 μs |  25.61 μs |  1.11 |    0.04 |   9.7656 |  1.9531 |       - |    289 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |       1000 |  1,410.4 μs |  26.98 μs |  33.13 μs |  1.12 |    0.04 |   9.7656 |  1.9531 |       - |    292 KB |
|                   VenflowQueryBatchAsync |       1000 |    927.9 μs |  18.45 μs |  40.10 μs |  0.73 |    0.04 |   3.9063 |       - |       - |    136 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |       1000 |    873.9 μs |  17.29 μs |  40.08 μs |  0.69 |    0.04 |   1.9531 |       - |       - |     97 KB |
|                    RepoDbQueryBatchAsync |       1000 |    898.4 μs |  17.94 μs |  31.88 μs |  0.70 |    0.04 |   1.9531 |       - |       - |     97 KB |
|                    DapperQueryBatchAsync |       1000 |  1,226.6 μs |  24.14 μs |  32.22 μs |  0.97 |    0.04 |   3.9063 |       - |       - |    119 KB |
|                                          |            |             |           |           |       |         |          |         |         |           |
|                    **EfCoreQueryBatchAsync** |      **10000** |  **8,206.2 μs** | **137.77 μs** | **128.87 μs** |  **1.00** |    **0.00** |  **93.7500** |       **-** |       **-** |  **2,684 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |      10000 | 12,365.0 μs | 238.37 μs | 283.76 μs |  1.52 |    0.04 | 125.0000 | 78.1250 | 31.2500 |  2,920 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |      10000 | 11,880.5 μs | 221.62 μs | 217.66 μs |  1.45 |    0.04 | 125.0000 | 78.1250 | 31.2500 |  2,923 KB |
|                   VenflowQueryBatchAsync |      10000 |  7,263.5 μs | 129.87 μs | 121.48 μs |  0.89 |    0.02 |  46.8750 | 15.6250 |       - |  1,430 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |      10000 |  6,483.1 μs | 110.69 μs | 135.93 μs |  0.79 |    0.03 |  39.0625 | 23.4375 |  7.8125 |  1,040 KB |
|                    RepoDbQueryBatchAsync |      10000 |  6,266.7 μs | 119.53 μs | 221.55 μs |  0.76 |    0.04 |  31.2500 | 15.6250 |       - |  1,041 KB |
|                    DapperQueryBatchAsync |      10000 | 10,153.4 μs | 201.23 μs | 178.38 μs |  1.24 |    0.03 |  31.2500 | 15.6250 |       - |  1,274 KB |
