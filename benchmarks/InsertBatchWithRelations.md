``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                  Method | BatchCount |         Mean |      Error |     StdDev | Ratio |      Gen 0 |      Gen 1 | Gen 2 |  Allocated |
|------------------------ |----------- |-------------:|-----------:|-----------:|------:|-----------:|-----------:|------:|-----------:|
|  **EfCoreInsertBatchAsync** |         **10** |     **7.927 ms** |  **0.1097 ms** |  **0.1026 ms** |  **1.00** |    **46.8750** |    **15.6250** |     **-** |     **948 KB** |
| VenflowInsertBatchAsync |         10 |     1.976 ms |  0.0386 ms |  0.0414 ms |  0.25 |     3.9063 |          - |     - |      82 KB |
|                         |            |              |            |            |       |            |            |       |            |
|  **EfCoreInsertBatchAsync** |        **100** |    **57.915 ms** |  **0.5067 ms** |  **0.4740 ms** |  **1.00** |   **444.4444** |   **111.1111** |     **-** |   **9,351 KB** |
| VenflowInsertBatchAsync |        100 |     9.145 ms |  0.0715 ms |  0.0668 ms |  0.16 |    31.2500 |    15.6250 |     - |     807 KB |
|                         |            |              |            |            |       |            |            |       |            |
|  **EfCoreInsertBatchAsync** |       **1000** |   **546.602 ms** |  **6.9519 ms** |  **6.5029 ms** |  **1.00** |  **4000.0000** |  **2000.0000** |     **-** |  **92,595 KB** |
| VenflowInsertBatchAsync |       1000 |    85.951 ms |  0.4698 ms |  0.4395 ms |  0.16 |   333.3333 |   166.6667 |     - |   7,889 KB |
|                         |            |              |            |            |       |            |            |       |            |
|  **EfCoreInsertBatchAsync** |      **10000** | **5,911.649 ms** | **61.0757 ms** | **57.1303 ms** |  **1.00** | **47000.0000** | **35000.0000** |     **-** | **926,217 KB** |
| VenflowInsertBatchAsync |      10000 |   897.786 ms |  7.9869 ms |  7.0802 ms |  0.15 |  3000.0000 |  1000.0000 |     - |  79,239 KB |
