``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                  Method | BatchCount |       Mean |     Error |    StdDev | Ratio | RatioSD |     Gen 0 |     Gen 1 | Gen 2 |  Allocated |
|------------------------ |----------- |-----------:|----------:|----------:|------:|--------:|----------:|----------:|------:|-----------:|
|  **EFCoreDeleteBatchAsync** |         **10** |   **1.952 ms** | **0.0375 ms** | **0.0332 ms** |  **1.00** |    **0.00** |    **3.9063** |         **-** |     **-** |      **80 KB** |
| VenflowDeleteBatchAsync |         10 |   1.227 ms | 0.0226 ms | 0.0211 ms |  0.63 |    0.01 |         - |         - |     - |      17 KB |
|  RepoDbDeleteBatchAsync |         10 |   1.722 ms | 0.0856 ms | 0.2523 ms |  0.70 |    0.03 |         - |         - |     - |      28 KB |
|                         |            |            |           |           |       |         |           |           |       |            |
|  **EFCoreDeleteBatchAsync** |        **100** |   **7.495 ms** | **0.1486 ms** | **0.1460 ms** |  **1.00** |    **0.00** |   **31.2500** |         **-** |     **-** |     **707 KB** |
| VenflowDeleteBatchAsync |        100 |   2.567 ms | 0.0510 ms | 0.0608 ms |  0.34 |    0.01 |    3.9063 |         - |     - |     112 KB |
|  RepoDbDeleteBatchAsync |        100 |   2.970 ms | 0.0369 ms | 0.0345 ms |  0.40 |    0.01 |    7.8125 |         - |     - |     155 KB |
|                         |            |            |           |           |       |         |           |           |       |            |
|  **EFCoreDeleteBatchAsync** |       **1000** |  **59.981 ms** | **0.3385 ms** | **0.3001 ms** |  **1.00** |    **0.00** |  **333.3333** |  **111.1111** |     **-** |   **6,901 KB** |
| VenflowDeleteBatchAsync |       1000 |  12.899 ms | 0.0920 ms | 0.0861 ms |  0.22 |    0.00 |   46.8750 |   15.6250 |     - |   1,077 KB |
|  RepoDbDeleteBatchAsync |       1000 |  29.374 ms | 0.1637 ms | 0.1531 ms |  0.49 |    0.00 |  125.0000 |   62.5000 |     - |   2,681 KB |
|                         |            |            |           |           |       |         |           |           |       |            |
|  **EFCoreDeleteBatchAsync** |      **10000** | **625.344 ms** | **2.9668 ms** | **2.7751 ms** |  **1.00** |    **0.00** | **3000.0000** | **1000.0000** |     **-** |  **69,047 KB** |
| VenflowDeleteBatchAsync |      10000 | 132.765 ms | 0.7132 ms | 0.6671 ms |  0.21 |    0.00 |  500.0000 |  250.0000 |     - |  10,751 KB |
|  RepoDbDeleteBatchAsync |      10000 | 410.240 ms | 2.2164 ms | 1.9648 ms |  0.66 |    0.00 | 5000.0000 | 1000.0000 |     - | 106,609 KB |
