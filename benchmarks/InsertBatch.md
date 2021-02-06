``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                  Method |           Job |       Runtime | InsertCount |         Mean |        Error |       StdDev |       Median | Ratio | RatioSD |     Gen 0 |     Gen 1 |    Gen 2 |    Allocated |
|------------------------ |-------------- |-------------- |------------ |-------------:|-------------:|-------------:|-------------:|------:|--------:|----------:|----------:|---------:|-------------:|
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |          **10** |   **1,944.7 μs** |     **29.59 μs** |     **26.23 μs** |   **1,939.8 μs** |  **1.00** |    **0.00** |    **3.9063** |         **-** |        **-** |     **125.2 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     855.1 μs |     16.90 μs |     41.77 μs |     848.3 μs |  0.45 |    0.02 |         - |         - |        - |     13.52 KB |
|  RepoDbInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     937.3 μs |     18.15 μs |     40.59 μs |     937.2 μs |  0.49 |    0.02 |         - |         - |        - |      32.6 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |   1,951.8 μs |     38.43 μs |     72.18 μs |   1,947.3 μs |  1.00 |    0.00 |    3.9063 |         - |        - |    104.41 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     838.8 μs |     16.48 μs |     26.62 μs |     835.3 μs |  0.43 |    0.02 |         - |         - |        - |      13.5 KB |
|  RepoDbInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     862.8 μs |     15.97 μs |     19.61 μs |     867.7 μs |  0.45 |    0.02 |         - |         - |        - |     23.41 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |         **100** |   **9,589.3 μs** |    **190.92 μs** |    **248.25 μs** |   **9,617.4 μs** |  **1.00** |    **0.00** |   **62.5000** |   **31.2500** |        **-** |   **1172.18 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |   1,462.6 μs |     29.03 μs |     52.34 μs |   1,450.8 μs |  0.15 |    0.01 |    3.9063 |         - |        - |     97.65 KB |
|  RepoDbInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |   4,427.9 μs |     87.95 μs |    123.30 μs |   4,410.2 μs |  0.46 |    0.02 |   15.6250 |         - |        - |    291.54 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   8,285.1 μs |    165.02 μs |    154.36 μs |   8,328.1 μs |  1.00 |    0.00 |   46.8750 |   15.6250 |        - |     960.1 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   1,620.9 μs |     78.05 μs |    225.18 μs |   1,495.6 μs |  0.20 |    0.03 |    3.9063 |         - |        - |     97.66 KB |
|  RepoDbInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   4,206.9 μs |     63.57 μs |     56.36 μs |   4,210.2 μs |  0.51 |    0.01 |    7.8125 |         - |        - |    200.23 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |        **1000** |  **76,530.7 μs** |  **1,458.21 μs** |  **1,432.16 μs** |  **76,197.4 μs** |  **1.00** |    **0.00** |  **571.4286** |  **285.7143** |        **-** |  **11612.67 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |   5,848.6 μs |    113.32 μs |    143.31 μs |   5,818.4 μs |  0.08 |    0.00 |   46.8750 |   23.4375 |        - |     934.8 KB |
|  RepoDbInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |  37,113.3 μs |    356.29 μs |    297.52 μs |  37,186.4 μs |  0.48 |    0.01 |  142.8571 |         - |        - |   2872.23 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |  62,802.9 μs |  1,168.45 μs |  1,035.80 μs |  62,970.5 μs |  1.00 |    0.00 |  500.0000 |  125.0000 |        - |   9487.94 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |   5,822.5 μs |    111.83 μs |    114.84 μs |   5,780.9 μs |  0.09 |    0.00 |   46.8750 |   23.4375 |        - |    934.66 KB |
|  RepoDbInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |  36,901.8 μs |    734.93 μs |  1,077.25 μs |  36,943.6 μs |  0.59 |    0.02 |   71.4286 |         - |        - |   1958.28 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |       **10000** | **787,655.4 μs** | **15,085.65 μs** | **13,373.04 μs** | **787,766.8 μs** |  **1.00** |    **0.00** | **6000.0000** | **3000.0000** |        **-** | **116243.91 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |  56,322.0 μs |  1,117.72 μs |  1,147.82 μs |  56,643.0 μs |  0.07 |    0.00 |  600.0000 |  500.0000 | 300.0000 |   9234.01 KB |
|  RepoDbInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 | 523,509.1 μs |  5,454.51 μs |  4,554.76 μs | 522,063.8 μs |  0.67 |    0.01 | 1000.0000 |         - |        - |   28777.7 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 646,724.3 μs | 12,491.67 μs | 12,268.49 μs | 647,393.4 μs |  1.00 |    0.00 | 4000.0000 | 1000.0000 |        - |  94988.84 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |  50,903.6 μs |    943.82 μs |    926.96 μs |  50,734.6 μs |  0.08 |    0.00 |  500.0000 |  300.0000 | 200.0000 |   9234.67 KB |
|  RepoDbInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 513,147.3 μs |  9,997.60 μs | 15,857.26 μs | 514,802.7 μs |  0.80 |    0.03 | 1000.0000 |         - |        - |  19629.55 KB |
