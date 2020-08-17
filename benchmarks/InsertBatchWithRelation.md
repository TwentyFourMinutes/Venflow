``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.388 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.7.20366.6
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4084.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT


```
|                  Method |           Job |       Runtime | InsertCount |          Mean |       Error |     StdDev |        Median | Ratio |       Gen 0 |       Gen 1 | Gen 2 |     Allocated |
|------------------------ |-------------- |-------------- |------------ |--------------:|------------:|-----------:|--------------:|------:|------------:|------------:|------:|--------------:|
|  **EfCoreInsertBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |          **10** |     **13.699 ms** |   **0.2737 ms** |  **0.4261 ms** |     **13.519 ms** |  **1.00** |    **343.7500** |     **93.7500** |     **-** |    **1323.95 KB** |
| VenflowInsertBatchAsync |      .NET 4.8 |      .NET 4.8 |          10 |      4.509 ms |   0.0443 ms |  0.0346 ms |      4.516 ms |  0.32 |     39.0625 |           - |     - |      124.7 KB |
|                         |               |               |             |               |             |            |               |       |             |             |       |               |
|  EfCoreInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     11.923 ms |   0.1553 ms |  0.1453 ms |     11.896 ms |  1.00 |    312.5000 |     93.7500 |     - |    1296.72 KB |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |      4.372 ms |   0.0693 ms |  0.0615 ms |      4.388 ms |  0.37 |     31.2500 |           - |     - |     100.13 KB |
|                         |               |               |             |               |             |            |               |       |             |             |       |               |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     11.887 ms |   0.1179 ms |  0.1103 ms |     11.877 ms |  1.00 |    312.5000 |     93.7500 |     - |     1222.9 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |      4.321 ms |   0.0813 ms |  0.0760 ms |      4.289 ms |  0.36 |     31.2500 |           - |     - |      99.99 KB |
|                         |               |               |             |               |             |            |               |       |             |             |       |               |
|  **EfCoreInsertBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |         **100** |     **99.006 ms** |   **1.1307 ms** |  **1.0576 ms** |     **98.943 ms** |  **1.00** |   **2333.3333** |    **833.3333** |     **-** |   **12976.32 KB** |
| VenflowInsertBatchAsync |      .NET 4.8 |      .NET 4.8 |         100 |     16.083 ms |   0.3192 ms |  0.7771 ms |     15.724 ms |  0.16 |    218.7500 |     93.7500 |     - |     1120.7 KB |
|                         |               |               |             |               |             |            |               |       |             |             |       |               |
|  EfCoreInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |     88.715 ms |   1.7039 ms |  2.0284 ms |     88.364 ms |  1.00 |   2333.3333 |    833.3333 |     - |   13569.11 KB |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |     15.419 ms |   0.3069 ms |  0.7756 ms |     15.178 ms |  0.18 |    156.2500 |     62.5000 |     - |     958.88 KB |
|                         |               |               |             |               |             |            |               |       |             |             |       |               |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     87.756 ms |   1.6235 ms |  1.5186 ms |     87.613 ms |  1.00 |   1833.3333 |    833.3333 |     - |   12046.88 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     14.971 ms |   0.2097 ms |  0.2870 ms |     14.912 ms |  0.17 |    187.5000 |     62.5000 |     - |     958.86 KB |
|                         |               |               |             |               |             |            |               |       |             |             |       |               |
|  **EfCoreInsertBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |        **1000** |    **954.145 ms** |  **17.6873 ms** | **22.3688 ms** |    **942.217 ms** |  **1.00** |  **21000.0000** |   **7000.0000** |     **-** |  **129673.73 KB** |
| VenflowInsertBatchAsync |      .NET 4.8 |      .NET 4.8 |        1000 |    140.394 ms |   2.7920 ms |  3.1033 ms |    140.141 ms |  0.15 |   1750.0000 |    750.0000 |     - |   10993.67 KB |
|                         |               |               |             |               |             |            |               |       |             |             |       |               |
|  EfCoreInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |    835.049 ms |  12.1475 ms | 11.3628 ms |    836.534 ms |  1.00 |  20000.0000 |   6000.0000 |     - |   121591.3 KB |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |    128.526 ms |   2.2191 ms |  1.8530 ms |    128.688 ms |  0.15 |   1500.0000 |    500.0000 |     - |    9423.86 KB |
|                         |               |               |             |               |             |            |               |       |             |             |       |               |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    820.349 ms |  13.3810 ms | 11.8619 ms |    818.684 ms |  1.00 |  19000.0000 |   6000.0000 |     - |  120622.63 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    124.470 ms |   2.4876 ms |  4.6723 ms |    123.153 ms |  0.15 |   1250.0000 |    500.0000 |     - |    9423.89 KB |
|                         |               |               |             |               |             |            |               |       |             |             |       |               |
|  **EfCoreInsertBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |       **10000** | **10,154.197 ms** |  **88.7932 ms** | **78.7129 ms** | **10,160.089 ms** |  **1.00** | **224000.0000** |  **58000.0000** |     **-** | **1297188.83 KB** |
| VenflowInsertBatchAsync |      .NET 4.8 |      .NET 4.8 |       10000 |  1,506.248 ms |  27.9960 ms | 24.8178 ms |  1,507.958 ms |  0.15 |  15000.0000 |   6000.0000 |     - |  118138.84 KB |
|                         |               |               |             |               |             |            |               |       |             |             |       |               |
|  EfCoreInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |  9,188.216 ms | 103.0624 ms | 96.4046 ms |  9,164.420 ms |  1.00 | 210000.0000 | 123000.0000 |     - | 1216659.17 KB |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |  1,415.822 ms |  27.9538 ms | 47.4677 ms |  1,417.292 ms |  0.15 |  12000.0000 |   5000.0000 |     - |  101155.63 KB |
|                         |               |               |             |               |             |            |               |       |             |             |       |               |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |  9,073.539 ms | 101.4962 ms | 89.9737 ms |  9,070.254 ms |  1.00 | 208000.0000 |  52000.0000 |     - | 1206967.77 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |  1,398.576 ms |  26.7533 ms | 66.6249 ms |  1,406.578 ms |  0.15 |  12000.0000 |   4000.0000 |     - |  101130.58 KB |
