``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                   Method |           Job |       Runtime |     Mean |    Error |   StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-------------- |-------------- |---------:|---------:|---------:|------:|--------:|------:|------:|------:|----------:|
|  EFCoreUpdateSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 914.2 μs | 18.11 μs | 18.60 μs |  1.00 |    0.00 |     - |     - |     - |  16.17 KB |
| VenflowUpdateSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 556.8 μs | 10.99 μs | 21.18 μs |  0.61 |    0.03 |     - |     - |     - |   4.07 KB |
|  RepoDbUpdateSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 569.3 μs | 11.11 μs | 16.63 μs |  0.62 |    0.03 |     - |     - |     - |   7.01 KB |
|                          |               |               |          |          |          |       |         |       |       |       |           |
|  EFCoreUpdateSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 939.4 μs | 18.68 μs | 34.16 μs |  1.00 |    0.00 |     - |     - |     - |  15.35 KB |
| VenflowUpdateSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 571.2 μs | 11.17 μs | 19.86 μs |  0.61 |    0.03 |     - |     - |     - |   4.06 KB |
|  RepoDbUpdateSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 588.5 μs | 11.69 μs | 24.92 μs |  0.63 |    0.04 |     - |     - |     - |   6.48 KB |
