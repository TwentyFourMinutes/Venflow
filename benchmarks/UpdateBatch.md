``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.388 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.7.20366.6
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4084.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT


```
|                  Method |           Job |       Runtime | UpdateCount |         Mean |      Error |     StdDev |       Median | Ratio | RatioSD |      Gen 0 |     Gen 1 | Gen 2 |    Allocated |
|------------------------ |-------------- |-------------- |------------ |-------------:|-----------:|-----------:|-------------:|------:|--------:|-----------:|----------:|------:|-------------:|
|  **EFCoreUpdateBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |          **10** |     **2.610 ms** |  **0.0462 ms** |  **0.0732 ms** |     **2.614 ms** |  **1.00** |    **0.00** |    **35.1563** |         **-** |     **-** |       **109 KB** |
| VenflowUpdateBatchAsync |      .NET 4.8 |      .NET 4.8 |          10 |     1.878 ms |  0.0208 ms |  0.0163 ms |     1.878 ms |  0.73 |    0.02 |     7.8125 |         - |     - |     26.75 KB |
|  RepoDbUpdateBatchAsync |      .NET 4.8 |      .NET 4.8 |          10 |     1.438 ms |  0.0275 ms |  0.0244 ms |     1.431 ms |  0.56 |    0.02 |     5.8594 |         - |     - |     22.97 KB |
|                         |               |               |             |              |            |            |              |       |         |            |           |       |              |
|  EFCoreUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     2.459 ms |  0.0471 ms |  0.0690 ms |     2.459 ms |  1.00 |    0.00 |    27.3438 |         - |     - |     94.41 KB |
| VenflowUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     1.847 ms |  0.0368 ms |  0.0327 ms |     1.838 ms |  0.76 |    0.03 |     5.8594 |         - |     - |     21.85 KB |
|  RepoDbUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     1.491 ms |  0.0285 ms |  0.0238 ms |     1.491 ms |  0.61 |    0.02 |     5.8594 |         - |     - |     18.06 KB |
|                         |               |               |             |              |            |            |              |       |         |            |           |       |              |
|  EFCoreUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     2.444 ms |  0.0472 ms |  0.0614 ms |     2.447 ms |  1.00 |    0.00 |    27.3438 |         - |     - |     93.12 KB |
| VenflowUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     1.842 ms |  0.0326 ms |  0.0305 ms |     1.846 ms |  0.76 |    0.02 |     5.8594 |         - |     - |     21.81 KB |
|  RepoDbUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     1.463 ms |  0.0290 ms |  0.0485 ms |     1.465 ms |  0.60 |    0.03 |     5.8594 |         - |     - |     18.01 KB |
|                         |               |               |             |              |            |            |              |       |         |            |           |       |              |
|  **EFCoreUpdateBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |         **100** |    **12.585 ms** |  **0.0785 ms** |  **0.0696 ms** |    **12.597 ms** |  **1.00** |    **0.00** |   **281.2500** |   **78.1250** |     **-** |   **1100.93 KB** |
| VenflowUpdateBatchAsync |      .NET 4.8 |      .NET 4.8 |         100 |     8.860 ms |  0.0868 ms |  0.0812 ms |     8.849 ms |  0.70 |    0.01 |    62.5000 |   15.6250 |     - |    224.21 KB |
|  RepoDbUpdateBatchAsync |      .NET 4.8 |      .NET 4.8 |         100 |     4.570 ms |  0.0669 ms |  0.0522 ms |     4.585 ms |  0.36 |    0.00 |    54.6875 |         - |     - |    186.44 KB |
|                         |               |               |             |              |            |            |              |       |         |            |           |       |              |
|  EFCoreUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |    11.817 ms |  0.0954 ms |  0.0796 ms |    11.811 ms |  1.00 |    0.00 |   296.8750 |   93.7500 |     - |   1102.33 KB |
| VenflowUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |     8.721 ms |  0.0742 ms |  0.0658 ms |     8.741 ms |  0.74 |    0.01 |    46.8750 |   15.6250 |     - |    207.11 KB |
|  RepoDbUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |     4.431 ms |  0.0885 ms |  0.2377 ms |     4.443 ms |  0.39 |    0.01 |    46.8750 |         - |     - |    147.57 KB |
|                         |               |               |             |              |            |            |              |       |         |            |           |       |              |
|  EFCoreUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |    10.967 ms |  0.0542 ms |  0.0480 ms |    10.974 ms |  1.00 |    0.00 |   218.7500 |   62.5000 |     - |    853.82 KB |
| VenflowUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     8.639 ms |  0.1307 ms |  0.1223 ms |     8.631 ms |  0.79 |    0.01 |    46.8750 |         - |     - |    206.88 KB |
|  RepoDbUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     4.356 ms |  0.0866 ms |  0.2429 ms |     4.249 ms |  0.40 |    0.02 |    46.8750 |         - |     - |    147.45 KB |
|                         |               |               |             |              |            |            |              |       |         |            |           |       |              |
|  **EFCoreUpdateBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |        **1000** |   **114.758 ms** |  **1.4628 ms** |  **1.3683 ms** |   **114.263 ms** |  **1.00** |    **0.00** |  **1800.0000** |  **800.0000** |     **-** |  **11345.33 KB** |
| VenflowUpdateBatchAsync |      .NET 4.8 |      .NET 4.8 |        1000 |    82.627 ms |  2.9591 ms |  7.9495 ms |    79.583 ms |  0.84 |    0.09 |   285.7143 |  142.8571 |     - |   2168.74 KB |
|  RepoDbUpdateBatchAsync |      .NET 4.8 |      .NET 4.8 |        1000 |    36.428 ms |  0.6322 ms |  0.5605 ms |    36.313 ms |  0.32 |    0.01 |   571.4286 |         - |     - |   1814.34 KB |
|                         |               |               |             |              |            |            |              |       |         |            |           |       |              |
|  EFCoreUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |   102.817 ms |  0.9094 ms |  0.8062 ms |   102.713 ms |  1.00 |    0.00 |  1600.0000 |  800.0000 |     - |  10407.65 KB |
| VenflowUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |    77.026 ms |  0.7049 ms |  0.6249 ms |    76.943 ms |  0.75 |    0.01 |   285.7143 |  142.8571 |     - |   2006.44 KB |
|  RepoDbUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |    34.606 ms |  0.6903 ms |  1.8897 ms |    34.050 ms |  0.35 |    0.02 |   466.6667 |         - |     - |   1436.75 KB |
|                         |               |               |             |              |            |            |              |       |         |            |           |       |              |
|  EFCoreUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    98.725 ms |  1.4590 ms |  1.3647 ms |    98.577 ms |  1.00 |    0.00 |  1200.0000 |  600.0000 |     - |   8417.88 KB |
| VenflowUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    76.504 ms |  0.8722 ms |  0.7732 ms |    76.382 ms |  0.77 |    0.02 |   285.7143 |  142.8571 |     - |    2004.6 KB |
|  RepoDbUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    33.274 ms |  0.5355 ms |  0.9093 ms |    33.128 ms |  0.34 |    0.01 |   466.6667 |         - |     - |   1428.18 KB |
|                         |               |               |             |              |            |            |              |       |         |            |           |       |              |
|  **EFCoreUpdateBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |       **10000** | **1,235.825 ms** | **13.3645 ms** | **11.8473 ms** | **1,233.899 ms** |  **1.00** |    **0.00** | **27000.0000** | **4000.0000** |     **-** |  **123778.2 KB** |
| VenflowUpdateBatchAsync |      .NET 4.8 |      .NET 4.8 |       10000 |   811.143 ms |  7.0412 ms |  6.5864 ms |   811.312 ms |  0.66 |    0.01 |  3000.0000 | 1000.0000 |     - |  21912.16 KB |
|  RepoDbUpdateBatchAsync |      .NET 4.8 |      .NET 4.8 |       10000 |   532.471 ms |  9.7711 ms | 13.6978 ms |   532.778 ms |  0.44 |    0.01 |  4000.0000 | 1000.0000 |     - |  18072.41 KB |
|                         |               |               |             |              |            |            |              |       |         |            |           |       |              |
|  EFCoreUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 | 1,109.041 ms | 18.5031 ms | 16.4025 ms | 1,110.398 ms |  1.00 |    0.00 | 25000.0000 | 4000.0000 |     - | 116223.34 KB |
| VenflowUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |   798.729 ms | 10.6728 ms |  9.9833 ms |   796.406 ms |  0.72 |    0.02 |  3000.0000 | 1000.0000 |     - |  19931.84 KB |
|  RepoDbUpdateBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |   540.983 ms | 10.6100 ms | 14.1641 ms |   543.654 ms |  0.49 |    0.02 |  3000.0000 | 1000.0000 |     - |  14245.42 KB |
|                         |               |               |             |              |            |            |              |       |         |            |           |       |              |
|  EFCoreUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 1,045.694 ms | 17.9664 ms | 16.8057 ms | 1,044.413 ms |  1.00 |    0.00 | 13000.0000 | 4000.0000 |     - |   84266.1 KB |
| VenflowUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   805.573 ms |  7.1757 ms |  5.9920 ms |   807.240 ms |  0.77 |    0.02 |  2000.0000 | 1000.0000 |     - |  19933.23 KB |
|  RepoDbUpdateBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   521.180 ms | 12.2137 ms | 36.0125 ms |   518.612 ms |  0.52 |    0.02 |  3000.0000 | 1000.0000 |     - |  14317.42 KB |
