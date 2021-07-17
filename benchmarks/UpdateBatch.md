``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                  Method | BatchCount |         Mean |      Error |     StdDev | Ratio | RatioSD |     Gen 0 |     Gen 1 | Gen 2 | Allocated |
|------------------------ |----------- |-------------:|-----------:|-----------:|------:|--------:|----------:|----------:|------:|----------:|
|  **EFCoreUpdateBatchAsync** |         **10** |     **2.178 ms** |  **0.0433 ms** |  **0.0405 ms** |  **1.00** |    **0.00** |         **-** |         **-** |     **-** |     **66 KB** |
| VenflowUpdateBatchAsync |         10 |     1.851 ms |  0.0368 ms |  0.0516 ms |  0.85 |    0.04 |         - |         - |     - |     18 KB |
|  RepoDbUpdateBatchAsync |         10 |     1.012 ms |  0.0201 ms |  0.0330 ms |  0.46 |    0.02 |         - |         - |     - |     11 KB |
|                         |            |              |            |            |       |         |           |           |       |           |
|  **EFCoreUpdateBatchAsync** |        **100** |    **11.475 ms** |  **0.1912 ms** |  **0.1788 ms** |  **1.00** |    **0.00** |   **15.6250** |         **-** |     **-** |    **577 KB** |
| VenflowUpdateBatchAsync |        100 |     8.871 ms |  0.1724 ms |  0.2527 ms |  0.78 |    0.02 |         - |         - |     - |    155 KB |
|  RepoDbUpdateBatchAsync |        100 |     4.794 ms |  0.0958 ms |  0.1548 ms |  0.42 |    0.01 |         - |         - |     - |     91 KB |
|                         |            |              |            |            |       |         |           |           |       |           |
|  **EFCoreUpdateBatchAsync** |       **1000** |   **102.812 ms** |  **2.0558 ms** |  **2.5248 ms** |  **1.00** |    **0.00** |  **200.0000** |         **-** |     **-** |  **5,711 KB** |
| VenflowUpdateBatchAsync |       1000 |    78.144 ms |  1.5537 ms |  2.1781 ms |  0.76 |    0.03 |         - |         - |     - |  1,508 KB |
|  RepoDbUpdateBatchAsync |       1000 |    41.958 ms |  0.8361 ms |  1.2768 ms |  0.41 |    0.01 |         - |         - |     - |    859 KB |
|                         |            |              |            |            |       |         |           |           |       |           |
|  **EFCoreUpdateBatchAsync** |      **10000** | **1,130.021 ms** | **13.5623 ms** | **12.0226 ms** |  **1.00** |    **0.00** | **2000.0000** | **1000.0000** |     **-** | **57,209 KB** |
| VenflowUpdateBatchAsync |      10000 |   827.176 ms | 16.4014 ms | 16.8430 ms |  0.73 |    0.02 |         - |         - |     - | 15,203 KB |
|  RepoDbUpdateBatchAsync |      10000 |   650.199 ms | 12.9465 ms | 33.6497 ms |  0.60 |    0.03 |         - |         - |     - |  8,766 KB |
