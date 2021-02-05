``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                  Method |           Job |       Runtime | UpdateCount |           Mean |        Error |       StdDev |         Median | Ratio | RatioSD |     Gen 0 |     Gen 1 | Gen 2 |    Allocated |
|------------------------ |-------------- |-------------- |------------ |---------------:|-------------:|-------------:|---------------:|------:|--------:|----------:|----------:|------:|-------------:|
|  **EFCoreUpdateBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |          **10** |     **1,845.5 μs** |     **36.25 μs** |     **54.26 μs** |     **1,846.7 μs** |  **1.00** |    **0.00** |         **-** |         **-** |     **-** |     **95.13 KB** |
| VenflowUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     1,465.9 μs |     28.52 μs |     31.70 μs |     1,468.4 μs |  0.80 |    0.03 |         - |         - |     - |     22.75 KB |
|  RepoDbUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |       847.1 μs |     14.80 μs |     19.76 μs |       847.5 μs |  0.46 |    0.02 |         - |         - |     - |     19.96 KB |
|                         |               |               |             |                |              |              |                |       |         |           |           |       |              |
|  EFCoreUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     1,700.8 μs |     33.60 μs |     41.26 μs |     1,688.7 μs |  1.00 |    0.00 |    1.9531 |         - |     - |     77.46 KB |
| VenflowUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     1,474.0 μs |     28.83 μs |     48.16 μs |     1,471.3 μs |  0.87 |    0.04 |         - |         - |     - |     22.73 KB |
|  RepoDbUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |       894.5 μs |     17.45 μs |     25.58 μs |       900.7 μs |  0.52 |    0.02 |         - |         - |     - |     19.94 KB |
|                         |               |               |             |                |              |              |                |       |         |           |           |       |              |
|  **EFCoreUpdateBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |         **100** |     **9,635.3 μs** |    **191.92 μs** |    **269.05 μs** |     **9,595.1 μs** |  **1.00** |    **0.00** |   **31.2500** |         **-** |     **-** |    **945.23 KB** |
| VenflowUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |     6,941.6 μs |    132.45 μs |    141.72 μs |     6,925.6 μs |  0.72 |    0.03 |         - |         - |     - |    195.91 KB |
|  RepoDbUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |     4,054.4 μs |     80.65 μs |    115.67 μs |     4,058.1 μs |  0.42 |    0.02 |         - |         - |     - |    155.97 KB |
|                         |               |               |             |                |              |              |                |       |         |           |           |       |              |
|  EFCoreUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     8,485.2 μs |    161.11 μs |    225.85 μs |     8,502.3 μs |  1.00 |    0.00 |   15.6250 |         - |     - |    672.24 KB |
| VenflowUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     6,954.7 μs |    137.76 μs |    128.86 μs |     6,965.6 μs |  0.83 |    0.03 |         - |         - |     - |     195.9 KB |
|  RepoDbUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     4,041.3 μs |     80.18 μs |    127.18 μs |     4,017.6 μs |  0.48 |    0.02 |         - |         - |     - |    155.18 KB |
|                         |               |               |             |                |              |              |                |       |         |           |           |       |              |
|  **EFCoreUpdateBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |        **1000** |    **87,910.6 μs** |  **1,602.86 μs** |  **1,499.31 μs** |    **87,304.2 μs** |  **1.00** |    **0.00** |  **333.3333** |  **166.6667** |     **-** |  **10268.28 KB** |
| VenflowUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |    61,459.7 μs |    934.09 μs |    873.75 μs |    61,550.4 μs |  0.70 |    0.02 |         - |         - |     - |   1873.54 KB |
|  RepoDbUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |    35,930.6 μs |    713.26 μs |    667.19 μs |    35,798.4 μs |  0.41 |    0.01 |         - |         - |     - |   1511.23 KB |
|                         |               |               |             |                |              |              |                |       |         |           |           |       |              |
|  EFCoreUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    75,847.5 μs |  1,476.76 μs |  1,920.20 μs |    75,943.2 μs |  1.00 |    0.00 |  142.8571 |         - |     - |   6621.35 KB |
| VenflowUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    63,612.6 μs |  1,377.24 μs |  3,995.63 μs |    62,080.6 μs |  0.86 |    0.05 |         - |         - |     - |   1873.81 KB |
|  RepoDbUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    34,983.7 μs |    668.74 μs |    980.23 μs |    34,805.6 μs |  0.46 |    0.02 |         - |         - |     - |   1503.73 KB |
|                         |               |               |             |                |              |              |                |       |         |           |           |       |              |
|  **EFCoreUpdateBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |       **10000** | **1,011,532.0 μs** | **19,216.49 μs** | **21,359.08 μs** | **1,006,684.8 μs** |  **1.00** |    **0.00** | **4000.0000** | **2000.0000** |     **-** | **114836.02 KB** |
| VenflowUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |   660,834.1 μs | 12,488.61 μs | 17,094.54 μs |   659,488.6 μs |  0.65 |    0.02 |         - |         - |     - |  18604.29 KB |
|  RepoDbUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |   546,696.9 μs | 10,415.42 μs | 14,937.49 μs |   545,807.5 μs |  0.54 |    0.02 |         - |         - |     - |  15066.66 KB |
|                         |               |               |             |                |              |              |                |       |         |           |           |       |              |
|  EFCoreUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   827,588.3 μs | 14,586.07 μs | 13,643.82 μs |   825,573.0 μs |  1.00 |    0.00 | 2000.0000 | 1000.0000 |     - |  66298.17 KB |
| VenflowUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   663,231.4 μs | 13,097.67 μs | 14,558.03 μs |   659,428.4 μs |  0.80 |    0.02 |         - |         - |     - |  18607.61 KB |
|  RepoDbUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   522,680.3 μs | 10,350.18 μs | 17,005.64 μs |   517,688.4 μs |  0.64 |    0.02 |         - |         - |     - |  14983.88 KB |
