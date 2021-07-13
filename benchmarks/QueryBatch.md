``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                                   Method | BatchCount |        Mean |     Error |    StdDev |      Median | Ratio | RatioSD |    Gen 0 |   Gen 1 |   Gen 2 | Allocated |
|----------------------------------------- |----------- |------------:|----------:|----------:|------------:|------:|--------:|---------:|--------:|--------:|----------:|
|                    **EfCoreQueryBatchAsync** |         **10** |    **278.0 μs** |   **5.54 μs** |  **15.25 μs** |    **282.2 μs** |  **1.00** |    **0.00** |        **-** |       **-** |       **-** |      **9 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |         10 |    322.5 μs |   9.39 μs |  27.68 μs |    318.1 μs |  1.18 |    0.11 |   0.4883 |       - |       - |     11 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |         10 |    298.0 μs |   5.92 μs |  14.97 μs |    297.0 μs |  1.07 |    0.08 |   0.4883 |       - |       - |     13 KB |
|                   VenflowQueryBatchAsync |         10 |    153.4 μs |   4.63 μs |  13.59 μs |    151.4 μs |  0.56 |    0.05 |        - |       - |       - |      4 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |         10 |    157.6 μs |   4.51 μs |  13.29 μs |    159.3 μs |  0.57 |    0.06 |        - |       - |       - |      3 KB |
|                    RepoDbQueryBatchAsync |         10 |    168.7 μs |   4.48 μs |  13.14 μs |    168.7 μs |  0.61 |    0.06 |        - |       - |       - |      3 KB |
|                    DapperQueryBatchAsync |         10 |    157.1 μs |   4.95 μs |  14.61 μs |    152.7 μs |  0.57 |    0.06 |        - |       - |       - |      3 KB |
|                                          |            |             |           |           |             |       |         |          |         |         |           |
|                    **EfCoreQueryBatchAsync** |        **100** |    **352.2 μs** |   **6.81 μs** |  **10.41 μs** |    **350.6 μs** |  **1.00** |    **0.00** |   **1.4648** |       **-** |       **-** |     **32 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |        100 |    388.4 μs |   7.69 μs |  21.57 μs |    390.2 μs |  1.11 |    0.08 |   1.9531 |       - |       - |     36 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |        100 |    368.3 μs |   7.31 μs |  14.26 μs |    365.5 μs |  1.05 |    0.05 |   1.9531 |       - |       - |     38 KB |
|                   VenflowQueryBatchAsync |        100 |    219.5 μs |   5.86 μs |  17.19 μs |    217.1 μs |  0.65 |    0.06 |   0.4883 |       - |       - |     16 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |        100 |    204.5 μs |   4.07 μs |  11.47 μs |    203.6 μs |  0.59 |    0.04 |   0.4883 |       - |       - |     12 KB |
|                    RepoDbQueryBatchAsync |        100 |    227.2 μs |   5.68 μs |  16.73 μs |    230.5 μs |  0.64 |    0.06 |   0.4883 |       - |       - |     12 KB |
|                    DapperQueryBatchAsync |        100 |    248.5 μs |   5.68 μs |  16.65 μs |    254.4 μs |  0.71 |    0.06 |   0.4883 |       - |       - |     13 KB |
|                                          |            |             |           |           |             |       |         |          |         |         |           |
|                    **EfCoreQueryBatchAsync** |       **1000** |    **937.3 μs** |  **18.08 μs** |  **19.35 μs** |    **934.2 μs** |  **1.00** |    **0.00** |  **13.6719** |       **-** |       **-** |    **264 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |       1000 |  1,064.3 μs |  11.87 μs |  11.10 μs |  1,066.4 μs |  1.13 |    0.03 |  15.6250 |  3.9063 |       - |    289 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |       1000 |  1,036.3 μs |  13.20 μs |  11.71 μs |  1,038.1 μs |  1.10 |    0.02 |  15.6250 |  3.9063 |       - |    292 KB |
|                   VenflowQueryBatchAsync |       1000 |    663.6 μs |  12.53 μs |  19.13 μs |    662.8 μs |  0.71 |    0.03 |   6.8359 |  1.9531 |       - |    136 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |       1000 |    628.6 μs |  12.25 μs |  20.13 μs |    633.4 μs |  0.67 |    0.03 |   4.8828 |  0.9766 |       - |     97 KB |
|                    RepoDbQueryBatchAsync |       1000 |    640.1 μs |  12.56 μs |  23.28 μs |    639.8 μs |  0.69 |    0.03 |   4.8828 |  0.9766 |       - |     97 KB |
|                    DapperQueryBatchAsync |       1000 |    903.0 μs |   9.36 μs |   8.76 μs |    902.5 μs |  0.96 |    0.02 |   5.8594 |  1.9531 |       - |    119 KB |
|                                          |            |             |           |           |             |       |         |          |         |         |           |
|                    **EfCoreQueryBatchAsync** |      **10000** |  **6,106.8 μs** | **121.69 μs** | **113.83 μs** |  **6,142.2 μs** |  **1.00** |    **0.00** | **140.6250** | **23.4375** |  **7.8125** |  **2,684 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |      10000 | 10,618.8 μs | 179.97 μs | 252.30 μs | 10,596.0 μs |  1.76 |    0.05 | 156.2500 | 78.1250 | 31.2500 |  2,920 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |      10000 | 10,733.9 μs | 203.27 μs | 190.14 μs | 10,760.8 μs |  1.76 |    0.04 | 156.2500 | 78.1250 | 31.2500 |  2,923 KB |
|                   VenflowQueryBatchAsync |      10000 |  6,231.1 μs |  84.65 μs |  70.69 μs |  6,247.0 μs |  1.02 |    0.02 |  78.1250 | 46.8750 | 15.6250 |  1,431 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |      10000 |  5,145.9 μs | 100.10 μs | 158.77 μs |  5,163.9 μs |  0.85 |    0.02 |  54.6875 | 31.2500 |  7.8125 |  1,041 KB |
|                    RepoDbQueryBatchAsync |      10000 |  5,376.8 μs | 106.01 μs | 137.85 μs |  5,390.2 μs |  0.87 |    0.03 |  54.6875 | 31.2500 |  7.8125 |  1,041 KB |
|                    DapperQueryBatchAsync |      10000 |  9,277.2 μs | 180.44 μs | 177.22 μs |  9,211.6 μs |  1.52 |    0.03 |  78.1250 | 46.8750 | 15.6250 |  1,274 KB |
