``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                   Method |           Job |       Runtime |     Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-------------- |-------------- |---------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|  EFCoreDeleteSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.637 ms | 0.0211 ms | 0.0176 ms |  1.00 |    0.00 |     - |     - |     - |  20.82 KB |
| VenflowDeleteSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.278 ms | 0.0248 ms | 0.0331 ms |  0.78 |    0.02 |     - |     - |     - |   9.23 KB |
|  RepoDbDeleteSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.304 ms | 0.0225 ms | 0.0423 ms |  0.80 |    0.03 |     - |     - |     - |  12.52 KB |
|                          |               |               |          |           |           |       |         |       |       |       |           |
|  EFCoreDeleteSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.694 ms | 0.0335 ms | 0.0314 ms |  1.00 |    0.00 |     - |     - |     - |  21.52 KB |
| VenflowDeleteSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.265 ms | 0.0249 ms | 0.0387 ms |  0.75 |    0.03 |     - |     - |     - |    9.2 KB |
|  RepoDbDeleteSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.310 ms | 0.0259 ms | 0.0447 ms |  0.78 |    0.02 |     - |     - |     - |  12.13 KB |
