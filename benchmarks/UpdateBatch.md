``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.7.21379.14
  [Host]   : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                  Method | BatchCount |           Mean |        Error |       StdDev | Ratio | RatioSD |     Gen 0 |     Gen 1 | Gen 2 | Allocated |
|------------------------ |----------- |---------------:|-------------:|-------------:|------:|--------:|----------:|----------:|------:|----------:|
|  **EFCoreUpdateBatchAsync** |         **10** |     **2,047.1 μs** |     **39.84 μs** |     **55.84 μs** |  **1.00** |    **0.00** |         **-** |         **-** |     **-** |     **66 KB** |
| VenflowUpdateBatchAsync |         10 |     1,712.7 μs |     27.30 μs |     25.54 μs |  0.85 |    0.02 |         - |         - |     - |     19 KB |
|  RepoDbUpdateBatchAsync |         10 |       931.2 μs |     18.27 μs |     23.11 μs |  0.46 |    0.02 |         - |         - |     - |     11 KB |
|                         |            |                |              |              |       |         |           |           |       |           |
|  **EFCoreUpdateBatchAsync** |        **100** |    **10,669.8 μs** |    **210.20 μs** |    **258.14 μs** |  **1.00** |    **0.00** |   **15.6250** |         **-** |     **-** |    **578 KB** |
| VenflowUpdateBatchAsync |        100 |     8,131.2 μs |    157.70 μs |    147.52 μs |  0.76 |    0.03 |         - |         - |     - |    171 KB |
|  RepoDbUpdateBatchAsync |        100 |     4,651.1 μs |     84.46 μs |    133.97 μs |  0.44 |    0.02 |         - |         - |     - |     91 KB |
|                         |            |                |              |              |       |         |           |           |       |           |
|  **EFCoreUpdateBatchAsync** |       **1000** |    **96,943.3 μs** |  **1,935.61 μs** |  **1,987.73 μs** |  **1.00** |    **0.00** |  **200.0000** |         **-** |     **-** |  **5,719 KB** |
| VenflowUpdateBatchAsync |       1000 |    71,576.6 μs |  1,245.41 μs |  1,039.97 μs |  0.74 |    0.02 |         - |         - |     - |  1,664 KB |
|  RepoDbUpdateBatchAsync |       1000 |    40,474.3 μs |    775.30 μs |  1,251.96 μs |  0.42 |    0.02 |         - |         - |     - |    882 KB |
|                         |            |                |              |              |       |         |           |           |       |           |
|  **EFCoreUpdateBatchAsync** |      **10000** | **1,063,995.3 μs** | **15,707.91 μs** | **13,924.66 μs** |  **1.00** |    **0.00** | **2000.0000** | **1000.0000** |     **-** | **57,290 KB** |
| VenflowUpdateBatchAsync |      10000 |   776,897.6 μs | 14,612.44 μs | 12,202.05 μs |  0.73 |    0.01 |         - |         - |     - | 16,767 KB |
|  RepoDbUpdateBatchAsync |      10000 |   623,146.7 μs | 12,358.01 μs | 23,512.39 μs |  0.60 |    0.03 |         - |         - |     - |  8,864 KB |
