``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                  Method | InsertCount |         Mean |      Error |     StdDev | Ratio |      Gen 0 |      Gen 1 | Gen 2 |  Allocated |
|------------------------ |------------ |-------------:|-----------:|-----------:|------:|-----------:|-----------:|------:|-----------:|
|  **EfCoreInsertBatchAsync** |          **10** |    **11.423 ms** |  **0.2285 ms** |  **0.2720 ms** |  **1.00** |    **31.2500** |    **15.6250** |     **-** |     **948 KB** |
| VenflowInsertBatchAsync |          10 |     2.754 ms |  0.0544 ms |  0.1061 ms |  0.24 |          - |          - |     - |      82 KB |
|                         |             |              |            |            |       |            |            |       |            |
|  **EfCoreInsertBatchAsync** |         **100** |    **84.258 ms** |  **1.3624 ms** |  **1.2077 ms** |  **1.00** |   **285.7143** |          **-** |     **-** |   **9,352 KB** |
| VenflowInsertBatchAsync |         100 |    11.312 ms |  0.2210 ms |  0.2951 ms |  0.13 |    31.2500 |    15.6250 |     - |     807 KB |
|                         |             |              |            |            |       |            |            |       |            |
|  **EfCoreInsertBatchAsync** |        **1000** |   **798.905 ms** | **15.2519 ms** | **14.2667 ms** |  **1.00** |  **3000.0000** |  **1000.0000** |     **-** |  **92,601 KB** |
| VenflowInsertBatchAsync |        1000 |    99.195 ms |  1.9619 ms |  2.9960 ms |  0.12 |   200.0000 |          - |     - |   7,889 KB |
|                         |             |              |            |            |       |            |            |       |            |
|  **EfCoreInsertBatchAsync** |       **10000** | **8,108.619 ms** | **51.5469 ms** | **45.6950 ms** |  **1.00** | **34000.0000** | **12000.0000** |     **-** | **926,231 KB** |
| VenflowInsertBatchAsync |       10000 | 1,002.862 ms | 17.2282 ms | 19.1491 ms |  0.12 |  2000.0000 |  1000.0000 |     - |  79,238 KB |
