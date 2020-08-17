``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.388 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.7.20366.6
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4084.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT


```
|                                   Method |           Job |       Runtime | QueryCount |        Mean |     Error |    StdDev |      Median | Ratio | RatioSD |     Gen 0 |    Gen 1 |    Gen 2 |  Allocated |
|----------------------------------------- |-------------- |-------------- |----------- |------------:|----------:|----------:|------------:|------:|--------:|----------:|---------:|---------:|-----------:|
|                    **EfCoreQueryBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |         **10** |    **361.2 μs** |   **6.95 μs** |  **15.53 μs** |    **363.2 μs** |  **1.00** |    **0.00** |    **6.3477** |        **-** |        **-** |   **19.61 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |         10 |    371.2 μs |   7.36 μs |  13.09 μs |    374.9 μs |  1.02 |    0.06 |    6.8359 |        - |        - |   22.22 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |         10 |    391.8 μs |   7.67 μs |  13.44 μs |    395.6 μs |  1.07 |    0.06 |    8.3008 |        - |        - |   25.56 KB |
|                   VenflowQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |         10 |    206.2 μs |   4.00 μs |   3.74 μs |    207.7 μs |  0.57 |    0.02 |    3.1738 |        - |        - |    9.76 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |         10 |    206.7 μs |   3.69 μs |   3.45 μs |    206.2 μs |  0.57 |    0.02 |    2.9297 |        - |        - |    9.36 KB |
|                    RepoDbQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |         10 |    206.3 μs |   4.10 μs |   7.60 μs |    207.3 μs |  0.57 |    0.02 |    2.6855 |        - |        - |    8.56 KB |
|                    DapperQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |         10 |    216.1 μs |   4.21 μs |   4.67 μs |    217.4 μs |  0.60 |    0.02 |    3.1738 |        - |        - |   10.42 KB |
|                                          |               |               |            |             |           |           |             |       |         |           |          |          |            |
|                    EfCoreQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    291.0 μs |   5.73 μs |  12.33 μs |    292.2 μs |  1.00 |    0.00 |    3.4180 |        - |        - |   11.85 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    308.4 μs |   6.12 μs |  13.05 μs |    311.3 μs |  1.06 |    0.07 |    4.3945 |        - |        - |   14.21 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    346.5 μs |   6.90 μs |  13.79 μs |    350.3 μs |  1.19 |    0.06 |    5.3711 |        - |        - |   17.74 KB |
|                   VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    171.6 μs |   2.30 μs |   2.15 μs |    170.9 μs |  0.61 |    0.02 |    1.7090 |        - |        - |    5.79 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    181.7 μs |   3.54 μs |   6.38 μs |    183.2 μs |  0.62 |    0.02 |    1.7090 |        - |        - |     5.4 KB |
|                    RepoDbQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    174.6 μs |   3.48 μs |   3.42 μs |    174.7 μs |  0.62 |    0.03 |    1.4648 |        - |        - |    4.67 KB |
|                    DapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         10 |    173.9 μs |   2.44 μs |   2.16 μs |    174.2 μs |  0.62 |    0.03 |    1.9531 |        - |        - |    6.27 KB |
|                                          |               |               |            |             |           |           |             |       |         |           |          |          |            |
|                    EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    281.9 μs |   5.63 μs |  11.23 μs |    283.1 μs |  1.00 |    0.00 |    3.4180 |        - |        - |   11.15 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    304.1 μs |   6.08 μs |  17.14 μs |    308.2 μs |  1.08 |    0.08 |    3.9063 |        - |        - |   12.88 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    293.0 μs |   5.80 μs |  11.73 μs |    290.5 μs |  1.04 |    0.05 |    4.8828 |        - |        - |   15.01 KB |
|                   VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    172.4 μs |   3.24 μs |   5.04 μs |    170.8 μs |  0.61 |    0.03 |    1.7090 |        - |        - |    5.73 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    173.3 μs |   3.25 μs |   5.78 μs |    171.5 μs |  0.61 |    0.03 |    1.7090 |        - |        - |    5.34 KB |
|                    RepoDbQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    178.4 μs |   3.53 μs |   6.63 μs |    177.5 μs |  0.63 |    0.03 |    1.4648 |        - |        - |    4.66 KB |
|                    DapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         10 |    172.9 μs |   3.24 μs |   3.03 μs |    173.0 μs |  0.61 |    0.03 |    1.9531 |        - |        - |    6.26 KB |
|                                          |               |               |            |             |           |           |             |       |         |           |          |          |            |
|                    **EfCoreQueryBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |        **100** |    **466.9 μs** |   **6.14 μs** |   **5.75 μs** |    **467.5 μs** |  **1.00** |    **0.00** |   **16.6016** |        **-** |        **-** |   **51.15 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |        100 |    509.3 μs |   8.47 μs |   7.93 μs |    509.8 μs |  1.09 |    0.02 |   19.0430 |        - |        - |   58.71 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |        100 |    531.3 μs |   9.10 μs |   8.51 μs |    533.5 μs |  1.14 |    0.02 |   19.5313 |        - |        - |   61.99 KB |
|                   VenflowQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |        100 |    301.3 μs |   5.83 μs |   6.94 μs |    301.9 μs |  0.64 |    0.02 |    9.2773 |        - |        - |   29.27 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |        100 |    292.9 μs |   5.77 μs |   8.81 μs |    292.7 μs |  0.62 |    0.02 |    7.8125 |        - |        - |   25.33 KB |
|                    RepoDbQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |        100 |    296.6 μs |   5.77 μs |   8.98 μs |    297.0 μs |  0.64 |    0.02 |    7.8125 |        - |        - |   24.59 KB |
|                    DapperQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |        100 |    474.7 μs |   8.60 μs |   8.04 μs |    475.0 μs |  1.02 |    0.02 |   16.6016 |        - |        - |    52.5 KB |
|                                          |               |               |            |             |           |           |             |       |         |           |          |          |            |
|                    EfCoreQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    377.4 μs |   6.61 μs |   8.59 μs |    373.7 μs |  1.00 |    0.00 |   12.2070 |        - |        - |   38.28 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    397.9 μs |   7.89 μs |   7.38 μs |    396.1 μs |  1.05 |    0.03 |   14.6484 |        - |        - |   45.52 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    419.8 μs |   5.88 μs |   8.05 μs |    419.9 μs |  1.11 |    0.03 |   15.6250 |        - |        - |   49.05 KB |
|                   VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    226.6 μs |   4.44 μs |   5.11 μs |    226.1 μs |  0.60 |    0.02 |    6.8359 |        - |        - |   20.92 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    221.1 μs |   2.84 μs |   2.79 μs |    220.7 μs |  0.58 |    0.02 |    5.3711 |        - |        - |   17.02 KB |
|                    RepoDbQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    273.2 μs |   5.46 μs |  15.66 μs |    278.8 μs |  0.70 |    0.07 |    5.1270 |        - |        - |   16.29 KB |
|                    DapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        100 |    333.8 μs |   4.88 μs |   4.08 μs |    334.2 μs |  0.88 |    0.02 |   12.6953 |        - |        - |   39.69 KB |
|                                          |               |               |            |             |           |           |             |       |         |           |          |          |            |
|                    EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    407.6 μs |   7.74 μs |   7.60 μs |    409.7 μs |  1.00 |    0.00 |   12.2070 |        - |        - |   37.58 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    414.3 μs |   8.25 μs |  18.95 μs |    412.9 μs |  0.97 |    0.05 |   14.1602 |        - |        - |   44.18 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    429.5 μs |   4.74 μs |   4.20 μs |    430.4 μs |  1.05 |    0.03 |   15.1367 |        - |        - |   46.31 KB |
|                   VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    218.6 μs |   4.35 μs |   5.50 μs |    218.0 μs |  0.54 |    0.01 |    6.8359 |        - |        - |   20.87 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    235.9 μs |   8.40 μs |  24.76 μs |    223.3 μs |  0.61 |    0.06 |    5.3711 |        - |        - |   16.96 KB |
|                    RepoDbQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    221.6 μs |   3.95 μs |   7.62 μs |    219.7 μs |  0.55 |    0.02 |    4.8828 |        - |        - |   16.27 KB |
|                    DapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        100 |    321.9 μs |   3.96 μs |   3.70 μs |    321.1 μs |  0.79 |    0.02 |   12.6953 |        - |        - |   39.67 KB |
|                                          |               |               |            |             |           |           |             |       |         |           |          |          |            |
|                    **EfCoreQueryBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |       **1000** |  **1,409.5 μs** |  **11.61 μs** |  **10.29 μs** |  **1,413.5 μs** |  **1.00** |    **0.00** |  **119.1406** |        **-** |        **-** |  **369.76 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |       1000 |  1,675.4 μs |  10.93 μs |   9.69 μs |  1,678.9 μs |  1.19 |    0.01 |  128.9063 |  33.2031 |        - |  426.47 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |       1000 |  1,714.7 μs |  11.85 μs |  11.09 μs |  1,715.4 μs |  1.22 |    0.01 |  128.9063 |  41.0156 |        - |  430.17 KB |
|                   VenflowQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |       1000 |  1,077.0 μs |  17.51 μs |  16.38 μs |  1,075.3 μs |  0.76 |    0.01 |   56.6406 |  17.5781 |        - |   225.3 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |       1000 |  1,040.2 μs |  14.13 μs |  13.21 μs |  1,042.0 μs |  0.74 |    0.01 |   54.6875 |  17.5781 |        - |  186.21 KB |
|                    RepoDbQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |       1000 |  1,071.8 μs |  20.04 μs |  18.74 μs |  1,074.4 μs |  0.76 |    0.02 |   54.6875 |  15.6250 |        - |  185.59 KB |
|                    DapperQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |       1000 |  1,928.6 μs |  18.41 μs |  17.22 μs |  1,922.4 μs |  1.37 |    0.01 |  140.6250 |  41.0156 |        - |  469.23 KB |
|                                          |               |               |            |             |           |           |             |       |         |           |          |          |            |
|                    EfCoreQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |    986.9 μs |  19.54 μs |  35.24 μs |    998.0 μs |  1.00 |    0.00 |   97.6563 |        - |        - |  299.08 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  1,119.7 μs |  21.78 μs |  20.38 μs |  1,124.3 μs |  1.13 |    0.05 |  107.4219 |  35.1563 |        - |  355.54 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  1,146.3 μs |  22.18 μs |  22.77 μs |  1,150.9 μs |  1.16 |    0.05 |  107.4219 |  35.1563 |        - |  359.11 KB |
|                   VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |    665.2 μs |  14.38 μs |  42.41 μs |    661.2 μs |  0.67 |    0.05 |   43.9453 |  13.6719 |        - |  169.17 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |    615.0 μs |  16.61 μs |  48.98 μs |    610.3 μs |  0.60 |    0.03 |   39.0625 |  12.6953 |        - |  130.11 KB |
|                    RepoDbQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |    643.0 μs |  16.29 μs |  48.03 μs |    643.7 μs |  0.66 |    0.04 |   41.0156 |  13.6719 |        - |   129.4 KB |
|                    DapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       1000 |  1,085.0 μs |  20.42 μs |  19.10 μs |  1,087.3 μs |  1.09 |    0.04 |  113.2813 |  37.1094 |        - |  370.16 KB |
|                                          |               |               |            |             |           |           |             |       |         |           |          |          |            |
|                    EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |    910.9 μs |  18.05 μs |  37.68 μs |    890.2 μs |  1.00 |    0.00 |   96.6797 |        - |        - |  298.35 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  1,059.5 μs |  21.14 μs |  42.23 μs |  1,075.4 μs |  1.16 |    0.07 |  111.3281 |  37.1094 |        - |  354.17 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  1,055.4 μs |  20.96 μs |  31.37 μs |  1,046.3 μs |  1.15 |    0.07 |  105.4688 |  35.1563 |        - |  356.31 KB |
|                   VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |    597.7 μs |  11.93 μs |  33.65 μs |    598.2 μs |  0.66 |    0.05 |   49.8047 |   5.8594 |        - |  169.12 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |    583.8 μs |   7.86 μs |   6.56 μs |    585.3 μs |  0.64 |    0.02 |   39.0625 |  12.6953 |        - |  130.06 KB |
|                    RepoDbQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |    603.4 μs |  11.69 μs |  16.00 μs |    601.1 μs |  0.66 |    0.03 |   40.0391 |  12.6953 |        - |   129.4 KB |
|                    DapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       1000 |  1,078.1 μs |  20.68 μs |  28.30 μs |  1,081.1 μs |  1.18 |    0.06 |  113.2813 |  37.1094 |        - |  370.15 KB |
|                                          |               |               |            |             |           |           |             |       |         |           |          |          |            |
|                    **EfCoreQueryBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |      **10000** | **10,122.3 μs** | **176.26 μs** | **164.87 μs** | **10,123.7 μs** |  **1.00** |    **0.00** | **1156.2500** |  **31.2500** |  **15.6250** |  **3650.3 KB** |
|    EfCoreQueryBatchNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |      10000 | 17,180.4 μs | 322.90 μs | 331.59 μs | 17,125.4 μs |  1.69 |    0.04 |  781.2500 | 281.2500 |  93.7500 | 4270.14 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |      10000 | 17,226.7 μs | 338.67 μs | 316.79 μs | 17,282.9 μs |  1.70 |    0.05 |  781.2500 | 281.2500 |  93.7500 | 4278.33 KB |
|                   VenflowQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |      10000 | 13,808.8 μs | 198.00 μs | 185.21 μs | 13,871.4 μs |  1.36 |    0.03 |  406.2500 | 187.5000 |  62.5000 | 2350.79 KB |
|   VenflowQueryBatchNoChangeTrackingAsync |      .NET 4.8 |      .NET 4.8 |      10000 | 11,315.8 μs | 220.72 μs | 206.46 μs | 11,292.8 μs |  1.12 |    0.02 |  328.1250 | 140.6250 |  46.8750 |  1957.8 KB |
|                    RepoDbQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |      10000 | 11,628.1 μs | 223.75 μs | 266.36 μs | 11,590.5 μs |  1.15 |    0.03 |  312.5000 | 125.0000 |  31.2500 |  1956.6 KB |
|                    DapperQueryBatchAsync |      .NET 4.8 |      .NET 4.8 |      10000 | 21,742.5 μs | 427.89 μs | 334.07 μs | 21,712.7 μs |  2.15 |    0.05 |  843.7500 | 281.2500 |  93.7500 | 4827.16 KB |
|                                          |               |               |            |             |           |           |             |       |         |           |          |          |            |
|                    EfCoreQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 |  5,787.2 μs | 112.46 μs | 105.20 μs |  5,800.7 μs |  1.00 |    0.00 |  960.9375 |  46.8750 |  23.4375 | 3006.64 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 11,786.1 μs | 234.38 μs | 260.51 μs | 11,838.2 μs |  2.04 |    0.07 |  671.8750 | 250.0000 | 109.3750 |  3554.7 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 11,599.8 μs | 222.67 μs | 289.54 μs | 11,567.7 μs |  2.00 |    0.07 |  671.8750 | 250.0000 | 109.3750 | 3558.55 KB |
|                   VenflowQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 |  7,269.8 μs | 109.40 μs | 102.33 μs |  7,253.4 μs |  1.26 |    0.03 |  289.0625 | 132.8125 |  46.8750 | 1750.98 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 |  6,115.6 μs | 120.85 μs | 177.14 μs |  6,143.8 μs |  1.05 |    0.04 |  234.3750 | 109.3750 |  31.2500 | 1360.48 KB |
|                    RepoDbQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 |  6,182.5 μs | 121.68 μs | 196.49 μs |  6,208.9 μs |  1.07 |    0.04 |  234.3750 | 109.3750 |  39.0625 | 1360.14 KB |
|                    DapperQueryBatchAsync | .NET Core 3.1 | .NET Core 3.1 |      10000 | 12,964.9 μs | 188.96 μs | 176.75 μs | 12,995.5 μs |  2.24 |    0.06 |  687.5000 | 250.0000 |  93.7500 | 3775.55 KB |
|                                          |               |               |            |             |           |           |             |       |         |           |          |          |            |
|                    EfCoreQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 |  5,283.8 μs |  86.65 μs |  81.05 μs |  5,273.0 μs |  1.00 |    0.00 |  960.9375 |  46.8750 |  23.4375 |  3005.2 KB |
|    EfCoreQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 10,604.1 μs |  75.01 μs |  58.56 μs | 10,611.3 μs |  2.01 |    0.03 |  656.2500 | 250.0000 | 109.3750 | 3553.19 KB |
| EfCoreQueryBatchRawNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 10,667.2 μs | 113.69 μs | 106.35 μs | 10,618.2 μs |  2.02 |    0.04 |  656.2500 | 250.0000 | 109.3750 | 3555.32 KB |
|                   VenflowQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 |  6,358.3 μs | 124.11 μs | 121.90 μs |  6,346.3 μs |  1.20 |    0.03 |  281.2500 | 132.8125 |  46.8750 | 1750.87 KB |
|   VenflowQueryBatchNoChangeTrackingAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 |  5,600.0 μs | 109.75 μs | 102.66 μs |  5,594.4 μs |  1.06 |    0.03 |  226.5625 | 109.3750 |  31.2500 | 1364.06 KB |
|                    RepoDbQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 |  5,557.8 μs |  65.04 μs |  60.83 μs |  5,535.5 μs |  1.05 |    0.02 |  226.5625 | 101.5625 |  31.2500 | 1362.17 KB |
|                    DapperQueryBatchAsync | .NET Core 5.0 | .NET Core 5.0 |      10000 | 11,366.7 μs | 182.20 μs | 170.43 μs | 11,364.1 μs |  2.15 |    0.05 |  687.5000 | 250.0000 |  93.7500 | 3775.32 KB |
