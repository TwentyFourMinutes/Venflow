``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                  Method | DeleteCount |       Mean |      Error |     StdDev |     Median | Ratio | RatioSD |     Gen 0 |     Gen 1 | Gen 2 | Allocated |
|------------------------ |------------ |-----------:|-----------:|-----------:|-----------:|------:|--------:|----------:|----------:|------:|----------:|
|  **EFCoreDeleteBatchAsync** |          **10** |   **3.056 ms** |  **0.0576 ms** |  **0.0664 ms** |   **3.061 ms** |  **1.00** |    **0.00** |         **-** |         **-** |     **-** |     **80 KB** |
| VenflowDeleteBatchAsync |          10 |   1.962 ms |  0.0389 ms |  0.0640 ms |   1.959 ms |  0.64 |    0.02 |         - |         - |     - |     17 KB |
|  RepoDbDeleteBatchAsync |          10 |   2.157 ms |  0.0431 ms |  0.1120 ms |   2.159 ms |  0.67 |    0.03 |         - |         - |     - |     28 KB |
|                         |             |            |            |            |            |       |         |           |           |       |           |
|  **EFCoreDeleteBatchAsync** |         **100** |  **10.835 ms** |  **0.2052 ms** |  **0.2196 ms** |  **10.801 ms** |  **1.00** |    **0.00** |   **15.6250** |         **-** |     **-** |    **707 KB** |
| VenflowDeleteBatchAsync |         100 |   5.234 ms |  0.8824 ms |  2.6016 ms |   3.546 ms |  0.47 |    0.20 |    3.9063 |         - |     - |    112 KB |
|  RepoDbDeleteBatchAsync |         100 |   3.766 ms |  0.0750 ms |  0.1390 ms |   3.766 ms |  0.35 |    0.02 |         - |         - |     - |    154 KB |
|                         |             |            |            |            |            |       |         |           |           |       |           |
|  **EFCoreDeleteBatchAsync** |        **1000** |  **88.288 ms** |  **1.7474 ms** |  **1.9423 ms** |  **87.795 ms** |  **1.00** |    **0.00** |  **166.6667** |         **-** |     **-** |  **6,902 KB** |
| VenflowDeleteBatchAsync |        1000 |  14.968 ms |  0.2912 ms |  0.4785 ms |  14.861 ms |  0.17 |    0.01 |   31.2500 |         - |     - |  1,077 KB |
|  RepoDbDeleteBatchAsync |        1000 |  31.919 ms |  0.6324 ms |  1.5151 ms |  31.370 ms |  0.38 |    0.02 |   93.7500 |   31.2500 |     - |  2,570 KB |
|                         |             |            |            |            |            |       |         |           |           |       |           |
|  **EFCoreDeleteBatchAsync** |       **10000** | **930.405 ms** | **18.5630 ms** | **20.6327 ms** | **923.341 ms** |  **1.00** |    **0.00** | **2000.0000** | **1000.0000** |     **-** | **69,049 KB** |
| VenflowDeleteBatchAsync |       10000 | 142.229 ms |  2.7808 ms |  3.8064 ms | 142.052 ms |  0.15 |    0.01 |  250.0000 |         - |     - | 10,750 KB |
|  RepoDbDeleteBatchAsync |       10000 | 487.361 ms |  9.6361 ms |  9.8956 ms | 487.772 ms |  0.52 |    0.01 | 3000.0000 | 1000.0000 |     - | 95,232 KB |
