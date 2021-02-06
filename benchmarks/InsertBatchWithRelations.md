``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                  Method |           Job |       Runtime | InsertCount |         Mean |       Error |      StdDev | Ratio |      Gen 0 |      Gen 1 |    Gen 2 |     Allocated |
|------------------------ |-------------- |-------------- |------------ |-------------:|------------:|------------:|------:|-----------:|-----------:|---------:|--------------:|
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |          **10** |    **11.807 ms** |   **0.1868 ms** |   **0.1656 ms** |  **1.00** |    **62.5000** |    **15.6250** |        **-** |    **1219.55 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     2.590 ms |   0.0492 ms |   0.0586 ms |  0.22 |     3.9063 |          - |        - |     111.33 KB |
|                         |               |               |             |              |             |             |       |            |            |          |               |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |    10.124 ms |   0.1503 ms |   0.1333 ms |  1.00 |    46.8750 |    15.6250 |        - |      989.5 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     2.553 ms |   0.0509 ms |   0.0793 ms |  0.26 |     3.9063 |          - |        - |     111.29 KB |
|                         |               |               |             |              |             |             |       |            |            |          |               |
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |         **100** |    **87.853 ms** |   **1.7362 ms** |   **1.9298 ms** |  **1.00** |   **500.0000** |   **166.6667** |        **-** |   **12003.23 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |    10.939 ms |   0.2132 ms |   0.3058 ms |  0.12 |    46.8750 |    15.6250 |        - |    1028.95 KB |
|                         |               |               |             |              |             |             |       |            |            |          |               |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |    74.668 ms |   1.4775 ms |   1.8145 ms |  1.00 |   428.5714 |   142.8571 |        - |    9702.35 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |    10.654 ms |   0.1881 ms |   0.1667 ms |  0.14 |    46.8750 |    15.6250 |        - |    1028.92 KB |
|                         |               |               |             |              |             |             |       |            |            |          |               |
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |        **1000** |   **827.827 ms** |  **15.1306 ms** |  **14.1532 ms** |  **1.00** |  **6000.0000** |  **2000.0000** |        **-** |  **120086.98 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |    97.047 ms |   1.9340 ms |   1.9860 ms |  0.12 |   600.0000 |   400.0000 | 200.0000 |    10023.6 KB |
|                         |               |               |             |              |             |             |       |            |            |          |               |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |   693.883 ms |  13.3759 ms |  14.3121 ms |  1.00 |  5000.0000 |  2000.0000 |        - |    97045.2 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    98.547 ms |   1.9346 ms |   2.7121 ms |  0.14 |   600.0000 |   400.0000 | 200.0000 |   10022.61 KB |
|                         |               |               |             |              |             |             |       |            |            |          |               |
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |       **10000** | **8,711.608 ms** | **162.3067 ms** | **143.8807 ms** |  **1.00** | **62000.0000** | **18000.0000** |        **-** | **1201515.38 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 | 1,008.880 ms |  18.6765 ms |  32.7104 ms |  0.12 |  3000.0000 |  1000.0000 |        - |  109053.75 KB |
|                         |               |               |             |              |             |             |       |            |            |          |               |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 7,193.576 ms | 132.7821 ms | 130.4098 ms |  1.00 | 50000.0000 | 15000.0000 |        - |  970627.49 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   972.769 ms |  19.0003 ms |  23.3340 ms |  0.13 |  3000.0000 |  1000.0000 |        - |  109045.34 KB |
