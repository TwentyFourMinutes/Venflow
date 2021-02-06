``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                  Method |           Job |       Runtime | UpdateCount |         Mean |        Error |       StdDev | Ratio | RatioSD |     Gen 0 |     Gen 1 |    Gen 2 |    Allocated |
|------------------------ |-------------- |-------------- |------------ |-------------:|-------------:|-------------:|------:|--------:|----------:|----------:|---------:|-------------:|
|  **EFCoreUpdateBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |          **10** |   **1,884.7 μs** |     **37.32 μs** |     **54.70 μs** |  **1.00** |    **0.00** |    **3.9063** |         **-** |        **-** |     **94.93 KB** |
| VenflowUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     910.4 μs |     15.74 μs |     21.55 μs |  0.48 |    0.01 |    0.9766 |         - |        - |      23.6 KB |
|  RepoDbUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     882.7 μs |     17.47 μs |     25.06 μs |  0.47 |    0.02 |    0.9766 |         - |        - |     19.96 KB |
|                         |               |               |             |              |              |              |       |         |           |           |          |              |
|  EFCoreUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |   1,757.8 μs |     33.94 μs |     46.45 μs |  1.00 |    0.00 |    3.9063 |         - |        - |      77.4 KB |
| VenflowUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     886.8 μs |     17.30 μs |     27.94 μs |  0.50 |    0.02 |         - |         - |        - |     23.51 KB |
|  RepoDbUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     902.5 μs |     17.91 μs |     42.22 μs |  0.52 |    0.02 |         - |         - |        - |     19.96 KB |
|                         |               |               |             |              |              |              |       |         |           |           |          |              |
|  **EFCoreUpdateBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |         **100** |   **8,693.2 μs** |    **134.27 μs** |    **179.25 μs** |  **1.00** |    **0.00** |   **46.8750** |   **15.6250** |        **-** |    **945.46 KB** |
| VenflowUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |   2,605.0 μs |     51.98 μs |     53.38 μs |  0.30 |    0.01 |    7.8125 |         - |        - |    204.23 KB |
|  RepoDbUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |   4,267.8 μs |     79.68 μs |     78.26 μs |  0.49 |    0.01 |    7.8125 |         - |        - |    155.82 KB |
|                         |               |               |             |              |              |              |       |         |           |           |          |              |
|  EFCoreUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   7,597.8 μs |    140.98 μs |    131.87 μs |  1.00 |    0.00 |   31.2500 |    7.8125 |        - |    672.21 KB |
| VenflowUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   2,689.8 μs |     49.98 μs |     44.30 μs |  0.35 |    0.01 |    7.8125 |         - |        - |    204.15 KB |
|  RepoDbUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   4,179.2 μs |     45.86 μs |     40.65 μs |  0.55 |    0.01 |    7.8125 |         - |        - |    155.16 KB |
|                         |               |               |             |              |              |              |       |         |           |           |          |              |
|  **EFCoreUpdateBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |        **1000** |  **81,896.8 μs** |  **1,633.33 μs** |  **1,815.44 μs** |  **1.00** |    **0.00** |  **500.0000** |  **166.6667** |        **-** |  **10267.51 KB** |
| VenflowUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |  16,649.7 μs |    270.81 μs |    240.07 μs |  0.20 |    0.01 |  125.0000 |   93.7500 |  62.5000 |   1959.08 KB |
|  RepoDbUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |  35,645.7 μs |    608.38 μs |    508.03 μs |  0.44 |    0.01 |   66.6667 |         - |        - |   1511.63 KB |
|                         |               |               |             |              |              |              |       |         |           |           |          |              |
|  EFCoreUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |  62,279.9 μs |  1,181.58 μs |  1,451.09 μs |  1.00 |    0.00 |  444.4444 |  222.2222 | 111.1111 |   6621.81 KB |
| VenflowUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |  16,339.6 μs |    316.47 μs |    364.45 μs |  0.26 |    0.01 |  125.0000 |   93.7500 |  62.5000 |   1965.86 KB |
|  RepoDbUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |  36,126.3 μs |    710.91 μs |  1,106.80 μs |  0.58 |    0.02 |   71.4286 |         - |        - |   1507.94 KB |
|                         |               |               |             |              |              |              |       |         |           |           |          |              |
|  **EFCoreUpdateBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |       **10000** | **898,024.6 μs** | **16,554.47 μs** | **14,675.11 μs** |  **1.00** |    **0.00** | **6000.0000** | **2000.0000** |        **-** | **114830.64 KB** |
| VenflowUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 | 192,276.5 μs |  3,791.50 μs |  4,056.86 μs |  0.21 |    0.01 |  666.6667 |  333.3333 |        - |  19495.38 KB |
|  RepoDbUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 | 530,524.4 μs | 10,149.85 μs |  9,494.18 μs |  0.59 |    0.02 |         - |         - |        - |  15069.65 KB |
|                         |               |               |             |              |              |              |       |         |           |           |          |              |
|  EFCoreUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 690,948.9 μs | 13,544.36 μs | 15,054.52 μs |  1.00 |    0.00 | 3000.0000 | 1000.0000 |        - |  66305.13 KB |
| VenflowUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 182,011.7 μs |  3,568.57 μs |  7,208.69 μs |  0.27 |    0.01 |  666.6667 |  333.3333 |        - |  19571.78 KB |
|  RepoDbUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 516,131.0 μs | 10,301.46 μs | 11,863.18 μs |  0.75 |    0.02 |         - |         - |        - |  15030.44 KB |
