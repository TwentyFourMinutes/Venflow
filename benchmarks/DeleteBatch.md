``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                  Method |           Job |       Runtime | DeleteCount |         Mean |      Error |     StdDev |       Median | Ratio | RatioSD |     Gen 0 |     Gen 1 |     Gen 2 |    Allocated |
|------------------------ |-------------- |-------------- |------------ |-------------:|-----------:|-----------:|-------------:|------:|--------:|----------:|----------:|----------:|-------------:|
|  **EFCoreDeleteBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |          **10** |     **2.655 ms** |  **0.0521 ms** |  **0.0695 ms** |     **2.665 ms** |  **1.00** |    **0.00** |    **3.9063** |         **-** |         **-** |    **108.21 KB** |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     1.886 ms |  0.0636 ms |  0.1844 ms |     1.870 ms |  0.63 |    0.05 |         - |         - |         - |     23.72 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     1.629 ms |  0.0195 ms |  0.0173 ms |     1.629 ms |  0.61 |    0.02 |         - |         - |         - |     43.58 KB |
|                         |               |               |             |              |            |            |              |       |         |           |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     2.650 ms |  0.0527 ms |  0.1293 ms |     2.618 ms |  1.00 |    0.00 |         - |         - |         - |     92.24 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     1.581 ms |  0.0316 ms |  0.0585 ms |     1.572 ms |  0.60 |    0.04 |         - |         - |         - |     23.61 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     1.906 ms |  0.0966 ms |  0.2833 ms |     1.786 ms |  0.67 |    0.09 |         - |         - |         - |     33.99 KB |
|                         |               |               |             |              |            |            |              |       |         |           |           |           |              |
|  **EFCoreDeleteBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |         **100** |    **10.538 ms** |  **0.2100 ms** |  **0.3012 ms** |    **10.500 ms** |  **1.00** |    **0.00** |   **31.2500** |         **-** |         **-** |   **1009.03 KB** |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |     2.810 ms |  0.0550 ms |  0.0634 ms |     2.821 ms |  0.27 |    0.01 |    3.9063 |         - |         - |    164.16 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |     3.158 ms |  0.0622 ms |  0.0611 ms |     3.150 ms |  0.30 |    0.01 |   11.7188 |    3.9063 |         - |    306.35 KB |
|                         |               |               |             |              |            |            |              |       |         |           |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     9.515 ms |  0.1672 ms |  0.2115 ms |     9.555 ms |  1.00 |    0.00 |   31.2500 |         - |         - |    809.74 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     2.842 ms |  0.0556 ms |  0.0866 ms |     2.856 ms |  0.30 |    0.01 |    3.9063 |         - |         - |    164.11 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     3.103 ms |  0.0620 ms |  0.1035 ms |     3.088 ms |  0.33 |    0.02 |    7.8125 |         - |         - |    214.03 KB |
|                         |               |               |             |              |            |            |              |       |         |           |           |           |              |
|  **EFCoreDeleteBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |        **1000** |    **92.645 ms** |  **1.8233 ms** |  **2.1705 ms** |    **92.578 ms** |  **1.00** |    **0.00** |  **333.3333** |  **166.6667** |         **-** |  **11586.42 KB** |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |    12.463 ms |  0.2385 ms |  0.2342 ms |    12.450 ms |  0.13 |    0.00 |   46.8750 |   15.6250 |         - |   1564.06 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |    26.746 ms |  0.5017 ms |  0.5152 ms |    26.918 ms |  0.29 |    0.01 |  156.2500 |   62.5000 |         - |   4235.32 KB |
|                         |               |               |             |              |            |            |              |       |         |           |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    77.329 ms |  1.5033 ms |  1.6710 ms |    76.731 ms |  1.00 |    0.00 |  285.7143 |  142.8571 |         - |   7888.34 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    12.543 ms |  0.2417 ms |  0.3466 ms |    12.491 ms |  0.16 |    0.00 |   46.8750 |   15.6250 |         - |   1564.11 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    26.700 ms |  0.5339 ms |  1.4794 ms |    26.165 ms |  0.37 |    0.01 |   93.7500 |   31.2500 |         - |   3052.62 KB |
|                         |               |               |             |              |            |            |              |       |         |           |           |           |              |
|  **EFCoreDeleteBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |       **10000** | **1,056.772 ms** | **21.1331 ms** | **24.3369 ms** | **1,050.680 ms** |  **1.00** |    **0.00** | **6000.0000** | **3000.0000** | **1000.0000** | **136588.05 KB** |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |   120.470 ms |  1.6441 ms |  1.2836 ms |   120.717 ms |  0.11 |    0.00 | 1200.0000 | 1000.0000 |  800.0000 |  15561.87 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |   420.723 ms |  6.9275 ms |  5.7847 ms |   422.166 ms |  0.40 |    0.01 | 4000.0000 | 1000.0000 |         - | 121298.79 KB |
|                         |               |               |             |              |            |            |              |       |         |           |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   808.492 ms | 15.6882 ms | 14.6748 ms |   810.190 ms |  1.00 |    0.00 | 2000.0000 | 1000.0000 |         - |  78778.41 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   121.155 ms |  2.3948 ms |  2.2401 ms |   120.895 ms |  0.15 |    0.00 | 1200.0000 | 1000.0000 |  800.0000 |  15562.23 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   404.596 ms |  7.7706 ms |  7.9799 ms |   403.747 ms |  0.50 |    0.01 | 3000.0000 | 1000.0000 |         - |  94242.23 KB |
