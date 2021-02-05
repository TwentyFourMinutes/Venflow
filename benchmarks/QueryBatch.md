``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                                   Method |           Job |       Runtime | QueryCount |        Mean |     Error |      StdDev |      Median | Ratio | RatioSD |    Gen 0 |   Gen 1 |   Gen 2 |  Allocated |
|----------------------------------------- |-------------- |-------------- |----------- |------------:|----------:|------------:|------------:|------:|--------:|---------:|--------:|--------:|-----------:|
|                    **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |         **10** |    **427.9 μs** |   **8.54 μs** |     **7.57 μs** |    **427.1 μs** |  **1.00** |    **0.00** |        **-** |       **-** |       **-** |   **12.38 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    439.4 μs |   8.11 μs |    12.63 μs |    440.5 μs |  1.03 |    0.04 |        - |       - |       - |   15.42 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    464.6 μs |   8.98 μs |    10.69 μs |    466.7 μs |  1.09 |    0.03 |        - |       - |       - |   18.96 KB |
|                   VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    245.0 μs |   4.86 μs |     9.92 μs |    242.7 μs |  0.58 |    0.03 |        - |       - |       - |     4.9 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    245.4 μs |   4.89 μs |     8.16 μs |    245.9 μs |  0.58 |    0.02 |        - |       - |       - |     4.5 KB |
|                    RepoDbQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    255.3 μs |   5.10 μs |    11.72 μs |    252.1 μs |  0.60 |    0.03 |        - |       - |       - |    5.53 KB |
|                    DapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    251.3 μs |   5.01 μs |     8.90 μs |    252.1 μs |  0.58 |    0.03 |        - |       - |       - |    4.73 KB |
|                                          |               |               |            |             |           |             |             |       |         |          |         |         |            |
|                    EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    427.8 μs |   8.49 μs |    19.50 μs |    427.5 μs |  1.00 |    0.00 |        - |       - |       - |   11.37 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    436.7 μs |   8.72 μs |    12.51 μs |    436.0 μs |  1.00 |    0.06 |        - |       - |       - |   13.44 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    439.2 μs |   8.75 μs |    20.45 μs |    440.1 μs |  1.03 |    0.07 |        - |       - |       - |   14.63 KB |
|                   VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    249.2 μs |   4.88 μs |    11.20 μs |    248.4 μs |  0.58 |    0.04 |        - |       - |       - |    4.78 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    246.9 μs |   4.80 μs |    12.05 μs |    246.2 μs |  0.58 |    0.03 |        - |       - |       - |    4.31 KB |
|                    RepoDbQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    261.4 μs |   5.23 μs |    14.57 μs |    260.6 μs |  0.62 |    0.04 |        - |       - |       - |    5.53 KB |
|                    DapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    254.3 μs |   5.06 μs |    10.22 μs |    251.7 μs |  0.59 |    0.04 |        - |       - |       - |    4.72 KB |
|                                          |               |               |            |             |           |             |             |       |         |          |         |         |            |
|                    **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |        **100** |    **474.8 μs** |   **9.46 μs** |    **18.46 μs** |    **473.5 μs** |  **1.00** |    **0.00** |   **0.9766** |       **-** |       **-** |   **38.81 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    498.2 μs |   9.87 μs |    11.75 μs |    497.1 μs |  1.04 |    0.05 |   1.4648 |       - |       - |   46.78 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    528.9 μs |  10.33 μs |    11.48 μs |    530.7 μs |  1.11 |    0.06 |   0.9766 |       - |       - |   49.65 KB |
|                   VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    306.2 μs |   5.89 μs |     4.92 μs |    305.8 μs |  0.64 |    0.03 |   0.4883 |       - |       - |   20.08 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    301.9 μs |   5.91 μs |    11.95 μs |    300.2 μs |  0.64 |    0.04 |   0.4883 |       - |       - |   16.16 KB |
|                    RepoDbQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    338.7 μs |  10.37 μs |    30.42 μs |    332.6 μs |  0.68 |    0.05 |   0.4883 |       - |       - |    17.2 KB |
|                    DapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    475.5 μs |   9.46 μs |    23.21 μs |    471.8 μs |  1.00 |    0.07 |        - |       - |       - |   21.33 KB |
|                                          |               |               |            |             |           |             |             |       |         |          |         |         |            |
|                    EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    553.4 μs |  11.05 μs |    19.93 μs |    554.6 μs |  1.00 |    0.00 |   0.9766 |       - |       - |   37.99 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    586.9 μs |  11.54 μs |    16.92 μs |    587.8 μs |  1.05 |    0.04 |   0.9766 |       - |       - |   44.47 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    576.0 μs |  11.45 μs |    16.42 μs |    574.3 μs |  1.03 |    0.05 |   0.9766 |       - |       - |   45.98 KB |
|                   VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    366.5 μs |   7.24 μs |    16.49 μs |    367.0 μs |  0.66 |    0.04 |   0.4883 |       - |       - |   20.03 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    372.4 μs |   7.41 μs |    16.43 μs |    368.1 μs |  0.68 |    0.04 |   0.4883 |       - |       - |   16.12 KB |
|                    RepoDbQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    384.6 μs |   7.47 μs |    13.66 μs |    380.9 μs |  0.70 |    0.04 |   0.4883 |       - |       - |    17.2 KB |
|                    DapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    452.8 μs |   8.65 μs |     9.61 μs |    454.7 μs |  0.81 |    0.04 |   0.4883 |       - |       - |   21.33 KB |
|                                          |               |               |            |             |           |             |             |       |         |          |         |         |            |
|                    **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |       **1000** |  **1,466.0 μs** |  **28.83 μs** |    **35.41 μs** |  **1,460.2 μs** |  **1.00** |    **0.00** |  **11.7188** |       **-** |       **-** |  **299.28 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  1,657.4 μs |  32.94 μs |    75.69 μs |  1,658.2 μs |  1.10 |    0.06 |  13.6719 |  3.9063 |       - |  355.81 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  1,709.2 μs |  33.13 μs |    35.45 μs |  1,700.2 μs |  1.16 |    0.04 |  13.6719 |  3.9063 |       - |  360.04 KB |
|                   VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  1,081.8 μs |  20.67 μs |    28.29 μs |  1,079.1 μs |  0.74 |    0.03 |   5.8594 |  1.9531 |       - |  168.05 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |    999.7 μs |  19.71 μs |    23.47 μs |    988.3 μs |  0.68 |    0.02 |   3.9063 |       - |       - |  128.99 KB |
|                    RepoDbQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  1,054.1 μs |  20.68 μs |    24.61 μs |  1,054.0 μs |  0.72 |    0.02 |   3.9063 |       - |       - |  130.05 KB |
|                    DapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  1,580.0 μs |  29.72 μs |    30.52 μs |  1,580.1 μs |  1.08 |    0.04 |   5.8594 |  1.9531 |       - |  183.06 KB |
|                                          |               |               |            |             |           |             |             |       |         |          |         |         |            |
|                    EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  1,300.9 μs |  24.03 μs |    47.99 μs |  1,293.7 μs |  1.00 |    0.00 |   9.7656 |       - |       - |   298.3 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  1,767.0 μs |  41.16 μs |   120.07 μs |  1,736.8 μs |  1.33 |    0.11 |  13.6719 |  3.9063 |       - |  354.17 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  1,723.0 μs |  34.38 μs |    79.68 μs |  1,718.5 μs |  1.32 |    0.07 |  13.6719 |  3.9063 |       - |  355.68 KB |
|                   VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  1,063.6 μs |  20.36 μs |    47.18 μs |  1,053.5 μs |  0.82 |    0.05 |   5.8594 |  1.9531 |       - |     168 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |    988.4 μs |  19.37 μs |    36.39 μs |    982.0 μs |  0.76 |    0.04 |   3.9063 |       - |       - |  128.95 KB |
|                    RepoDbQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |    922.5 μs |  17.73 μs |    22.42 μs |    916.0 μs |  0.71 |    0.03 |   3.9063 |       - |       - |  130.03 KB |
|                    DapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  1,375.8 μs |  25.89 μs |    40.31 μs |  1,370.6 μs |  1.05 |    0.05 |   5.8594 |  1.9531 |       - |  183.05 KB |
|                                          |               |               |            |             |           |             |             |       |         |          |         |         |            |
|                    **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |      **10000** |  **8,804.5 μs** | **174.84 μs** |   **227.35 μs** |  **8,762.6 μs** |  **1.00** |    **0.00** | **125.0000** | **31.2500** | **15.6250** | **3003.42 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 14,119.4 μs | 276.36 μs |   387.42 μs | 14,271.4 μs |  1.60 |    0.06 | 156.2500 | 78.1250 | 31.2500 | 3552.77 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 14,164.1 μs | 282.78 μs |   367.70 μs | 14,139.2 μs |  1.61 |    0.06 | 156.2500 | 78.1250 | 31.2500 | 3555.68 KB |
|                   VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 |  8,661.4 μs | 164.51 μs |   168.94 μs |  8,645.0 μs |  0.98 |    0.04 |  78.1250 | 46.8750 | 15.6250 | 1747.19 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 |  7,146.8 μs |  89.71 μs |    70.04 μs |  7,160.8 μs |  0.81 |    0.03 |  70.3125 | 46.8750 | 23.4375 | 1356.62 KB |
|                    RepoDbQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 |  7,453.5 μs | 130.78 μs |   128.44 μs |  7,446.4 μs |  0.84 |    0.02 |  70.3125 | 46.8750 | 23.4375 | 1357.61 KB |
|                    DapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 14,031.3 μs | 279.54 μs |   261.49 μs | 14,099.5 μs |  1.59 |    0.05 |  78.1250 | 46.8750 | 15.6250 | 1900.53 KB |
|                                          |               |               |            |             |           |             |             |       |         |          |         |         |            |
|                    EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 10,699.9 μs | 207.49 μs |   213.07 μs | 10,712.6 μs |  1.00 |    0.00 | 125.0000 | 31.2500 | 15.6250 | 3002.49 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 19,153.4 μs | 381.34 μs | 1,124.40 μs | 18,764.7 μs |  1.76 |    0.11 | 125.0000 | 31.2500 |       - | 3550.57 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 18,812.8 μs | 470.38 μs | 1,386.92 μs | 18,911.6 μs |  1.77 |    0.11 | 125.0000 | 31.2500 |       - | 3551.91 KB |
|                   VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 10,171.7 μs | 202.36 μs |   529.54 μs | 10,114.9 μs |  0.96 |    0.05 |  78.1250 | 46.8750 | 15.6250 | 1747.12 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 10,580.9 μs | 211.47 μs |   592.98 μs | 10,475.2 μs |  0.99 |    0.05 |  62.5000 | 46.8750 | 15.6250 | 1356.86 KB |
|                    RepoDbQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 10,736.7 μs | 211.50 μs |   502.66 μs | 10,670.0 μs |  0.99 |    0.06 |  62.5000 | 46.8750 | 15.6250 | 1358.08 KB |
|                    DapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 16,329.6 μs | 405.44 μs | 1,182.69 μs | 16,290.5 μs |  1.58 |    0.09 |  62.5000 | 31.2500 |       - | 1900.47 KB |
