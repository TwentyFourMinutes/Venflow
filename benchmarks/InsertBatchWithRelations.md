``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                  Method | BatchCount |         Mean |       Error |      StdDev | Ratio |      Gen 0 |      Gen 1 | Gen 2 |  Allocated |
|------------------------ |----------- |-------------:|------------:|------------:|------:|-----------:|-----------:|------:|-----------:|
|  **EfCoreInsertBatchAsync** |         **10** |    **12.148 ms** |   **0.2411 ms** |   **0.3753 ms** |  **1.00** |    **31.2500** |    **15.6250** |     **-** |     **948 KB** |
| VenflowInsertBatchAsync |         10 |     2.921 ms |   0.0581 ms |   0.0734 ms |  0.24 |          - |          - |     - |      82 KB |
|                         |            |              |             |             |       |            |            |       |            |
|  **EfCoreInsertBatchAsync** |        **100** |    **92.083 ms** |   **1.7957 ms** |   **3.0975 ms** |  **1.00** |   **333.3333** |   **166.6667** |     **-** |   **9,351 KB** |
| VenflowInsertBatchAsync |        100 |    12.634 ms |   0.2520 ms |   0.4070 ms |  0.14 |    31.2500 |    15.6250 |     - |     807 KB |
|                         |            |              |             |             |       |            |            |       |            |
|  **EfCoreInsertBatchAsync** |       **1000** |   **870.842 ms** |  **16.9286 ms** |  **15.8350 ms** |  **1.00** |  **3000.0000** |  **1000.0000** |     **-** |  **92,603 KB** |
| VenflowInsertBatchAsync |       1000 |   108.090 ms |   2.0496 ms |   2.4399 ms |  0.12 |   200.0000 |          - |     - |   7,889 KB |
|                         |            |              |             |             |       |            |            |       |            |
|  **EfCoreInsertBatchAsync** |      **10000** | **8,802.902 ms** | **117.3807 ms** | **109.7980 ms** |  **1.00** | **34000.0000** | **12000.0000** |     **-** | **926,229 KB** |
| VenflowInsertBatchAsync |      10000 | 1,133.496 ms |  18.0779 ms |  15.0958 ms |  0.13 |  2000.0000 |  1000.0000 |     - |  79,246 KB |
