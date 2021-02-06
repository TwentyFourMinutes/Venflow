``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                  Method |           Job |       Runtime | DeleteCount |       Mean |      Error |      StdDev |     Median | Ratio | RatioSD |     Gen 0 |     Gen 1 |     Gen 2 |    Allocated |
|------------------------ |-------------- |-------------- |------------ |-----------:|-----------:|------------:|-----------:|------:|--------:|----------:|----------:|----------:|-------------:|
|  **EFCoreDeleteBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |          **10** |   **2.777 ms** |  **0.0546 ms** |   **0.0766 ms** |   **2.778 ms** |  **1.00** |    **0.00** |    **3.9063** |         **-** |         **-** |    **108.16 KB** |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |   1.677 ms |  0.0315 ms |   0.0295 ms |   1.675 ms |  0.61 |    0.02 |         - |         - |         - |      23.7 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |          10 |   2.033 ms |  0.0580 ms |   0.1709 ms |   2.075 ms |  0.66 |    0.03 |         - |         - |         - |     43.63 KB |
|                         |               |               |             |            |            |             |            |       |         |           |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |   2.716 ms |  0.0536 ms |   0.0895 ms |   2.712 ms |  1.00 |    0.00 |    3.9063 |         - |         - |     92.16 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |   1.648 ms |  0.0313 ms |   0.0373 ms |   1.646 ms |  0.61 |    0.02 |         - |         - |         - |     23.66 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |          10 |   1.984 ms |  0.0834 ms |   0.2459 ms |   1.997 ms |  0.71 |    0.05 |         - |         - |         - |     33.86 KB |
|                         |               |               |             |            |            |             |            |       |         |           |           |           |              |
|  **EFCoreDeleteBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |         **100** |   **9.731 ms** |  **0.1258 ms** |   **0.1116 ms** |   **9.732 ms** |  **1.00** |    **0.00** |   **46.8750** |   **15.6250** |         **-** |   **1009.09 KB** |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |   3.161 ms |  0.0591 ms |   0.0493 ms |   3.150 ms |  0.32 |    0.01 |    7.8125 |         - |         - |    165.75 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |         100 |   3.743 ms |  0.0659 ms |   0.0834 ms |   3.749 ms |  0.39 |    0.01 |   15.6250 |         - |         - |    306.91 KB |
|                         |               |               |             |            |            |             |            |       |         |           |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   8.769 ms |  0.1752 ms |   0.1947 ms |   8.751 ms |  1.00 |    0.00 |   31.2500 |         - |         - |    810.03 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   3.933 ms |  0.4974 ms |   1.3866 ms |   3.239 ms |  0.63 |    0.12 |    7.8125 |         - |         - |     165.6 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |         100 |   3.689 ms |  0.0734 ms |   0.0874 ms |   3.686 ms |  0.42 |    0.01 |    7.8125 |         - |         - |     214.3 KB |
|                         |               |               |             |            |            |             |            |       |         |           |           |           |              |
|  **EFCoreDeleteBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |        **1000** |  **82.886 ms** |  **1.4204 ms** |   **1.3951 ms** |  **82.776 ms** |  **1.00** |    **0.00** |  **571.4286** |  **285.7143** |         **-** |  **11587.42 KB** |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |  14.019 ms |  0.2733 ms |   0.2556 ms |  14.057 ms |  0.17 |    0.00 |   78.1250 |   31.2500 |         - |   1579.93 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |        1000 |  31.000 ms |  0.5961 ms |   0.7539 ms |  30.784 ms |  0.37 |    0.01 |  250.0000 |  125.0000 |         - |   4608.31 KB |
|                         |               |               |             |            |            |             |            |       |         |           |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |  67.184 ms |  1.3396 ms |   1.3157 ms |  67.192 ms |  1.00 |    0.00 |  375.0000 |  125.0000 |         - |   7888.43 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |  13.893 ms |  0.2736 ms |   0.3151 ms |  13.880 ms |  0.21 |    0.01 |   78.1250 |   31.2500 |         - |   1579.89 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |        1000 |  34.828 ms |  1.5212 ms |   4.4614 ms |  33.976 ms |  0.51 |    0.05 |  156.2500 |   62.5000 |         - |   3375.61 KB |
|                         |               |               |             |            |            |             |            |       |         |           |           |           |              |
|  **EFCoreDeleteBatchAsync** | **.NET Core 3.1** | **.NET Core 3.1** |       **10000** | **939.510 ms** | **18.3120 ms** |  **17.9848 ms** | **937.216 ms** |  **1.00** |    **0.00** | **8000.0000** | **3000.0000** | **1000.0000** | **136590.79 KB** |
| VenflowDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 | 133.312 ms |  2.6046 ms |   4.1312 ms | 132.726 ms |  0.14 |    0.01 | 1000.0000 |  666.6667 |  666.6667 |  15731.01 KB |
|  RepoDbDeleteBatchAsync | .NET Core 3.1 | .NET Core 3.1 |       10000 | 453.422 ms |  8.9625 ms |  17.2677 ms | 452.908 ms |  0.48 |    0.02 | 7000.0000 | 3000.0000 | 1000.0000 | 121306.99 KB |
|                         |               |               |             |            |            |             |            |       |         |           |           |           |              |
|  EFCoreDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 689.143 ms | 13.3833 ms |  18.3192 ms | 685.826 ms |  1.00 |    0.00 | 4000.0000 | 1000.0000 |         - |   78776.3 KB |
| VenflowDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 234.411 ms | 42.4870 ms | 125.2740 ms | 135.855 ms |  0.30 |    0.17 | 1250.0000 | 1000.0000 |  750.0000 |  15735.24 KB |
|  RepoDbDeleteBatchAsync | .NET Core 5.0 | .NET Core 5.0 |       10000 | 431.172 ms |  8.5554 ms |  11.1244 ms | 431.066 ms |  0.63 |    0.02 | 6000.0000 | 2000.0000 | 1000.0000 | 112246.59 KB |
