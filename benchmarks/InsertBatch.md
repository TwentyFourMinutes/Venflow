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
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |          **10** |   **1,765.3 μs** |     **20.21 μs** |     **18.91 μs** |   **1,766.0 μs** |  **1.00** |    **0.00** |    **5.8594** |    **1.9531** |        **-** |    **125.21 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     649.8 μs |      7.31 μs |      6.10 μs |     649.6 μs |  0.37 |    0.01 |         - |         - |        - |     13.48 KB |
|  RepoDbInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     724.5 μs |     14.29 μs |     24.26 μs |     720.5 μs |  0.41 |    0.01 |    0.9766 |         - |        - |     32.59 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |   1,694.3 μs |     26.62 μs |     24.90 μs |   1,700.1 μs |  1.00 |    0.00 |    3.9063 |    1.9531 |        - |    104.41 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     641.3 μs |     11.43 μs |     10.13 μs |     638.0 μs |  0.38 |    0.01 |         - |         - |        - |     13.48 KB |
|  RepoDbInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     699.6 μs |     13.75 μs |     17.88 μs |     693.3 μs |  0.41 |    0.02 |    0.9766 |         - |        - |     23.42 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |         **100** |   **9,415.7 μs** |    **167.21 μs** |    **217.42 μs** |   **9,403.4 μs** |  **1.00** |    **0.00** |   **62.5000** |   **15.6250** |        **-** |   **1172.06 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |   1,383.0 μs |     93.57 μs |    254.57 μs |   1,289.1 μs |  0.15 |    0.02 |    3.9063 |         - |        - |     97.64 KB |
|  RepoDbInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |   3,998.6 μs |     72.31 μs |    149.33 μs |   3,950.4 μs |  0.43 |    0.02 |   15.6250 |         - |        - |    291.58 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   8,123.3 μs |    160.68 μs |    150.30 μs |   8,068.5 μs |  1.00 |    0.00 |   46.8750 |   15.6250 |        - |    959.96 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   1,262.1 μs |     21.83 μs |     54.75 μs |   1,256.2 μs |  0.16 |    0.01 |    3.9063 |         - |        - |     97.63 KB |
|  RepoDbInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   3,875.9 μs |     76.28 μs |    125.33 μs |   3,865.4 μs |  0.48 |    0.02 |    7.8125 |         - |        - |    199.94 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |        **1000** |  **76,468.1 μs** |  **1,345.75 μs** |  **1,258.81 μs** |  **76,129.2 μs** |  **1.00** |    **0.00** |  **571.4286** |  **285.7143** |        **-** |  **11612.53 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |   5,557.9 μs |     87.17 μs |     77.28 μs |   5,563.3 μs |  0.07 |    0.00 |   46.8750 |   23.4375 |        - |    934.62 KB |
|  RepoDbInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |  34,849.7 μs |    247.64 μs |    193.34 μs |  34,877.7 μs |  0.46 |    0.01 |  133.3333 |         - |        - |   2872.24 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |  63,358.8 μs |  1,233.64 μs |  1,093.59 μs |  63,441.1 μs |  1.00 |    0.00 |  500.0000 |  125.0000 |        - |   9487.91 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |   5,578.3 μs |    109.25 μs |    138.16 μs |   5,609.6 μs |  0.09 |    0.00 |   46.8750 |   23.4375 |        - |    934.61 KB |
|  RepoDbInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |  33,895.3 μs |    673.40 μs |    562.32 μs |  33,805.0 μs |  0.53 |    0.01 |   66.6667 |         - |        - |   1956.37 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |       **10000** | **809,305.0 μs** | **14,110.05 μs** | **12,508.19 μs** | **813,786.7 μs** |  **1.00** |    **0.00** | **6000.0000** | **3000.0000** |        **-** | **116240.75 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |  54,990.9 μs |  1,050.39 μs |  1,209.63 μs |  55,162.2 μs |  0.07 |    0.00 |  600.0000 |  500.0000 | 300.0000 |   9234.03 KB |
|  RepoDbInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 | 519,178.2 μs | 10,066.41 μs | 12,730.79 μs | 520,246.6 μs |  0.64 |    0.02 | 1000.0000 |         - |        - |  28777.07 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 659,639.8 μs |  9,001.55 μs |  8,420.05 μs | 659,488.0 μs |  1.00 |    0.00 | 4000.0000 | 2000.0000 |        - |  94987.62 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |  51,751.8 μs |    846.90 μs |    792.19 μs |  51,669.7 μs |  0.08 |    0.00 |  500.0000 |  300.0000 | 200.0000 |   9236.75 KB |
|  RepoDbInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 480,804.3 μs |  6,442.68 μs |  7,912.19 μs | 481,950.7 μs |  0.73 |    0.02 | 1000.0000 |         - |        - |  19485.97 KB |
