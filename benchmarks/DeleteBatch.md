``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.508 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-rc.1.20452.10
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4220.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT


```
|                  Method |           Job |       Runtime | DeleteCount |         Mean |      Error |      StdDev |       Median | Ratio | RatioSD |      Gen 0 |     Gen 1 |     Gen 2 |    Allocated |
|------------------------ |-------------- |-------------- |------------ |-------------:|-----------:|------------:|-------------:|------:|--------:|-----------:|----------:|----------:|-------------:|
|  **EFCoreDeleteBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |          **10** |     **4.095 ms** |  **0.0810 ms** |   **0.1261 ms** |     **4.081 ms** |  **1.00** |    **0.00** |    **39.0625** |         **-** |         **-** |    **125.25 KB** |
| VenflowDeleteBatchAsync |      .NET 4.8 |      .NET 4.8 |          10 |     2.650 ms |  0.0734 ms |   0.2009 ms |     2.626 ms |  0.64 |    0.04 |     7.8125 |         - |         - |     27.16 KB |
|  RepoDbDeleteBatchAsync |      .NET 4.8 |      .NET 4.8 |          10 |     3.317 ms |  0.0839 ms |   0.2353 ms |     3.306 ms |  0.78 |    0.07 |    15.6250 |         - |         - |      53.3 KB |
|                         |               |               |             |              |            |             |              |       |         |            |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     3.958 ms |  0.0780 ms |   0.1118 ms |     3.944 ms |  1.00 |    0.00 |    31.2500 |         - |         - |       106 KB |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     3.377 ms |  0.1372 ms |   0.4044 ms |     3.415 ms |  0.76 |    0.09 |     3.9063 |         - |         - |      18.2 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     3.350 ms |  0.0615 ms |   0.1271 ms |     3.322 ms |  0.84 |    0.04 |    11.7188 |         - |         - |     39.11 KB |
|                         |               |               |             |              |            |             |              |       |         |            |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     4.435 ms |  0.0871 ms |   0.1004 ms |     4.418 ms |  1.00 |    0.00 |    23.4375 |         - |         - |     90.46 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     2.850 ms |  0.0554 ms |   0.0616 ms |     2.840 ms |  0.64 |    0.02 |     3.9063 |         - |         - |     18.19 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     3.255 ms |  0.0621 ms |   0.0762 ms |     3.254 ms |  0.74 |    0.02 |     7.8125 |         - |         - |     29.38 KB |
|                         |               |               |             |              |            |             |              |       |         |            |           |           |              |
|  **EFCoreDeleteBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |         **100** |    **17.844 ms** |  **0.4847 ms** |   **1.4216 ms** |    **18.344 ms** |  **1.00** |    **0.00** |   **281.2500** |   **62.5000** |         **-** |   **1103.03 KB** |
| VenflowDeleteBatchAsync |      .NET 4.8 |      .NET 4.8 |         100 |     5.060 ms |  0.0752 ms |   0.0628 ms |     5.058 ms |  0.32 |    0.01 |    46.8750 |         - |         - |    163.75 KB |
|  RepoDbDeleteBatchAsync |      .NET 4.8 |      .NET 4.8 |         100 |     6.093 ms |  0.1174 ms |   0.0981 ms |     6.079 ms |  0.39 |    0.01 |   109.3750 |         - |         - |     346.4 KB |
|                         |               |               |             |              |            |             |              |       |         |            |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |    15.661 ms |  0.3128 ms |   0.6247 ms |    15.745 ms |  1.00 |    0.00 |   312.5000 |   93.7500 |         - |   1007.21 KB |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |     4.986 ms |  0.0756 ms |   0.0631 ms |     4.979 ms |  0.34 |    0.01 |    39.0625 |    7.8125 |         - |    139.99 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |     8.456 ms |  0.5894 ms |   1.7379 ms |     8.579 ms |  0.46 |    0.08 |    93.7500 |         - |         - |    286.15 KB |
|                         |               |               |             |              |            |             |              |       |         |            |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |    14.100 ms |  0.2768 ms |   0.5061 ms |    14.097 ms |  1.00 |    0.00 |   234.3750 |   62.5000 |         - |    808.16 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     4.211 ms |  0.0826 ms |   0.0645 ms |     4.203 ms |  0.31 |    0.01 |    39.0625 |    7.8125 |         - |    139.98 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     5.038 ms |  0.1001 ms |   0.2741 ms |     4.930 ms |  0.36 |    0.02 |    62.5000 |         - |         - |    190.91 KB |
|                         |               |               |             |              |            |             |              |       |         |            |           |           |              |
|  **EFCoreDeleteBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |        **1000** |   **135.235 ms** |  **1.6257 ms** |   **1.4411 ms** |   **135.086 ms** |  **1.00** |    **0.00** |  **2000.0000** |  **750.0000** |         **-** |  **12398.58 KB** |
| VenflowDeleteBatchAsync |      .NET 4.8 |      .NET 4.8 |        1000 |    33.947 ms |  1.5334 ms |   4.5214 ms |    34.787 ms |  0.21 |    0.01 |   250.0000 |   93.7500 |         - |   1532.17 KB |
|  RepoDbDeleteBatchAsync |      .NET 4.8 |      .NET 4.8 |        1000 |    49.287 ms |  0.9847 ms |   1.2093 ms |    48.964 ms |  0.37 |    0.01 |  1400.0000 |  500.0000 |         - |   7917.72 KB |
|                         |               |               |             |              |            |             |              |       |         |            |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |   125.757 ms |  2.4569 ms |   2.1779 ms |   125.818 ms |  1.00 |    0.00 |  2000.0000 |  750.0000 |         - |  11603.56 KB |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |    26.288 ms |  0.4915 ms |   0.4597 ms |    26.192 ms |  0.21 |    0.00 |   218.7500 |   93.7500 |         - |   1354.57 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |    48.449 ms |  0.9648 ms |   2.2361 ms |    48.501 ms |  0.36 |    0.02 |  1181.8182 |  454.5455 |         - |    7356.7 KB |
|                         |               |               |             |              |            |             |              |       |         |            |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |   111.791 ms |  1.9289 ms |   2.6403 ms |   111.689 ms |  1.00 |    0.00 |  1400.0000 |  400.0000 |         - |    7905.4 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    26.348 ms |  0.4994 ms |   0.5550 ms |    26.322 ms |  0.24 |    0.01 |   312.5000 |  156.2500 |         - |   1354.56 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    43.423 ms |  0.8097 ms |   1.1351 ms |    43.181 ms |  0.39 |    0.01 |  1166.6667 |  250.0000 |         - |   6449.98 KB |
|                         |               |               |             |              |            |             |              |       |         |            |           |           |              |
|  **EFCoreDeleteBatchAsync** |      **.NET 4.8** |      **.NET 4.8** |       **10000** | **1,473.648 ms** | **28.2366 ms** |  **27.7321 ms** | **1,471.631 ms** |  **1.00** |    **0.00** | **34000.0000** | **6000.0000** | **2000.0000** | **145195.17 KB** |
| VenflowDeleteBatchAsync |      .NET 4.8 |      .NET 4.8 |       10000 |   284.400 ms |  5.6391 ms |  10.4525 ms |   283.659 ms |  0.19 |    0.01 |  3000.0000 | 1500.0000 |  500.0000 |  15835.51 KB |
|  RepoDbDeleteBatchAsync |      .NET 4.8 |      .NET 4.8 |       10000 |   580.573 ms | 10.1694 ms |   9.5125 ms |   578.139 ms |  0.39 |    0.01 | 38000.0000 | 9000.0000 | 3000.0000 | 125804.42 KB |
|                         |               |               |             |              |            |             |              |       |         |            |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 | 1,319.776 ms | 25.3744 ms |  23.7352 ms | 1,319.672 ms |  1.00 |    0.00 | 31000.0000 | 6000.0000 | 1000.0000 |  136864.2 KB |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |   269.099 ms |  5.3008 ms |   8.0948 ms |   269.031 ms |  0.20 |    0.01 |  2000.0000 | 1000.0000 |         - |  13710.92 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |   584.135 ms | 11.1703 ms |  13.2975 ms |   585.912 ms |  0.44 |    0.01 | 32000.0000 | 8000.0000 | 3000.0000 | 118973.48 KB |
|                         |               |               |             |              |            |             |              |       |         |            |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 1,139.150 ms | 22.3978 ms |  32.8304 ms | 1,130.112 ms |  1.00 |    0.00 | 15000.0000 | 6000.0000 | 2000.0000 |   79057.7 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   405.768 ms | 44.8528 ms | 132.2496 ms |   507.431 ms |  0.22 |    0.01 |  2000.0000 |  666.6667 |         - |  13710.82 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   507.479 ms | 10.0818 ms |  12.0017 ms |   507.868 ms |  0.45 |    0.01 | 33000.0000 | 9000.0000 | 3000.0000 | 109897.01 KB |
