``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                  Method |           Job |       Runtime | DeleteCount |         Mean |      Error |     StdDev |       Median | Ratio | RatioSD |     Gen 0 |     Gen 1 |     Gen 2 |    Allocated |
|------------------------ |-------------- |-------------- |------------ |-------------:|-----------:|-----------:|-------------:|------:|--------:|----------:|----------:|----------:|-------------:|
|  **EFCoreDeleteBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |          **10** |     **2.427 ms** |  **0.0482 ms** |  **0.0644 ms** |     **2.410 ms** |  **1.00** |    **0.00** |    **3.9063** |         **-** |         **-** |    **107.99 KB** |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     1.333 ms |  0.0222 ms |  0.0256 ms |     1.330 ms |  0.55 |    0.02 |         - |         - |         - |     23.64 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |     1.950 ms |  0.1108 ms |  0.3266 ms |     2.002 ms |  0.64 |    0.04 |    1.9531 |         - |         - |     43.77 KB |
|                         |               |               |             |              |            |            |              |       |         |           |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     2.295 ms |  0.0308 ms |  0.0288 ms |     2.293 ms |  1.00 |    0.00 |    3.9063 |         - |         - |     91.83 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     1.336 ms |  0.0253 ms |  0.0237 ms |     1.331 ms |  0.58 |    0.01 |         - |         - |         - |      23.5 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |     1.425 ms |  0.0277 ms |  0.0285 ms |     1.418 ms |  0.62 |    0.02 |         - |         - |         - |     33.84 KB |
|                         |               |               |             |              |            |            |              |       |         |           |           |           |              |
|  **EFCoreDeleteBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |         **100** |     **9.685 ms** |  **0.1931 ms** |  **0.2066 ms** |     **9.671 ms** |  **1.00** |    **0.00** |   **46.8750** |   **15.6250** |         **-** |   **1009.16 KB** |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |     3.066 ms |  0.0549 ms |  0.0654 ms |     3.054 ms |  0.32 |    0.01 |    7.8125 |         - |         - |    165.16 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |     6.067 ms |  0.8154 ms |  2.4042 ms |     6.016 ms |  0.46 |    0.11 |   15.6250 |    3.9063 |         - |    306.83 KB |
|                         |               |               |             |              |            |            |              |       |         |           |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     8.555 ms |  0.0865 ms |  0.0809 ms |     8.550 ms |  1.00 |    0.00 |   31.2500 |         - |         - |    810.01 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     3.917 ms |  0.5365 ms |  1.5045 ms |     3.144 ms |  0.46 |    0.11 |    7.8125 |         - |         - |    165.17 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |     3.642 ms |  0.2132 ms |  0.5763 ms |     3.394 ms |  0.53 |    0.10 |   11.7188 |    3.9063 |         - |    214.37 KB |
|                         |               |               |             |              |            |            |              |       |         |           |           |           |              |
|  **EFCoreDeleteBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |        **1000** |    **89.440 ms** |  **1.7805 ms** |  **1.5784 ms** |    **89.064 ms** |  **1.00** |    **0.00** |  **500.0000** |  **166.6667** |         **-** |  **11587.05 KB** |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |    14.546 ms |  0.2886 ms |  0.3951 ms |    14.592 ms |  0.16 |    0.01 |   62.5000 |   31.2500 |         - |   1596.45 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |    31.729 ms |  0.5351 ms |  0.5006 ms |    31.765 ms |  0.35 |    0.01 |  250.0000 |  125.0000 |         - |   4596.61 KB |
|                         |               |               |             |              |            |            |              |       |         |           |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    70.693 ms |  1.4090 ms |  2.1936 ms |    70.297 ms |  1.00 |    0.00 |  428.5714 |  142.8571 |         - |   7888.57 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    14.435 ms |  0.2766 ms |  0.3293 ms |    14.505 ms |  0.20 |    0.01 |   78.1250 |   31.2500 |         - |   1596.53 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |    34.529 ms |  0.9065 ms |  2.6443 ms |    34.298 ms |  0.50 |    0.03 |  125.0000 |   62.5000 |         - |   3408.65 KB |
|                         |               |               |             |              |            |            |              |       |         |           |           |           |              |
|  **EFCoreDeleteBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |       **10000** | **1,018.776 ms** | **20.0870 ms** | **25.4036 ms** | **1,013.508 ms** |  **1.00** |    **0.00** | **8000.0000** | **3000.0000** | **1000.0000** |  **136594.7 KB** |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |   142.719 ms |  2.7206 ms |  2.5448 ms |   143.019 ms |  0.14 |    0.00 | 1000.0000 |  666.6667 |  666.6667 |  15610.07 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 |   451.766 ms |  6.0207 ms |  5.3372 ms |   451.572 ms |  0.44 |    0.01 | 7000.0000 | 3000.0000 | 1000.0000 | 121314.77 KB |
|                         |               |               |             |              |            |            |              |       |         |           |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   727.807 ms |  8.6514 ms |  7.6692 ms |   726.814 ms |  1.00 |    0.00 | 4000.0000 | 1000.0000 |         - |  78780.92 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   142.351 ms |  2.8291 ms |  3.3679 ms |   141.887 ms |  0.20 |    0.00 | 1250.0000 | 1000.0000 |  750.0000 |  15609.04 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 |   441.842 ms |  8.5707 ms |  8.0171 ms |   441.242 ms |  0.61 |    0.01 | 6000.0000 | 2000.0000 | 1000.0000 | 112241.98 KB |
