``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.508 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-rc.1.20452.10
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4220.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT


```
|                  Method |           Job |       Runtime | InsertCount |         Mean |       Error |      StdDev | Ratio |       Gen 0 |      Gen 1 | Gen 2 |     Allocated |
|------------------------ |-------------- |-------------- |------------ |-------------:|------------:|------------:|------:|------------:|-----------:|------:|--------------:|
|  **EfCoreInsertBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |          **10** |    **13.828 ms** |   **0.2388 ms** |   **0.2234 ms** |  **1.00** |    **343.7500** |   **109.3750** |     **-** |    **1325.29 KB** |
| VenflowInsertBatchAsync |      .NET 4.8 |      .NET 4.8 |          10 |     4.458 ms |   0.0722 ms |   0.0603 ms |  0.32 |     39.0625 |          - |     - |        123 KB |
|                         |               |               |             |              |             |             |       |             |            |       |               |
|  EfCoreInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |    12.036 ms |   0.1711 ms |   0.1601 ms |  1.00 |    312.5000 |    78.1250 |     - |    1232.38 KB |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     4.339 ms |   0.0808 ms |   0.0716 ms |  0.36 |     31.2500 |          - |     - |      98.15 KB |
|                         |               |               |             |              |             |             |       |             |            |       |               |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |    10.729 ms |   0.1500 ms |   0.1330 ms |  1.00 |    218.7500 |    62.5000 |     - |     990.27 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     4.302 ms |   0.0833 ms |   0.0926 ms |  0.40 |     31.2500 |          - |     - |      98.12 KB |
|                         |               |               |             |              |             |             |       |             |            |       |               |
|  **EfCoreInsertBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |         **100** |   **100.956 ms** |   **1.9521 ms** |   **2.0887 ms** |  **1.00** |   **2400.0000** |   **800.0000** |     **-** |      **12974 KB** |
| VenflowInsertBatchAsync |      .NET 4.8 |      .NET 4.8 |         100 |    15.503 ms |   0.3076 ms |   0.3021 ms |  0.15 |    187.5000 |    93.7500 |     - |    1092.64 KB |
|                         |               |               |             |              |             |             |       |             |            |       |               |
|  EfCoreInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |    89.117 ms |   1.7055 ms |   1.9640 ms |  1.00 |   2333.3333 |   833.3333 |     - |   13569.21 KB |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |    14.940 ms |   0.2895 ms |   0.3555 ms |  0.17 |    187.5000 |    62.5000 |     - |     945.77 KB |
|                         |               |               |             |              |             |             |       |             |            |       |               |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |    76.632 ms |   1.2710 ms |   1.1889 ms |  1.00 |   1500.0000 |   666.6667 |     - |    9721.12 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |    14.859 ms |   0.2435 ms |   0.3166 ms |  0.19 |    156.2500 |    62.5000 |     - |     945.77 KB |
|                         |               |               |             |              |             |             |       |             |            |       |               |
|  **EfCoreInsertBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |        **1000** |   **958.324 ms** |  **18.0846 ms** |  **20.1010 ms** |  **1.00** |  **21000.0000** |  **7000.0000** |     **-** |  **129673.59 KB** |
| VenflowInsertBatchAsync |      .NET 4.8 |      .NET 4.8 |        1000 |   136.206 ms |   2.2016 ms |   2.0594 ms |  0.14 |   1500.0000 |   500.0000 |     - |    11101.3 KB |
|                         |               |               |             |              |             |             |       |             |            |       |               |
|  EfCoreInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |   850.566 ms |  16.2344 ms |  19.3259 ms |  1.00 |  20000.0000 |  6000.0000 |     - |  121589.82 KB |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |   128.942 ms |   2.5060 ms |   2.5734 ms |  0.15 |   1250.0000 |   500.0000 |     - |    9279.95 KB |
|                         |               |               |             |              |             |             |       |             |            |       |               |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |   726.332 ms |  14.2392 ms |  14.6226 ms |  1.00 |  16000.0000 |  6000.0000 |     - |   97337.02 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |   125.938 ms |   2.4455 ms |   3.4282 ms |  0.17 |   1250.0000 |   250.0000 |     - |    9279.98 KB |
|                         |               |               |             |              |             |             |       |             |            |       |               |
|  **EfCoreInsertBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |       **10000** | **9,655.693 ms** |  **90.2144 ms** |  **84.3866 ms** |  **1.00** | **224000.0000** | **67000.0000** |     **-** | **1297126.38 KB** |
| VenflowInsertBatchAsync |      .NET 4.8 |      .NET 4.8 |       10000 | 1,396.346 ms |  27.8574 ms |  53.6718 ms |  0.14 |  14000.0000 |  6000.0000 |     - |  118028.73 KB |
|                         |               |               |             |              |             |             |       |             |            |       |               |
|  EfCoreInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 | 8,565.161 ms | 136.0222 ms | 127.2352 ms |  1.00 | 210000.0000 | 51000.0000 |     - | 1216678.33 KB |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 | 1,284.977 ms |  25.6707 ms |  24.0124 ms |  0.15 |  12000.0000 |  5000.0000 |     - |   99307.88 KB |
|                         |               |               |             |              |             |             |       |             |            |       |               |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 7,406.901 ms | 111.6059 ms |  98.9357 ms |  1.00 | 168000.0000 | 38000.0000 |     - |  973668.32 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 1,286.968 ms |  25.3139 ms |  37.8886 ms |  0.17 |  12000.0000 |  5000.0000 |     - |   99310.07 KB |
