``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                     Method |           Job |       Runtime |     Mean |    Error |   StdDev |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|--------------------------- |-------------- |-------------- |---------:|---------:|---------:|-------:|-------:|------:|----------:|
|   InstantiateEFCoreContext | .NET Core 3.1 | .NET Core 3.1 | 89.64 μs | 1.750 μs | 2.083 μs | 2.0752 | 0.1221 |     - |  39.12 KB |
| InstantiateVenflowDatabase | .NET Core 3.1 | .NET Core 3.1 | 83.98 μs | 1.656 μs | 1.468 μs | 1.9531 | 0.1221 |     - |   37.1 KB |
|   InstantiateEFCoreContext | .NET Core 5.0 | .NET Core 5.0 | 88.28 μs | 1.526 μs | 1.427 μs | 2.4414 | 0.1221 |     - |   45.2 KB |
| InstantiateVenflowDatabase | .NET Core 5.0 | .NET Core 5.0 | 74.67 μs | 0.783 μs | 0.732 μs | 1.9531 | 0.1221 |     - |  37.13 KB |
