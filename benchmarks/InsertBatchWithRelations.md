``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                  Method |           Job |       Runtime | InsertCount |         Mean |       Error |      StdDev | Ratio |      Gen 0 |      Gen 1 |    Gen 2 |    Allocated |
|------------------------ |-------------- |-------------- |------------ |-------------:|------------:|------------:|------:|-----------:|-----------:|---------:|-------------:|
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |          **10** |    **11.485 ms** |   **0.2036 ms** |   **0.1805 ms** |  **1.00** |    **46.8750** |    **15.6250** |        **-** |   **1219.52 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     2.245 ms |   0.0448 ms |   0.0926 ms |  0.20 |     3.9063 |          - |        - |    111.22 KB |
|                         |               |               |             |              |             |             |       |            |            |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |    10.304 ms |   0.2015 ms |   0.1885 ms |  1.00 |    31.2500 |    15.6250 |        - |    989.56 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     2.229 ms |   0.0434 ms |   0.0689 ms |  0.22 |     3.9063 |          - |        - |    111.36 KB |
|                         |               |               |             |              |             |             |       |            |            |          |              |
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |         **100** |    **86.900 ms** |   **1.6429 ms** |   **1.8261 ms** |  **1.00** |   **333.3333** |   **166.6667** |        **-** |  **12691.02 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |     9.369 ms |   0.1869 ms |   0.1748 ms |  0.11 |    31.2500 |    15.6250 |        - |   1028.96 KB |
|                         |               |               |             |              |             |             |       |            |            |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |    77.049 ms |   1.4666 ms |   1.5061 ms |  1.00 |   285.7143 |   142.8571 |        - |   9702.26 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     9.397 ms |   0.1875 ms |   0.2864 ms |  0.12 |    31.2500 |    15.6250 |        - |   1028.91 KB |
|                         |               |               |             |              |             |             |       |            |            |          |              |
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |        **1000** |   **803.569 ms** |  **15.4726 ms** |  **15.8892 ms** |  **1.00** |  **4000.0000** |  **2000.0000** |        **-** | **120101.59 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |    82.217 ms |   1.5964 ms |   1.4151 ms |  0.10 |   428.5714 |   285.7143 | 142.8571 |  10024.02 KB |
|                         |               |               |             |              |             |             |       |            |            |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |   736.826 ms |  14.7154 ms |  16.3561 ms |  1.00 |  3000.0000 |  1000.0000 |        - |  97044.93 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    81.992 ms |   1.5903 ms |   2.0678 ms |  0.11 |   428.5714 |   285.7143 | 142.8571 |  10023.51 KB |
|                         |               |               |             |              |             |             |       |            |            |          |              |
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |       **10000** | **8,587.589 ms** | **129.4395 ms** | **108.0878 ms** |  **1.00** | **44000.0000** | **29000.0000** |        **-** |   **1201520 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |   903.460 ms |  16.1252 ms |  32.5738 ms |  0.11 |  2000.0000 |  1000.0000 |        - | 109059.63 KB |
|                         |               |               |             |              |             |             |       |            |            |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 7,583.398 ms | 149.0969 ms | 139.4653 ms |  1.00 | 35000.0000 | 12000.0000 |        - | 970643.42 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   875.371 ms |  16.9056 ms |  23.6993 ms |  0.11 |  2000.0000 |  1000.0000 |        - | 109048.17 KB |
