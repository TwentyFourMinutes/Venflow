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
|  EFCoreInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 945.7 μs | 18.76 μs | 40.78 μs |  1.00 |    0.00 |     - |     - |     - |  27.68 KB |
| VenflowInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 730.9 μs | 14.15 μs | 20.74 μs |  0.78 |    0.05 |     - |     - |     - |   5.06 KB |
|  RepoDbInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 543.5 μs | 10.77 μs | 19.42 μs |  0.58 |    0.04 |     - |     - |     - |   4.32 KB |
|                          |               |               |          |          |          |       |         |       |       |       |           |
|  EFCoreInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 987.6 μs | 18.27 μs | 35.20 μs |  1.00 |    0.00 |     - |     - |     - |  18.22 KB |
| VenflowInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 720.9 μs | 13.98 μs | 13.07 μs |  0.72 |    0.02 |     - |     - |     - |   5.04 KB |
|  RepoDbInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 555.3 μs | 11.08 μs | 28.59 μs |  0.57 |    0.04 |     - |     - |     - |    4.3 KB |
