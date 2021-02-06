``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                   Method |           Job |       Runtime |     Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-------------- |-------------- |---------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|  EFCoreDeleteSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.773 ms | 0.0353 ms | 0.0591 ms |  1.00 |    0.00 |     - |     - |     - |  20.94 KB |
| VenflowDeleteSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.458 ms | 0.0273 ms | 0.0255 ms |  0.82 |    0.03 |     - |     - |     - |  10.43 KB |
|  RepoDbDeleteSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.360 ms | 0.0266 ms | 0.0382 ms |  0.77 |    0.03 |     - |     - |     - |  12.53 KB |
|                          |               |               |          |           |           |       |         |       |       |       |           |
|  EFCoreDeleteSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.877 ms | 0.0370 ms | 0.0871 ms |  1.00 |    0.00 |     - |     - |     - |  21.52 KB |
| VenflowDeleteSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.460 ms | 0.0289 ms | 0.0355 ms |  0.78 |    0.05 |     - |     - |     - |  10.37 KB |
|  RepoDbDeleteSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.358 ms | 0.0264 ms | 0.0386 ms |  0.72 |    0.03 |     - |     - |     - |  12.13 KB |
