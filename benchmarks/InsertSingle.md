``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                   Method |           Job |       Runtime |       Mean |    Error |   StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-------------- |-------------- |-----------:|---------:|---------:|------:|--------:|------:|------:|------:|----------:|
|  EFCoreInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1,036.6 μs | 18.09 μs | 24.15 μs |  1.00 |    0.00 |     - |     - |     - |  19.61 KB |
| VenflowInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 |   716.4 μs | 11.75 μs |  9.81 μs |  0.69 |    0.02 |     - |     - |     - |   5.09 KB |
|  RepoDbInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 |   564.0 μs | 11.22 μs | 24.85 μs |  0.54 |    0.02 |     - |     - |     - |   4.32 KB |
|                          |               |               |            |          |          |       |         |       |       |       |           |
|  EFCoreInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1,079.4 μs | 21.28 μs | 49.32 μs |  1.00 |    0.00 |     - |     - |     - |  18.22 KB |
| VenflowInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 |   748.1 μs | 14.73 μs | 23.36 μs |  0.70 |    0.04 |     - |     - |     - |   5.04 KB |
|  RepoDbInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 |   590.6 μs | 12.06 μs | 35.37 μs |  0.55 |    0.04 |     - |     - |     - |    4.3 KB |
