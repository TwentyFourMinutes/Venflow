``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                  Method |           Job |       Runtime | InsertCount |         Mean |        Error |       StdDev |       Median | Ratio | RatioSD |     Gen 0 |     Gen 1 |    Gen 2 |    Allocated |
|------------------------ |-------------- |-------------- |------------ |-------------:|-------------:|-------------:|-------------:|------:|--------:|----------:|----------:|---------:|-------------:|
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |          **10** |   **1,822.7 μs** |     **36.35 μs** |     **45.97 μs** |   **1,835.7 μs** |  **1.00** |    **0.00** |    **3.9063** |         **-** |        **-** |    **125.22 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     780.7 μs |     14.61 μs |     18.48 μs |     779.3 μs |  0.43 |    0.02 |         - |         - |        - |     13.49 KB |
|  RepoDbInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     851.9 μs |     16.96 μs |     38.97 μs |     841.5 μs |  0.47 |    0.03 |    0.9766 |         - |        - |     32.59 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |   1,714.8 μs |     31.24 μs |     48.63 μs |   1,713.8 μs |  1.00 |    0.00 |    3.9063 |    1.9531 |        - |     104.4 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     785.4 μs |     15.36 μs |     28.46 μs |     779.8 μs |  0.46 |    0.02 |         - |         - |        - |     13.48 KB |
|  RepoDbInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     832.8 μs |     14.01 μs |     21.39 μs |     829.6 μs |  0.49 |    0.02 |         - |         - |        - |     23.41 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |         **100** |   **9,360.4 μs** |    **184.85 μs** |    **220.04 μs** |   **9,327.8 μs** |  **1.00** |    **0.00** |   **31.2500** |   **15.6250** |        **-** |   **1172.07 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |   1,424.4 μs |    128.22 μs |    374.02 μs |   1,215.1 μs |  0.15 |    0.04 |         - |         - |        - |     97.63 KB |
|  RepoDbInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |   4,042.6 μs |     79.76 μs |    131.04 μs |   3,997.3 μs |  0.43 |    0.02 |    7.8125 |         - |        - |    291.55 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   8,428.6 μs |    167.74 μs |    256.15 μs |   8,398.0 μs |  1.00 |    0.00 |   31.2500 |   15.6250 |        - |       960 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   1,219.5 μs |     24.30 μs |     65.29 μs |   1,205.2 μs |  0.15 |    0.01 |         - |         - |        - |     97.63 KB |
|  RepoDbInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   3,902.8 μs |     77.40 μs |     95.05 μs |   3,878.6 μs |  0.46 |    0.02 |    7.8125 |         - |        - |    200.08 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |        **1000** |  **74,732.7 μs** |  **1,434.28 μs** |  **1,864.97 μs** |  **75,033.4 μs** |  **1.00** |    **0.00** |  **428.5714** |  **142.8571** |        **-** |  **11612.61 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |   5,070.0 μs |     97.83 μs |    130.59 μs |   5,055.7 μs |  0.07 |    0.00 |   31.2500 |   15.6250 |        - |    934.75 KB |
|  RepoDbInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |  36,849.7 μs |    727.71 μs |  1,293.50 μs |  36,526.5 μs |  0.50 |    0.02 |   71.4286 |         - |        - |   2872.12 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |  65,402.8 μs |  1,296.14 μs |  1,639.20 μs |  65,325.4 μs |  1.00 |    0.00 |  250.0000 |  125.0000 |        - |   9487.75 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |   5,010.5 μs |     98.45 μs |    124.50 μs |   5,008.2 μs |  0.08 |    0.00 |   31.2500 |   15.6250 |        - |    934.71 KB |
|  RepoDbInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |  35,726.5 μs |    696.50 μs |  1,201.43 μs |  35,773.2 μs |  0.55 |    0.02 |   71.4286 |         - |        - |   1958.28 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  **EfCoreInsertBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |       **10000** | **786,901.1 μs** | **15,512.78 μs** | **15,930.48 μs** | **788,111.7 μs** |  **1.00** |    **0.00** | **4000.0000** | **2000.0000** |        **-** | **116240.99 KB** |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |  49,154.2 μs |    971.60 μs |  1,623.33 μs |  48,910.4 μs |  0.06 |    0.00 |  444.4444 |  333.3333 | 222.2222 |   9234.36 KB |
|  RepoDbInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 | 491,251.7 μs |  9,659.35 μs | 16,138.60 μs | 487,186.9 μs |  0.62 |    0.03 | 1000.0000 |         - |        - |  28777.92 KB |
|                         |               |               |             |              |              |              |              |       |         |           |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 657,067.5 μs | 11,426.50 μs | 10,129.29 μs | 657,052.5 μs |  1.00 |    0.00 | 3000.0000 | 1000.0000 |        - |  94987.05 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |  45,914.7 μs |    896.27 μs |  1,032.14 μs |  45,813.4 μs |  0.07 |    0.00 |  400.0000 |  300.0000 | 200.0000 |   9235.18 KB |
|  RepoDbInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 457,894.9 μs |  7,487.04 μs |  9,735.26 μs | 459,226.3 μs |  0.70 |    0.02 |         - |         - |        - |  19614.55 KB |
