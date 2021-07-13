``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                  Method | UpdateCount |         Mean |      Error |     StdDev | Ratio | RatioSD |     Gen 0 |     Gen 1 | Gen 2 | Allocated |
|------------------------ |------------ |-------------:|-----------:|-----------:|------:|--------:|----------:|----------:|------:|----------:|
|  **EFCoreUpdateBatchAsync** |          **10** |     **2.146 ms** |  **0.0419 ms** |  **0.0601 ms** |  **1.00** |    **0.00** |         **-** |         **-** |     **-** |     **66 KB** |
| VenflowUpdateBatchAsync |          10 |     1.768 ms |  0.0348 ms |  0.0532 ms |  0.83 |    0.04 |         - |         - |     - |     19 KB |
|  RepoDbUpdateBatchAsync |          10 |     1.017 ms |  0.0203 ms |  0.0351 ms |  0.48 |    0.02 |         - |         - |     - |     11 KB |
|                         |             |              |            |            |       |         |           |           |       |           |
|  **EFCoreUpdateBatchAsync** |         **100** |    **11.027 ms** |  **0.2001 ms** |  **0.1872 ms** |  **1.00** |    **0.00** |   **15.6250** |         **-** |     **-** |    **577 KB** |
| VenflowUpdateBatchAsync |         100 |     8.121 ms |  0.1616 ms |  0.1924 ms |  0.74 |    0.02 |         - |         - |     - |    158 KB |
|  RepoDbUpdateBatchAsync |         100 |     4.978 ms |  0.0995 ms |  0.2496 ms |  0.42 |    0.02 |         - |         - |     - |     93 KB |
|                         |             |              |            |            |       |         |           |           |       |           |
|  **EFCoreUpdateBatchAsync** |        **1000** |    **93.709 ms** |  **1.8566 ms** |  **2.6027 ms** |  **1.00** |    **0.00** |  **200.0000** |         **-** |     **-** |  **5,711 KB** |
| VenflowUpdateBatchAsync |        1000 |    69.863 ms |  1.3946 ms |  1.9551 ms |  0.75 |    0.03 |         - |         - |     - |  1,504 KB |
|  RepoDbUpdateBatchAsync |        1000 |    39.091 ms |  0.7309 ms |  0.9758 ms |  0.42 |    0.01 |         - |         - |     - |    867 KB |
|                         |             |              |            |            |       |         |           |           |       |           |
|  **EFCoreUpdateBatchAsync** |       **10000** | **1,070.242 ms** | **20.9136 ms** | **24.0841 ms** |  **1.00** |    **0.00** | **2000.0000** | **1000.0000** |     **-** | **57,206 KB** |
| VenflowUpdateBatchAsync |       10000 |   770.476 ms | 11.7090 ms | 12.0243 ms |  0.72 |    0.02 |         - |         - |     - | 15,210 KB |
|  RepoDbUpdateBatchAsync |       10000 |   599.384 ms | 11.7331 ms | 20.2391 ms |  0.56 |    0.02 |         - |         - |     - |  8,639 KB |
