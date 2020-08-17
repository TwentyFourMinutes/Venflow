``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.388 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.7.20366.6
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4084.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT


```
|                  Method |           Job |       Runtime | DeleteCount |         Mean |      Error |      StdDev |       Median | Ratio | RatioSD |      Gen 0 |      Gen 1 |     Gen 2 |    Allocated |
|------------------------ |-------------- |-------------- |------------ |-------------:|-----------:|------------:|-------------:|------:|--------:|-----------:|-----------:|----------:|-------------:|
|  **EFCoreDeleteBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |          **10** |     **4.403 ms** |  **0.0818 ms** |   **0.0765 ms** |     **4.400 ms** |  **1.00** |    **0.00** |    **39.0625** |          **-** |         **-** |    **126.19 KB** |
| VenflowDeleteBatchAsync |      .NET 4.8 |      .NET 4.8 |          10 |     2.848 ms |  0.0567 ms |   0.0653 ms |     2.838 ms |  0.64 |    0.02 |     7.8125 |          - |         - |     27.94 KB |
|  RepoDbDeleteBatchAsync |      .NET 4.8 |      .NET 4.8 |          10 |     3.453 ms |  0.1024 ms |   0.3002 ms |     3.348 ms |  0.78 |    0.08 |    11.7188 |          - |         - |     47.61 KB |
|                         |               |               |             |              |            |             |              |       |         |            |            |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     4.304 ms |  0.0784 ms |   0.0695 ms |     4.297 ms |  1.00 |    0.00 |    31.2500 |          - |         - |    106.81 KB |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     2.595 ms |  0.1668 ms |   0.4918 ms |     2.292 ms |  0.71 |    0.09 |     3.9063 |          - |         - |     18.98 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     2.551 ms |  0.0504 ms |   0.0933 ms |     2.551 ms |  0.58 |    0.02 |     7.8125 |          - |         - |     34.03 KB |
|                         |               |               |             |              |            |             |              |       |         |            |            |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     3.968 ms |  0.1227 ms |   0.3539 ms |     3.772 ms |  1.00 |    0.00 |    27.3438 |          - |         - |     94.98 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     3.602 ms |  0.1569 ms |   0.4576 ms |     3.581 ms |  0.92 |    0.15 |     3.9063 |          - |         - |     18.96 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     3.701 ms |  0.1821 ms |   0.5254 ms |     3.438 ms |  0.94 |    0.15 |     7.8125 |          - |         - |     32.84 KB |
|                         |               |               |             |              |            |             |              |       |         |            |            |           |              |
|  **EFCoreDeleteBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |         **100** |    **22.046 ms** |  **0.6665 ms** |   **1.8908 ms** |    **21.506 ms** |  **1.00** |    **0.00** |   **343.7500** |    **93.7500** |         **-** |   **1103.52 KB** |
| VenflowDeleteBatchAsync |      .NET 4.8 |      .NET 4.8 |         100 |    10.248 ms |  0.6736 ms |   1.9649 ms |     9.170 ms |  0.47 |    0.10 |    46.8750 |          - |         - |    164.58 KB |
|  RepoDbDeleteBatchAsync |      .NET 4.8 |      .NET 4.8 |         100 |    11.886 ms |  0.7692 ms |   2.2437 ms |    11.766 ms |  0.54 |    0.11 |    93.7500 |    15.6250 |         - |     301.9 KB |
|                         |               |               |             |              |            |             |              |       |         |            |            |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |    21.699 ms |  0.8182 ms |   2.3077 ms |    21.101 ms |  1.00 |    0.00 |   312.5000 |    62.5000 |         - |   1007.98 KB |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |    12.997 ms |  0.7246 ms |   2.0791 ms |    13.145 ms |  0.60 |    0.12 |    31.2500 |          - |         - |    140.76 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |    14.542 ms |  0.6658 ms |   1.9421 ms |    13.904 ms |  0.68 |    0.12 |    62.5000 |          - |         - |    237.47 KB |
|                         |               |               |             |              |            |             |              |       |         |            |            |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |    19.490 ms |  0.3877 ms |   0.4149 ms |    19.502 ms |  1.00 |    0.00 |   218.7500 |    62.5000 |         - |    848.77 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |    10.137 ms |  0.6465 ms |   1.9063 ms |    10.809 ms |  0.60 |    0.07 |    31.2500 |          - |         - |    140.76 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     7.979 ms |  0.1590 ms |   0.3900 ms |     7.850 ms |  0.43 |    0.02 |    46.8750 |          - |         - |    188.86 KB |
|                         |               |               |             |              |            |             |              |       |         |            |            |           |              |
|  **EFCoreDeleteBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |        **1000** |   **166.824 ms** |  **2.2311 ms** |   **2.2912 ms** |   **166.487 ms** |  **1.00** |    **0.00** |  **2000.0000** |   **666.6667** |         **-** |  **12394.16 KB** |
| VenflowDeleteBatchAsync |      .NET 4.8 |      .NET 4.8 |        1000 |    63.511 ms |  1.1576 ms |   1.4216 ms |    63.369 ms |  0.38 |    0.01 |   250.0000 |          - |         - |    1531.5 KB |
|  RepoDbDeleteBatchAsync |      .NET 4.8 |      .NET 4.8 |        1000 |    84.472 ms |  1.6081 ms |   1.6514 ms |    83.915 ms |  0.51 |    0.01 |  1166.6667 |   500.0000 |         - |   7489.92 KB |
|                         |               |               |             |              |            |             |              |       |         |            |            |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |   151.512 ms |  2.9767 ms |   2.9236 ms |   151.444 ms |  1.00 |    0.00 |  2000.0000 |   666.6667 |         - |  11604.03 KB |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |    63.746 ms |  0.6964 ms |   0.5816 ms |    63.831 ms |  0.42 |    0.01 |   250.0000 |   125.0000 |         - |   1355.34 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |    80.695 ms |  1.1702 ms |   0.9771 ms |    80.248 ms |  0.53 |    0.01 |  1166.6667 |   500.0000 |         - |   6909.58 KB |
|                         |               |               |             |              |            |             |              |       |         |            |            |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |   150.500 ms |  2.9931 ms |   3.4468 ms |   150.172 ms |  1.00 |    0.00 |  1500.0000 |   500.0000 |         - |    8306.2 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    64.014 ms |  0.9058 ms |   1.0783 ms |    64.097 ms |  0.43 |    0.01 |   250.0000 |   125.0000 |         - |   1355.43 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    79.843 ms |  1.5249 ms |   1.3517 ms |    79.655 ms |  0.53 |    0.01 |  1142.8571 |   142.8571 |         - |   6413.12 KB |
|                         |               |               |             |              |            |             |              |       |         |            |            |           |              |
|  **EFCoreDeleteBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |       **10000** | **1,777.270 ms** | **19.4152 ms** |  **16.2125 ms** | **1,776.244 ms** |  **1.00** |    **0.00** | **34000.0000** |  **6000.0000** | **2000.0000** | **145179.05 KB** |
| VenflowDeleteBatchAsync |      .NET 4.8 |      .NET 4.8 |       10000 | 1,022.707 ms | 21.2983 ms |  61.4505 ms | 1,011.731 ms |  0.56 |    0.02 |  3000.0000 |  1000.0000 |         - |  15826.44 KB |
|  RepoDbDeleteBatchAsync |      .NET 4.8 |      .NET 4.8 |       10000 | 1,377.577 ms | 26.9974 ms |  52.0148 ms | 1,372.721 ms |  0.78 |    0.04 | 36000.0000 | 10000.0000 | 4000.0000 | 121355.85 KB |
|                         |               |               |             |              |            |             |              |       |         |            |            |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 | 1,905.736 ms | 37.4875 ms |  53.7634 ms | 1,910.884 ms |  1.00 |    0.00 | 31000.0000 |  6000.0000 | 1000.0000 | 136863.46 KB |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |   689.150 ms | 28.0693 ms |  76.3647 ms |   668.238 ms |  0.38 |    0.05 |  2000.0000 |  1000.0000 |         - |  13710.18 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 | 1,310.706 ms | 21.9514 ms |  27.7615 ms | 1,307.028 ms |  0.69 |    0.03 | 31000.0000 |  8000.0000 | 3000.0000 | 114617.71 KB |
|                         |               |               |             |              |            |             |              |       |         |            |            |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 1,647.453 ms | 32.0528 ms |  55.2895 ms | 1,643.363 ms |  1.00 |    0.00 | 18000.0000 |  6000.0000 | 2000.0000 |  83144.05 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   854.876 ms | 60.2952 ms | 177.7818 ms |   968.376 ms |  0.40 |    0.03 |  3000.0000 |  1000.0000 |         - |  13716.52 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   946.477 ms | 18.7698 ms |  41.2002 ms |   943.485 ms |  0.58 |    0.03 | 38000.0000 |  9000.0000 | 3000.0000 | 109644.04 KB |
