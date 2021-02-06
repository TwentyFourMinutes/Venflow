``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                                   Method |           Job |       Runtime | QueryCount |        Mean |     Error |    StdDev | Ratio | RatioSD |    Gen 0 |   Gen 1 |   Gen 2 |  Allocated |
|----------------------------------------- |-------------- |-------------- |----------- |------------:|----------:|----------:|------:|--------:|---------:|--------:|--------:|-----------:|
|                    **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |         **10** |    **436.2 μs** |   **8.63 μs** |  **16.42 μs** |  **1.00** |    **0.00** |   **0.4883** |       **-** |       **-** |   **12.38 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    465.3 μs |   8.84 μs |   8.68 μs |  1.05 |    0.05 |   0.4883 |       - |       - |   15.07 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    492.8 μs |   8.73 μs |  15.29 μs |  1.13 |    0.06 |   0.9766 |       - |       - |   18.11 KB |
|                   VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    240.0 μs |   4.68 μs |   8.45 μs |  0.55 |    0.03 |        - |       - |       - |    4.89 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    238.3 μs |   4.74 μs |   9.24 μs |  0.55 |    0.03 |   0.2441 |       - |       - |     4.5 KB |
|                    RepoDbQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    245.9 μs |   4.77 μs |   6.03 μs |  0.56 |    0.03 |        - |       - |       - |    5.55 KB |
|                    DapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    245.8 μs |   4.89 μs |   6.70 μs |  0.56 |    0.03 |   0.2441 |       - |       - |    4.75 KB |
|                                          |               |               |            |             |           |           |       |         |          |         |         |            |
|                    EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    445.5 μs |   8.86 μs |  10.21 μs |  1.00 |    0.00 |   0.4883 |       - |       - |   11.38 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    454.5 μs |   9.06 μs |  19.51 μs |  1.01 |    0.05 |   0.4883 |       - |       - |   13.13 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    450.8 μs |   8.98 μs |  17.94 μs |  1.01 |    0.05 |        - |       - |       - |    14.6 KB |
|                   VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    234.9 μs |   4.66 μs |   6.54 μs |  0.53 |    0.02 |   0.2441 |       - |       - |    4.85 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    232.2 μs |   4.62 μs |  11.67 μs |  0.53 |    0.02 |        - |       - |       - |    4.35 KB |
|                    RepoDbQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    252.8 μs |   5.05 μs |   9.60 μs |  0.57 |    0.02 |        - |       - |       - |    5.54 KB |
|                    DapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    246.7 μs |   4.81 μs |  13.64 μs |  0.55 |    0.03 |        - |       - |       - |    4.69 KB |
|                                          |               |               |            |             |           |           |       |         |          |         |         |            |
|                    **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |        **100** |    **503.2 μs** |   **9.98 μs** |  **23.71 μs** |  **1.00** |    **0.00** |   **1.9531** |       **-** |       **-** |   **38.82 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    532.2 μs |  10.64 μs |  22.22 μs |  1.06 |    0.07 |   1.9531 |       - |       - |   46.44 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    542.3 μs |   9.74 μs |  13.66 μs |  1.06 |    0.06 |   1.9531 |       - |       - |   50.16 KB |
|                   VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    322.4 μs |   4.61 μs |   4.31 μs |  0.64 |    0.03 |   0.9766 |       - |       - |   20.07 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    312.9 μs |   5.99 μs |   8.00 μs |  0.62 |    0.03 |   0.4883 |       - |       - |   16.16 KB |
|                    RepoDbQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    318.0 μs |   6.25 μs |   7.90 μs |  0.63 |    0.03 |   0.4883 |       - |       - |   17.22 KB |
|                    DapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    392.1 μs |   5.24 μs |   4.38 μs |  0.77 |    0.04 |   0.9766 |       - |       - |   21.33 KB |
|                                          |               |               |            |             |           |           |       |         |          |         |         |            |
|                    EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    501.7 μs |   9.28 μs |  16.26 μs |  1.00 |    0.00 |   1.9531 |       - |       - |   37.82 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    523.7 μs |  10.18 μs |  15.86 μs |  1.04 |    0.05 |   1.9531 |       - |       - |   44.47 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    521.0 μs |  10.30 μs |  15.10 μs |  1.04 |    0.04 |   1.9531 |       - |       - |   45.98 KB |
|                   VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    326.3 μs |   6.42 μs |   9.00 μs |  0.65 |    0.03 |   0.9766 |       - |       - |   20.03 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    319.3 μs |   6.23 μs |  10.41 μs |  0.64 |    0.03 |   0.4883 |       - |       - |   16.12 KB |
|                    RepoDbQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    327.1 μs |   5.71 μs |   8.19 μs |  0.65 |    0.02 |   0.4883 |       - |       - |   17.11 KB |
|                    DapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    387.6 μs |   6.45 μs |   6.62 μs |  0.77 |    0.03 |   0.9766 |       - |       - |   21.31 KB |
|                                          |               |               |            |             |           |           |       |         |          |         |         |            |
|                    **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |       **1000** |  **1,287.6 μs** |  **16.70 μs** |  **13.95 μs** |  **1.00** |    **0.00** |  **15.6250** |       **-** |       **-** |  **299.33 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  1,446.1 μs |  22.14 μs |  22.74 μs |  1.13 |    0.02 |  17.5781 |  5.8594 |       - |  355.79 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  1,503.9 μs |  17.24 μs |  16.13 μs |  1.17 |    0.02 |  19.5313 |  5.8594 |       - |  359.88 KB |
|                   VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |    925.3 μs |  18.23 μs |  20.99 μs |  0.72 |    0.02 |   8.7891 |  2.9297 |       - |  168.05 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |    857.7 μs |  14.23 μs |  12.62 μs |  0.66 |    0.01 |   6.8359 |  1.9531 |       - |  128.98 KB |
|                    RepoDbQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |    898.5 μs |  17.18 μs |  18.38 μs |  0.70 |    0.01 |   6.8359 |  1.9531 |       - |  130.05 KB |
|                    DapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  1,461.3 μs |  28.76 μs |  35.32 μs |  1.14 |    0.04 |   9.7656 |  1.9531 |       - |  183.06 KB |
|                                          |               |               |            |             |           |           |       |         |          |         |         |            |
|                    EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  1,323.6 μs |  23.82 μs |  22.28 μs |  1.00 |    0.00 |  15.6250 |       - |       - |   298.3 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  1,435.9 μs |  26.71 μs |  23.68 μs |  1.09 |    0.03 |  17.5781 |  5.8594 |       - |  354.03 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  1,359.0 μs |  17.02 μs |  14.21 μs |  1.03 |    0.02 |  17.5781 |  5.8594 |       - |  356.03 KB |
|                   VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |    859.0 μs |   9.80 μs |   8.19 μs |  0.65 |    0.01 |   8.7891 |  2.9297 |       - |     168 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |    788.0 μs |  15.74 μs |  17.49 μs |  0.60 |    0.01 |   6.8359 |  1.9531 |       - |  128.94 KB |
|                    RepoDbQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |    844.6 μs |  15.01 μs |  14.04 μs |  0.64 |    0.02 |   6.8359 |  1.9531 |       - |  130.04 KB |
|                    DapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  1,273.2 μs |  25.26 μs |  38.58 μs |  0.97 |    0.03 |   9.7656 |  1.9531 |       - |  183.05 KB |
|                                          |               |               |            |             |           |           |       |         |          |         |         |            |
|                    **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |      **10000** |  **8,670.1 μs** | **170.10 μs** | **274.68 μs** |  **1.00** |    **0.00** | **171.8750** | **31.2500** | **15.6250** | **3003.55 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 14,235.4 μs | 277.94 μs | 259.99 μs |  1.66 |    0.06 | 203.1250 | 93.7500 | 31.2500 | 3552.23 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 14,475.4 μs | 272.50 μs | 254.90 μs |  1.69 |    0.07 | 203.1250 | 93.7500 | 31.2500 | 3555.77 KB |
|                   VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 |  8,089.9 μs | 142.91 μs | 126.69 μs |  0.94 |    0.03 |  93.7500 | 46.8750 | 15.6250 | 1747.12 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 |  7,398.1 μs | 139.00 μs | 165.47 μs |  0.86 |    0.03 |  78.1250 | 54.6875 | 23.4375 | 1356.69 KB |
|                    RepoDbQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 |  7,601.0 μs | 151.10 μs | 216.71 μs |  0.88 |    0.04 |  78.1250 | 46.8750 | 15.6250 | 1357.55 KB |
|                    DapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 14,880.6 μs | 287.24 μs | 352.76 μs |  1.73 |    0.08 |  93.7500 | 31.2500 |       - | 1900.46 KB |
|                                          |               |               |            |             |           |           |       |         |          |         |         |            |
|                    EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 |  8,760.3 μs | 171.35 μs | 168.29 μs |  1.00 |    0.00 | 171.8750 | 31.2500 | 15.6250 | 3002.49 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 13,975.0 μs | 271.98 μs | 302.30 μs |  1.60 |    0.03 | 203.1250 | 93.7500 | 31.2500 | 3550.46 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 14,266.8 μs | 281.45 μs | 562.09 μs |  1.60 |    0.07 | 218.7500 | 93.7500 | 31.2500 | 3552.09 KB |
|                   VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 |  7,748.2 μs | 140.83 μs | 117.60 μs |  0.88 |    0.03 |  93.7500 | 46.8750 | 15.6250 | 1747.29 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 |  7,147.4 μs | 137.08 μs | 140.77 μs |  0.82 |    0.02 |  78.1250 | 54.6875 | 23.4375 | 1356.87 KB |
|                    RepoDbQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 |  7,308.9 μs | 141.88 μs | 194.21 μs |  0.83 |    0.03 |  78.1250 | 46.8750 | 15.6250 | 1357.58 KB |
|                    DapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 14,679.7 μs | 283.31 μs | 303.14 μs |  1.68 |    0.05 | 109.3750 | 62.5000 | 15.6250 | 1900.45 KB |
