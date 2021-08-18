``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.7.21379.14
  [Host]   : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                  Method | BatchCount |       Mean |      Error |     StdDev | Ratio | RatioSD |     Gen 0 |     Gen 1 | Gen 2 | Allocated |
|------------------------ |----------- |-----------:|-----------:|-----------:|------:|--------:|----------:|----------:|------:|----------:|
|  **EFCoreDeleteBatchAsync** |         **10** |   **2.876 ms** |  **0.0559 ms** |  **0.0686 ms** |  **1.00** |    **0.00** |         **-** |         **-** |     **-** |     **82 KB** |
| VenflowDeleteBatchAsync |         10 |   1.749 ms |  0.0347 ms |  0.0643 ms |  0.62 |    0.03 |         - |         - |     - |     17 KB |
|  RepoDbDeleteBatchAsync |         10 |   1.811 ms |  0.0352 ms |  0.0505 ms |  0.63 |    0.03 |         - |         - |     - |     28 KB |
|                         |            |            |            |            |       |         |           |           |       |           |
|  **EFCoreDeleteBatchAsync** |        **100** |  **11.002 ms** |  **0.2100 ms** |  **0.2247 ms** |  **1.00** |    **0.00** |   **15.6250** |         **-** |     **-** |    **729 KB** |
| VenflowDeleteBatchAsync |        100 |   3.290 ms |  0.0644 ms |  0.1058 ms |  0.30 |    0.01 |    3.9063 |         - |     - |    112 KB |
|  RepoDbDeleteBatchAsync |        100 |   3.589 ms |  0.0699 ms |  0.1046 ms |  0.33 |    0.01 |         - |         - |     - |    154 KB |
|                         |            |            |            |            |       |         |           |           |       |           |
|  **EFCoreDeleteBatchAsync** |       **1000** |  **89.576 ms** |  **1.7651 ms** |  **2.0327 ms** |  **1.00** |    **0.00** |  **166.6667** |         **-** |     **-** |  **7,127 KB** |
| VenflowDeleteBatchAsync |       1000 |  14.871 ms |  0.2909 ms |  0.3572 ms |  0.17 |    0.01 |   31.2500 |         - |     - |  1,069 KB |
|  RepoDbDeleteBatchAsync |       1000 |  31.644 ms |  0.5389 ms |  0.6816 ms |  0.35 |    0.01 |   62.5000 |         - |     - |  2,559 KB |
|                         |            |            |            |            |       |         |           |           |       |           |
|  **EFCoreDeleteBatchAsync** |      **10000** | **930.914 ms** | **17.4743 ms** | **16.3454 ms** |  **1.00** |    **0.00** | **2000.0000** | **1000.0000** |     **-** | **71,313 KB** |
| VenflowDeleteBatchAsync |      10000 | 142.113 ms |  2.7685 ms |  3.3999 ms |  0.15 |    0.00 |  250.0000 |         - |     - | 10,745 KB |
|  RepoDbDeleteBatchAsync |      10000 | 479.895 ms |  8.6848 ms |  7.6988 ms |  0.52 |    0.01 | 3000.0000 | 1000.0000 |     - | 95,553 KB |
