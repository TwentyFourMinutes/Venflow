``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                  Method | BatchCount |       Mean |      Error |     StdDev |     Median | Ratio | RatioSD |     Gen 0 |     Gen 1 | Gen 2 | Allocated |
|------------------------ |----------- |-----------:|-----------:|-----------:|-----------:|------:|--------:|----------:|----------:|------:|----------:|
|  **EFCoreDeleteBatchAsync** |         **10** |   **3.093 ms** |  **0.0599 ms** |  **0.0933 ms** |   **3.096 ms** |  **1.00** |    **0.00** |         **-** |         **-** |     **-** |     **80 KB** |
| VenflowDeleteBatchAsync |         10 |   1.866 ms |  0.0357 ms |  0.0382 ms |   1.866 ms |  0.60 |    0.02 |         - |         - |     - |     17 KB |
|  RepoDbDeleteBatchAsync |         10 |   1.994 ms |  0.0388 ms |  0.0557 ms |   1.999 ms |  0.64 |    0.02 |         - |         - |     - |     28 KB |
|                         |            |            |            |            |            |       |         |           |           |       |           |
|  **EFCoreDeleteBatchAsync** |        **100** |  **11.620 ms** |  **0.2235 ms** |  **0.2660 ms** |  **11.665 ms** |  **1.00** |    **0.00** |   **15.6250** |         **-** |     **-** |    **707 KB** |
| VenflowDeleteBatchAsync |        100 |   4.499 ms |  0.6173 ms |  1.7712 ms |   3.554 ms |  0.37 |    0.10 |    3.9063 |         - |     - |    112 KB |
|  RepoDbDeleteBatchAsync |        100 |   4.389 ms |  0.3414 ms |  0.9741 ms |   3.882 ms |  0.45 |    0.06 |         - |         - |     - |    154 KB |
|                         |            |            |            |            |            |       |         |           |           |       |           |
|  **EFCoreDeleteBatchAsync** |       **1000** |  **94.278 ms** |  **1.7710 ms** |  **1.6566 ms** |  **93.786 ms** |  **1.00** |    **0.00** |  **166.6667** |         **-** |     **-** |  **6,901 KB** |
| VenflowDeleteBatchAsync |       1000 |  16.107 ms |  0.3159 ms |  0.4325 ms |  16.078 ms |  0.17 |    0.01 |   31.2500 |         - |     - |  1,069 KB |
|  RepoDbDeleteBatchAsync |       1000 |  39.160 ms |  1.1284 ms |  3.3271 ms |  39.815 ms |  0.39 |    0.03 |   66.6667 |         - |     - |  2,605 KB |
|                         |            |            |            |            |            |       |         |           |           |       |           |
|  **EFCoreDeleteBatchAsync** |      **10000** | **965.767 ms** | **16.2871 ms** | **18.1031 ms** | **966.431 ms** |  **1.00** |    **0.00** | **2000.0000** | **1000.0000** |     **-** | **69,047 KB** |
| VenflowDeleteBatchAsync |      10000 | 153.077 ms |  3.0073 ms |  3.5800 ms | 153.452 ms |  0.16 |    0.01 |  250.0000 |         - |     - | 10,745 KB |
|  RepoDbDeleteBatchAsync |      10000 | 496.097 ms |  9.7474 ms | 14.5894 ms | 496.408 ms |  0.52 |    0.02 | 3000.0000 | 1000.0000 |     - | 95,121 KB |
