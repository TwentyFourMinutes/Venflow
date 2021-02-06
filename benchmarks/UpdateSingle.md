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
|  EFCoreUpdateSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1,039.0 μs | 20.74 μs | 36.86 μs |  1.00 |    0.00 |     - |     - |     - |  16.18 KB |
| VenflowUpdateSingleAsync | .NET Core 3.1 | .NET Core 3.1 |   752.3 μs | 14.40 μs | 16.01 μs |  0.72 |    0.03 |     - |     - |     - |    5.3 KB |
|  RepoDbUpdateSingleAsync | .NET Core 3.1 | .NET Core 3.1 |   571.0 μs | 11.39 μs | 22.47 μs |  0.55 |    0.03 |     - |     - |     - |   7.02 KB |
|                          |               |               |            |          |          |       |         |       |       |       |           |
|  EFCoreUpdateSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1,019.7 μs | 20.34 μs | 38.70 μs |  1.00 |    0.00 |     - |     - |     - |  15.35 KB |
| VenflowUpdateSingleAsync | .NET Core 5.0 | .NET Core 5.0 |   766.4 μs | 11.36 μs | 10.62 μs |  0.75 |    0.02 |     - |     - |     - |   5.27 KB |
|  RepoDbUpdateSingleAsync | .NET Core 5.0 | .NET Core 5.0 |   602.2 μs | 11.96 μs | 30.45 μs |  0.59 |    0.04 |     - |     - |     - |   6.49 KB |
