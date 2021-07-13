``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                  Method | BatchCount |         Mean |       Error |      StdDev | Ratio | RatioSD |     Gen 0 |     Gen 1 | Gen 2 | Allocated |
|------------------------ |----------- |-------------:|------------:|------------:|------:|--------:|----------:|----------:|------:|----------:|
|  **EFCoreUpdateBatchAsync** |         **10** |   **1,434.0 μs** |    **21.72 μs** |    **20.32 μs** |  **1.00** |    **0.00** |    **1.9531** |         **-** |     **-** |     **66 KB** |
| VenflowUpdateBatchAsync |         10 |   1,049.4 μs |    20.76 μs |    34.11 μs |  0.74 |    0.03 |         - |         - |     - |     19 KB |
|  RepoDbUpdateBatchAsync |         10 |     621.7 μs |     6.45 μs |     6.03 μs |  0.43 |    0.01 |         - |         - |     - |     11 KB |
|                         |            |              |             |             |       |         |           |           |       |           |
|  **EFCoreUpdateBatchAsync** |        **100** |   **7,202.5 μs** |    **42.81 μs** |    **40.04 μs** |  **1.00** |    **0.00** |   **31.2500** |    **7.8125** |     **-** |    **577 KB** |
| VenflowUpdateBatchAsync |        100 |   4,949.8 μs |    47.40 μs |    42.02 μs |  0.69 |    0.01 |    7.8125 |         - |     - |    158 KB |
|  RepoDbUpdateBatchAsync |        100 |   3,096.7 μs |    39.50 μs |    35.02 μs |  0.43 |    0.01 |    3.9063 |         - |     - |     92 KB |
|                         |            |              |             |             |       |         |           |           |       |           |
|  **EFCoreUpdateBatchAsync** |       **1000** |  **60,843.6 μs** |   **302.96 μs** |   **252.99 μs** |  **1.00** |    **0.00** |  **222.2222** |  **111.1111** |     **-** |  **5,711 KB** |
| VenflowUpdateBatchAsync |       1000 |  41,804.8 μs |   110.39 μs |    97.86 μs |  0.69 |    0.00 |         - |         - |     - |  1,503 KB |
|  RepoDbUpdateBatchAsync |       1000 |  27,049.5 μs |   398.25 μs |   372.53 μs |  0.45 |    0.01 |   31.2500 |         - |     - |    873 KB |
|                         |            |              |             |             |       |         |           |           |       |           |
|  **EFCoreUpdateBatchAsync** |      **10000** | **678,159.3 μs** | **2,141.07 μs** | **2,002.76 μs** |  **1.00** |    **0.00** | **2000.0000** | **1000.0000** |     **-** | **57,207 KB** |
| VenflowUpdateBatchAsync |      10000 | 444,538.8 μs | 2,506.85 μs | 2,344.91 μs |  0.66 |    0.00 |         - |         - |     - | 15,208 KB |
|  RepoDbUpdateBatchAsync |      10000 | 446,374.4 μs | 4,601.09 μs | 4,078.74 μs |  0.66 |    0.01 |         - |         - |     - |  8,891 KB |
