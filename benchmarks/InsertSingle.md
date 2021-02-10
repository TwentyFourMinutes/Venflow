``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                   Method |           Job |       Runtime |     Mean |    Error |   StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-------------- |-------------- |---------:|---------:|---------:|------:|--------:|------:|------:|------:|----------:|
|  EFCoreInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 804.9 μs | 11.02 μs |  9.77 μs |  1.00 |    0.00 |     - |     - |     - |  19.62 KB |
| VenflowInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 557.4 μs |  8.26 μs |  7.72 μs |  0.69 |    0.01 |     - |     - |     - |   5.09 KB |
|  RepoDbInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 410.1 μs |  8.17 μs | 19.26 μs |  0.53 |    0.02 |     - |     - |     - |   4.32 KB |
|                          |               |               |          |          |          |       |         |       |       |       |           |
|  EFCoreInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 871.5 μs | 17.19 μs | 31.00 μs |  1.00 |    0.00 |     - |     - |     - |  18.22 KB |
| VenflowInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 573.5 μs |  8.53 μs |  7.98 μs |  0.65 |    0.02 |     - |     - |     - |   5.07 KB |
|  RepoDbInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 411.5 μs |  8.20 μs | 23.26 μs |  0.48 |    0.02 |     - |     - |     - |    4.3 KB |
