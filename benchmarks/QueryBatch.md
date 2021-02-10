``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                                   Method |           Job |       Runtime | QueryCount |        Mean |     Error |    StdDev |      Median | Ratio | RatioSD |    Gen 0 |   Gen 1 |   Gen 2 |  Allocated |
|----------------------------------------- |-------------- |-------------- |----------- |------------:|----------:|----------:|------------:|------:|--------:|---------:|--------:|--------:|-----------:|
|                    **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |         **10** |    **387.4 μs** |   **7.48 μs** |  **16.57 μs** |    **387.5 μs** |  **1.00** |    **0.00** |   **0.4883** |       **-** |       **-** |   **12.37 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    404.1 μs |   7.92 μs |  15.26 μs |    403.5 μs |  1.05 |    0.06 |   0.4883 |       - |       - |   14.91 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    437.6 μs |   8.57 μs |  12.57 μs |    436.8 μs |  1.13 |    0.06 |   0.9766 |       - |       - |   17.95 KB |
|                   VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    211.8 μs |   4.23 μs |   9.81 μs |    210.9 μs |  0.55 |    0.03 |   0.2441 |       - |       - |    4.89 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    208.2 μs |   4.12 μs |  10.17 μs |    206.4 μs |  0.54 |    0.04 |        - |       - |       - |    4.51 KB |
|                    RepoDbQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    220.0 μs |   3.86 μs |   3.22 μs |    218.6 μs |  0.58 |    0.03 |   0.2441 |       - |       - |    5.56 KB |
|                    DapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    216.7 μs |   4.03 μs |   7.27 μs |    213.5 μs |  0.56 |    0.03 |        - |       - |       - |    4.75 KB |
|                                          |               |               |            |             |           |           |             |       |         |          |         |         |            |
|                    EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    410.9 μs |   8.17 μs |  12.96 μs |    414.1 μs |  1.00 |    0.00 |   0.4883 |       - |       - |   11.55 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    428.2 μs |   8.48 μs |  13.46 μs |    428.9 μs |  1.04 |    0.05 |   0.4883 |       - |       - |   13.11 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    415.5 μs |   8.41 μs |  24.80 μs |    416.6 μs |  1.05 |    0.06 |        - |       - |       - |   14.61 KB |
|                   VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    211.8 μs |   4.20 μs |   9.40 μs |    209.9 μs |  0.51 |    0.03 |        - |       - |       - |    4.73 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    207.6 μs |   4.12 μs |   4.74 μs |    208.2 μs |  0.50 |    0.02 |        - |       - |       - |    4.27 KB |
|                    RepoDbQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    220.2 μs |   4.37 μs |   8.72 μs |    218.4 μs |  0.54 |    0.03 |        - |       - |       - |    5.55 KB |
|                    DapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    221.4 μs |   4.46 μs |  13.01 μs |    220.6 μs |  0.55 |    0.04 |        - |       - |       - |    4.59 KB |
|                                          |               |               |            |             |           |           |             |       |         |          |         |         |            |
|                    **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |        **100** |    **457.2 μs** |   **9.03 μs** |  **22.31 μs** |    **452.2 μs** |  **1.00** |    **0.00** |   **1.9531** |       **-** |       **-** |   **38.81 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    458.8 μs |   8.72 μs |   8.56 μs |    458.1 μs |  1.01 |    0.05 |   1.9531 |       - |       - |    46.5 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    510.6 μs |   9.70 μs |  20.24 μs |    510.1 μs |  1.12 |    0.07 |   1.9531 |       - |       - |   49.29 KB |
|                   VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    296.4 μs |   5.83 μs |  10.22 μs |    296.8 μs |  0.65 |    0.04 |   0.9766 |       - |       - |   20.08 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    281.4 μs |   5.57 μs |  10.19 μs |    281.4 μs |  0.61 |    0.04 |   0.4883 |       - |       - |   16.17 KB |
|                    RepoDbQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    295.7 μs |   5.69 μs |  10.11 μs |    295.0 μs |  0.65 |    0.04 |   0.4883 |       - |       - |   17.22 KB |
|                    DapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    369.8 μs |   3.57 μs |   3.16 μs |    368.9 μs |  0.82 |    0.04 |   0.9766 |       - |       - |   21.34 KB |
|                                          |               |               |            |             |           |           |             |       |         |          |         |         |            |
|                    EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    442.2 μs |   8.81 μs |  16.11 μs |    441.7 μs |  1.00 |    0.00 |   1.9531 |       - |       - |   37.82 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    474.9 μs |   9.06 μs |  10.79 μs |    475.7 μs |  1.06 |    0.06 |   1.9531 |       - |       - |   44.47 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    471.1 μs |   9.14 μs |   8.11 μs |    470.6 μs |  1.03 |    0.03 |   1.9531 |       - |       - |   45.98 KB |
|                   VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    288.7 μs |   5.76 μs |  12.03 μs |    290.3 μs |  0.65 |    0.04 |   0.9766 |       - |       - |   20.03 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    278.8 μs |   5.44 μs |  10.09 μs |    280.8 μs |  0.63 |    0.03 |   0.4883 |       - |       - |   16.12 KB |
|                    RepoDbQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    293.2 μs |   5.08 μs |   5.22 μs |    292.6 μs |  0.65 |    0.03 |   0.4883 |       - |       - |   17.22 KB |
|                    DapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    358.7 μs |   6.95 μs |   8.27 μs |    356.1 μs |  0.80 |    0.03 |   0.9766 |       - |       - |   21.33 KB |
|                                          |               |               |            |             |           |           |             |       |         |          |         |         |            |
|                    **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |       **1000** |  **1,203.7 μs** |  **23.25 μs** |  **31.03 μs** |  **1,210.6 μs** |  **1.00** |    **0.00** |  **15.6250** |       **-** |       **-** |  **299.56 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  1,337.7 μs |  20.29 μs |  16.95 μs |  1,337.4 μs |  1.11 |    0.04 |  17.5781 |  5.8594 |       - |  355.79 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  1,412.4 μs |  28.04 μs |  40.22 μs |  1,410.1 μs |  1.17 |    0.05 |  19.5313 |  5.8594 |       - |  359.38 KB |
|                   VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |    866.4 μs |  16.95 μs |  23.20 μs |    869.4 μs |  0.72 |    0.02 |   7.8125 |  1.9531 |       - |  168.04 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |    804.0 μs |  15.93 μs |  18.96 μs |    803.3 μs |  0.67 |    0.03 |   6.8359 |  1.9531 |       - |  128.99 KB |
|                    RepoDbQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |    854.9 μs |  16.53 μs |  19.68 μs |    853.2 μs |  0.71 |    0.03 |   6.8359 |  1.9531 |       - |  130.05 KB |
|                    DapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  1,367.8 μs |  24.32 μs |  22.75 μs |  1,367.1 μs |  1.14 |    0.04 |   9.7656 |  1.9531 |       - |  183.06 KB |
|                                          |               |               |            |             |           |           |             |       |         |          |         |         |            |
|                    EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  1,156.2 μs |  22.36 μs |  20.92 μs |  1,159.8 μs |  1.00 |    0.00 |  15.6250 |       - |       - |  298.31 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  1,334.2 μs |  20.50 μs |  18.18 μs |  1,335.5 μs |  1.16 |    0.02 |  17.5781 |  5.8594 |       - |  354.17 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  1,277.0 μs |  25.37 μs |  41.68 μs |  1,269.2 μs |  1.09 |    0.04 |  17.5781 |  5.8594 |       - |  355.68 KB |
|                   VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |    833.5 μs |  13.78 μs |  12.89 μs |    830.5 μs |  0.72 |    0.01 |   8.7891 |  2.9297 |       - |     168 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |    772.9 μs |  15.30 μs |  14.31 μs |    775.4 μs |  0.67 |    0.01 |   6.8359 |  1.9531 |       - |  128.94 KB |
|                    RepoDbQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |    814.8 μs |  12.24 μs |  10.85 μs |    815.9 μs |  0.71 |    0.02 |   6.8359 |  1.9531 |       - |  130.03 KB |
|                    DapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  1,323.8 μs |  25.31 μs |  23.68 μs |  1,321.7 μs |  1.15 |    0.02 |   9.7656 |  1.9531 |       - |  183.05 KB |
|                                          |               |               |            |             |           |           |             |       |         |          |         |         |            |
|                    **EfCoreQueryBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |      **10000** |  **8,845.9 μs** | **174.40 μs** | **314.47 μs** |  **8,933.5 μs** |  **1.00** |    **0.00** | **171.8750** | **31.2500** | **15.6250** | **3003.65 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 14,961.6 μs | 298.97 μs | 419.12 μs | 15,042.9 μs |  1.69 |    0.07 | 203.1250 | 93.7500 | 31.2500 | 3552.29 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 15,645.2 μs | 311.75 μs | 359.01 μs | 15,606.1 μs |  1.77 |    0.07 | 218.7500 | 93.7500 | 31.2500 | 3555.51 KB |
|                   VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 |  8,311.0 μs | 165.95 μs | 267.98 μs |  8,271.3 μs |  0.94 |    0.04 |  93.7500 | 46.8750 | 15.6250 | 1747.15 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 |  7,263.4 μs | 143.54 μs | 134.27 μs |  7,285.4 μs |  0.82 |    0.04 |  78.1250 | 46.8750 | 15.6250 | 1356.72 KB |
|                    RepoDbQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 |  7,865.4 μs | 152.72 μs | 198.58 μs |  7,829.4 μs |  0.89 |    0.04 |  85.9375 | 54.6875 | 23.4375 | 1357.58 KB |
|                    DapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 15,634.0 μs | 295.79 μs | 276.68 μs | 15,717.6 μs |  1.77 |    0.07 |  93.7500 | 31.2500 |       - | 1900.55 KB |
|                                          |               |               |            |             |           |           |             |       |         |          |         |         |            |
|                    EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 |  8,789.5 μs | 139.15 μs | 130.16 μs |  8,803.8 μs |  1.00 |    0.00 | 171.8750 | 31.2500 | 15.6250 | 3002.48 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 14,508.8 μs | 278.73 μs | 273.75 μs | 14,514.1 μs |  1.65 |    0.04 | 218.7500 | 93.7500 | 31.2500 | 3550.61 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 14,374.1 μs | 283.55 μs | 303.40 μs | 14,402.3 μs |  1.64 |    0.04 | 218.7500 | 93.7500 | 31.2500 | 3552.13 KB |
|                   VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 |  8,038.9 μs | 150.39 μs | 140.67 μs |  8,002.1 μs |  0.91 |    0.02 |  93.7500 | 46.8750 | 15.6250 | 1747.08 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 |  7,265.0 μs | 136.25 μs | 186.50 μs |  7,242.5 μs |  0.83 |    0.03 |  78.1250 | 54.6875 | 23.4375 | 1357.22 KB |
|                    RepoDbQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 |  7,285.1 μs | 137.44 μs | 163.61 μs |  7,273.8 μs |  0.83 |    0.02 |  78.1250 | 46.8750 | 15.6250 | 1358.23 KB |
|                    DapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 13,588.8 μs | 249.05 μs | 232.96 μs | 13,621.9 μs |  1.55 |    0.03 | 109.3750 | 62.5000 | 15.6250 | 1900.48 KB |
