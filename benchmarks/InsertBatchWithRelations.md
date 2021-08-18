``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.7.21379.14
  [Host]   : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                  Method | BatchCount |         Mean |       Error |      StdDev | Ratio |      Gen 0 |      Gen 1 | Gen 2 |  Allocated |
|------------------------ |----------- |-------------:|------------:|------------:|------:|-----------:|-----------:|------:|-----------:|
|  **EfCoreInsertBatchAsync** |         **10** |    **11.302 ms** |   **0.1364 ms** |   **0.1209 ms** |  **1.00** |    **31.2500** |    **15.6250** |     **-** |     **969 KB** |
| VenflowInsertBatchAsync |         10 |     2.682 ms |   0.0513 ms |   0.0527 ms |  0.24 |          - |          - |     - |      82 KB |
|                         |            |              |             |             |       |            |            |       |            |
|  **EfCoreInsertBatchAsync** |        **100** |    **83.651 ms** |   **1.6702 ms** |   **1.9883 ms** |  **1.00** |   **333.3333** |   **166.6667** |     **-** |  **10,982 KB** |
| VenflowInsertBatchAsync |        100 |    11.452 ms |   0.2147 ms |   0.2008 ms |  0.14 |    31.2500 |    15.6250 |     - |     807 KB |
|                         |            |              |             |             |       |            |            |       |            |
|  **EfCoreInsertBatchAsync** |       **1000** |   **813.300 ms** |  **11.7411 ms** |   **9.8043 ms** |  **1.00** |  **3000.0000** |  **1000.0000** |     **-** |  **94,187 KB** |
| VenflowInsertBatchAsync |       1000 |   103.848 ms |   2.0310 ms |   3.5571 ms |  0.13 |   200.0000 |          - |     - |   7,888 KB |
|                         |            |              |             |             |       |            |            |       |            |
|  **EfCoreInsertBatchAsync** |      **10000** | **8,422.345 ms** | **113.1848 ms** | **105.8732 ms** |  **1.00** | **34000.0000** | **12000.0000** |     **-** | **942,093 KB** |
| VenflowInsertBatchAsync |      10000 | 1,067.643 ms |  21.1191 ms |  22.5972 ms |  0.13 |  2000.0000 |  1000.0000 |     - |  79,240 KB |
