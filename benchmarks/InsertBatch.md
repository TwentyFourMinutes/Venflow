``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.508 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-rc.1.20452.10
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4220.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT


```
|                  Method |           Job |       Runtime | InsertCount |       Mean |      Error |     StdDev |     Median | Ratio | RatioSD |      Gen 0 |     Gen 1 |    Gen 2 |    Allocated |
|------------------------ |-------------- |-------------- |------------ |-----------:|-----------:|-----------:|-----------:|------:|--------:|-----------:|----------:|---------:|-------------:|
|  **EfCoreInsertBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |          **10** |   **2.653 ms** |  **0.0525 ms** |  **0.1119 ms** |   **2.641 ms** |  **1.00** |    **0.00** |    **42.9688** |   **11.7188** |        **-** |    **143.82 KB** |
| VenflowInsertBatchAsync |      .NET 4.8 |      .NET 4.8 |          10 |   1.146 ms |  0.0226 ms |  0.0278 ms |   1.146 ms |  0.44 |    0.02 |     3.9063 |         - |        - |      15.8 KB |
|  RepoDbInsertBatchAsync |      .NET 4.8 |      .NET 4.8 |          10 |   1.356 ms |  0.0266 ms |  0.0222 ms |   1.363 ms |  0.52 |    0.02 |     5.8594 |         - |        - |     22.69 KB |
|                         |               |               |             |            |            |            |            |       |         |            |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |   2.365 ms |  0.0471 ms |  0.0628 ms |   2.375 ms |  1.00 |    0.00 |    39.0625 |    7.8125 |        - |     126.7 KB |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |   1.098 ms |  0.0216 ms |  0.0222 ms |   1.094 ms |  0.46 |    0.02 |     1.9531 |         - |        - |     11.01 KB |
|  RepoDbInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |   1.367 ms |  0.0268 ms |  0.0503 ms |   1.364 ms |  0.57 |    0.03 |     3.9063 |         - |        - |     16.95 KB |
|                         |               |               |             |            |            |            |            |       |         |            |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |   2.260 ms |  0.0436 ms |  0.0596 ms |   2.264 ms |  1.00 |    0.00 |    31.2500 |    3.9063 |        - |    105.64 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |   1.102 ms |  0.0220 ms |  0.0468 ms |   1.103 ms |  0.48 |    0.03 |          - |         - |        - |        11 KB |
|  RepoDbInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |   1.237 ms |  0.0742 ms |  0.2187 ms |   1.118 ms |  0.65 |    0.07 |     3.9063 |         - |        - |      16.9 KB |
|                         |               |               |             |            |            |            |            |       |         |            |           |          |              |
|  **EfCoreInsertBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |         **100** |  **12.157 ms** |  **0.2376 ms** |  **0.2641 ms** |  **12.073 ms** |  **1.00** |    **0.00** |   **265.6250** |   **78.1250** |        **-** |   **1294.08 KB** |
| VenflowInsertBatchAsync |      .NET 4.8 |      .NET 4.8 |         100 |   1.817 ms |  0.0532 ms |  0.1510 ms |   1.817 ms |  0.15 |    0.01 |    31.2500 |         - |        - |    100.95 KB |
|  RepoDbInsertBatchAsync |      .NET 4.8 |      .NET 4.8 |         100 |   4.394 ms |  0.0875 ms |  0.1107 ms |   4.359 ms |  0.36 |    0.01 |    54.6875 |         - |        - |    182.45 KB |
|                         |               |               |             |            |            |            |            |       |         |            |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |  10.360 ms |  0.1605 ms |  0.1423 ms |  10.316 ms |  1.00 |    0.00 |   265.6250 |   78.1250 |        - |   1194.27 KB |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |   1.769 ms |  0.0348 ms |  0.0582 ms |   1.761 ms |  0.17 |    0.01 |    27.3438 |         - |        - |     87.36 KB |
|  RepoDbInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |   4.371 ms |  0.0864 ms |  0.2103 ms |   4.348 ms |  0.43 |    0.01 |    39.0625 |         - |        - |    137.84 KB |
|                         |               |               |             |            |            |            |            |       |         |            |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   9.552 ms |  0.1591 ms |  0.1489 ms |   9.467 ms |  1.00 |    0.00 |   218.7500 |   62.5000 |        - |    978.38 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   1.723 ms |  0.0337 ms |  0.0315 ms |   1.717 ms |  0.18 |    0.00 |    27.3438 |         - |        - |     87.35 KB |
|  RepoDbInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   4.533 ms |  0.0905 ms |  0.1145 ms |   4.542 ms |  0.47 |    0.02 |    39.0625 |         - |        - |    137.72 KB |
|                         |               |               |             |            |            |            |            |       |         |            |           |          |              |
|  **EfCoreInsertBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |        **1000** |  **94.875 ms** |  **1.6235 ms** |  **1.4392 ms** |  **94.638 ms** |  **1.00** |    **0.00** |  **2000.0000** |  **833.3333** |        **-** |  **12763.36 KB** |
| VenflowInsertBatchAsync |      .NET 4.8 |      .NET 4.8 |        1000 |   8.634 ms |  0.1716 ms |  0.2348 ms |   8.634 ms |  0.09 |    0.00 |   156.2500 |   78.1250 |        - |    967.98 KB |
|  RepoDbInsertBatchAsync |      .NET 4.8 |      .NET 4.8 |        1000 |  37.375 ms |  0.7261 ms |  0.6792 ms |  37.113 ms |  0.39 |    0.01 |   571.4286 |         - |        - |   1776.75 KB |
|                         |               |               |             |            |            |            |            |       |         |            |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |  86.868 ms |  1.1325 ms |  1.0039 ms |  86.998 ms |  1.00 |    0.00 |  1800.0000 |  800.0000 |        - |  11809.35 KB |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |   8.126 ms |  0.1543 ms |  0.1368 ms |   8.152 ms |  0.09 |    0.00 |   140.6250 |   62.5000 |        - |    853.33 KB |
|  RepoDbInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |  36.468 ms |  0.7281 ms |  2.1354 ms |  36.878 ms |  0.38 |    0.02 |   384.6154 |   76.9231 |        - |   1337.66 KB |
|                         |               |               |             |            |            |            |            |       |         |            |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |  76.624 ms |  1.3681 ms |  1.3437 ms |  76.439 ms |  1.00 |    0.00 |  1428.5714 |  571.4286 |        - |   9645.99 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |   8.008 ms |  0.1531 ms |  0.1937 ms |   8.032 ms |  0.10 |    0.00 |   125.0000 |   62.5000 |        - |    853.32 KB |
|  RepoDbInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |  35.522 ms |  0.7618 ms |  2.2463 ms |  35.636 ms |  0.45 |    0.03 |   400.0000 |  133.3333 |        - |   1336.87 KB |
|                         |               |               |             |            |            |            |            |       |         |            |           |          |              |
|  **EfCoreInsertBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |       **10000** | **938.914 ms** | **18.6815 ms** | **20.7645 ms** | **934.892 ms** |  **1.00** |    **0.00** | **21000.0000** | **7000.0000** |        **-** | **127685.58 KB** |
| VenflowInsertBatchAsync |      .NET 4.8 |      .NET 4.8 |       10000 |  96.209 ms |  3.6724 ms | 10.7126 ms |  91.850 ms |  0.11 |    0.01 |  1500.0000 |  666.6667 | 166.6667 |  10075.13 KB |
|  RepoDbInsertBatchAsync |      .NET 4.8 |      .NET 4.8 |       10000 | 601.179 ms | 11.8129 ms | 19.7367 ms | 595.889 ms |  0.64 |    0.02 |  5000.0000 | 1000.0000 |        - |  18050.65 KB |
|                         |               |               |             |            |            |            |            |       |         |            |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 | 849.826 ms | 16.9767 ms | 14.1763 ms | 853.742 ms |  1.00 |    0.00 | 19000.0000 | 7000.0000 |        - | 118204.25 KB |
| VenflowInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |  86.719 ms |  2.7068 ms |  7.8098 ms |  83.262 ms |  0.10 |    0.01 |  1285.7143 |  571.4286 | 142.8571 |   8530.35 KB |
|  RepoDbInsertBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 | 465.992 ms |  9.2702 ms | 21.2999 ms | 458.318 ms |  0.57 |    0.03 |  3000.0000 | 1000.0000 |        - |   13432.8 KB |
|                         |               |               |             |            |            |            |            |       |         |            |           |          |              |
|  EfCoreInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 752.093 ms | 14.0007 ms | 13.0963 ms | 750.631 ms |  1.00 |    0.00 | 16000.0000 | 4000.0000 |        - |  96562.56 KB |
| VenflowInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |  78.693 ms |  1.5631 ms |  2.7784 ms |  78.245 ms |  0.11 |    0.00 |  1285.7143 |  571.4286 | 142.8571 |   8531.74 KB |
|  RepoDbInsertBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 476.596 ms |  9.5096 ms | 23.6823 ms | 475.564 ms |  0.62 |    0.04 |  3000.0000 | 1000.0000 |        - |  13425.68 KB |
