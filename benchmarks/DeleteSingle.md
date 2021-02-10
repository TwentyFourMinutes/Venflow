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
|  EFCoreDeleteSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1,389.4 μs | 26.64 μs | 24.92 μs |  1.00 |    0.00 |     - |     - |     - |  20.81 KB |
| VenflowDeleteSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1,153.9 μs | 20.40 μs | 27.23 μs |  0.84 |    0.03 |     - |     - |     - |  10.33 KB |
|  RepoDbDeleteSingleAsync | .NET Core 3.1 | .NET Core 3.1 |   986.7 μs | 14.95 μs | 13.99 μs |  0.71 |    0.01 |     - |     - |     - |  12.52 KB |
|                          |               |               |            |          |          |       |         |       |       |       |           |
|  EFCoreDeleteSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1,424.3 μs | 28.02 μs | 23.40 μs |  1.00 |    0.00 |     - |     - |     - |  21.49 KB |
| VenflowDeleteSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1,178.0 μs | 20.28 μs | 24.15 μs |  0.83 |    0.02 |     - |     - |     - |  10.37 KB |
|  RepoDbDeleteSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1,053.6 μs | 19.36 μs | 25.18 μs |  0.75 |    0.02 |     - |     - |     - |  12.14 KB |
