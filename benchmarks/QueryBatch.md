``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                                   Method | QueryCount |        Mean |     Error |    StdDev | Ratio | RatioSD |    Gen 0 |   Gen 1 |   Gen 2 | Allocated |
|----------------------------------------- |----------- |------------:|----------:|----------:|------:|--------:|---------:|--------:|--------:|----------:|
|                    **EfCoreQueryBatchAsync** |         **10** |    **454.4 μs** |  **10.53 μs** |  **31.04 μs** |  **1.00** |    **0.00** |        **-** |       **-** |       **-** |      **8 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |         10 |    522.2 μs |  10.39 μs |  26.64 μs |  1.15 |    0.09 |        - |       - |       - |     10 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |         10 |    489.9 μs |   9.79 μs |  27.93 μs |  1.08 |    0.09 |        - |       - |       - |     13 KB |
|                   VenflowQueryBatchAsync |         10 |    280.2 μs |   6.44 μs |  18.99 μs |  0.62 |    0.06 |        - |       - |       - |      4 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |         10 |    275.9 μs |   7.36 μs |  21.70 μs |  0.61 |    0.06 |        - |       - |       - |      3 KB |
|                    RepoDbQueryBatchAsync |         10 |    289.2 μs |   5.90 μs |  17.29 μs |  0.64 |    0.05 |        - |       - |       - |      4 KB |
|                    DapperQueryBatchAsync |         10 |    272.4 μs |   6.85 μs |  20.08 μs |  0.60 |    0.06 |        - |       - |       - |      3 KB |
|                                          |            |             |           |           |       |         |          |         |         |           |
|                    **EfCoreQueryBatchAsync** |        **100** |    **538.4 μs** |  **10.73 μs** |  **20.41 μs** |  **1.00** |    **0.00** |   **0.9766** |       **-** |       **-** |     **32 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |        100 |    581.1 μs |  11.41 μs |  16.36 μs |  1.07 |    0.05 |   0.9766 |       - |       - |     36 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |        100 |    548.7 μs |  10.87 μs |  20.14 μs |  1.02 |    0.05 |   0.9766 |       - |       - |     39 KB |
|                   VenflowQueryBatchAsync |        100 |    345.4 μs |   6.89 μs |  15.12 μs |  0.64 |    0.04 |   0.4883 |       - |       - |     16 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |        100 |    325.3 μs |   6.88 μs |  20.08 μs |  0.61 |    0.04 |        - |       - |       - |     12 KB |
|                    RepoDbQueryBatchAsync |        100 |    347.9 μs |   6.92 μs |  20.09 μs |  0.64 |    0.05 |   0.4883 |       - |       - |     13 KB |
|                    DapperQueryBatchAsync |        100 |    377.1 μs |   7.52 μs |  13.94 μs |  0.70 |    0.04 |   0.4883 |       - |       - |     14 KB |
|                                          |            |             |           |           |       |         |          |         |         |           |
|                    **EfCoreQueryBatchAsync** |       **1000** |  **1,265.0 μs** |  **23.92 μs** |  **43.13 μs** |  **1.00** |    **0.00** |   **9.7656** |       **-** |       **-** |    **264 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |       1000 |  1,413.9 μs |  27.65 μs |  27.16 μs |  1.11 |    0.04 |   9.7656 |  1.9531 |       - |    289 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |       1000 |  1,382.3 μs |  26.98 μs |  38.70 μs |  1.09 |    0.05 |   9.7656 |  1.9531 |       - |    292 KB |
|                   VenflowQueryBatchAsync |       1000 |    920.7 μs |  18.37 μs |  29.66 μs |  0.73 |    0.04 |   3.9063 |       - |       - |    136 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |       1000 |    874.9 μs |  17.39 μs |  35.53 μs |  0.70 |    0.04 |   1.9531 |       - |       - |     97 KB |
|                    RepoDbQueryBatchAsync |       1000 |    919.7 μs |  18.05 μs |  38.46 μs |  0.73 |    0.04 |   1.9531 |       - |       - |     97 KB |
|                    DapperQueryBatchAsync |       1000 |  1,269.9 μs |  25.15 μs |  43.38 μs |  1.01 |    0.05 |   3.9063 |       - |       - |    119 KB |
|                                          |            |             |           |           |       |         |          |         |         |           |
|                    **EfCoreQueryBatchAsync** |      **10000** |  **8,045.1 μs** | **156.80 μs** | **253.20 μs** |  **1.00** |    **0.00** |  **93.7500** |       **-** |       **-** |  **2,684 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |      10000 | 12,518.8 μs | 225.57 μs | 199.96 μs |  1.56 |    0.06 | 125.0000 | 78.1250 | 31.2500 |  2,920 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |      10000 | 12,216.8 μs | 241.40 μs | 305.29 μs |  1.52 |    0.06 | 125.0000 | 78.1250 | 31.2500 |  2,923 KB |
|                   VenflowQueryBatchAsync |      10000 |  7,066.5 μs | 136.19 μs | 238.52 μs |  0.88 |    0.04 |  46.8750 | 15.6250 |       - |  1,431 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |      10000 |  6,568.1 μs | 129.78 μs | 168.75 μs |  0.82 |    0.03 |  39.0625 | 23.4375 |  7.8125 |  1,040 KB |
|                    RepoDbQueryBatchAsync |      10000 |  6,638.8 μs | 123.26 μs | 155.88 μs |  0.83 |    0.03 |  39.0625 | 23.4375 |  7.8125 |  1,041 KB |
|                    DapperQueryBatchAsync |      10000 | 10,107.6 μs | 200.86 μs | 300.65 μs |  1.26 |    0.05 |  31.2500 | 15.6250 |       - |  1,274 KB |
