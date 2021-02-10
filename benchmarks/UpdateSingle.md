``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                   Method |           Job |       Runtime |     Mean |    Error |   StdDev |   Median | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-------------- |-------------- |---------:|---------:|---------:|---------:|------:|--------:|------:|------:|------:|----------:|
|  EFCoreUpdateSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 792.2 μs | 14.44 μs | 13.50 μs | 788.9 μs |  1.00 |    0.00 |     - |     - |     - |  16.19 KB |
| VenflowUpdateSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 611.5 μs | 11.98 μs | 18.65 μs | 608.3 μs |  0.78 |    0.03 |     - |     - |     - |   5.32 KB |
|  RepoDbUpdateSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 436.9 μs |  8.62 μs | 17.61 μs | 431.0 μs |  0.58 |    0.02 |     - |     - |     - |   7.02 KB |
|                          |               |               |          |          |          |          |       |         |       |       |       |           |
|  EFCoreUpdateSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 804.2 μs | 15.80 μs | 30.07 μs | 790.2 μs |  1.00 |    0.00 |     - |     - |     - |  15.35 KB |
| VenflowUpdateSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 603.3 μs | 11.40 μs | 12.67 μs | 603.8 μs |  0.74 |    0.03 |     - |     - |     - |   5.25 KB |
|  RepoDbUpdateSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 448.0 μs | 10.22 μs | 30.14 μs | 438.6 μs |  0.56 |    0.05 |     - |     - |     - |    6.5 KB |
