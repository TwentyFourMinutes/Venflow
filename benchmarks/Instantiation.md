``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                     Method |           Job |       Runtime |     Mean |    Error |   StdDev |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|--------------------------- |-------------- |-------------- |---------:|---------:|---------:|-------:|-------:|------:|----------:|
|   InstantiateEFCoreContext | .NET Core 3.1 | .NET Core 3.1 | 83.17 μs | 0.340 μs | 0.284 μs | 2.0752 | 0.1221 |     - |  39.12 KB |
| InstantiateVenflowDatabase | .NET Core 3.1 | .NET Core 3.1 | 77.63 μs | 0.472 μs | 0.442 μs | 1.9531 | 0.1221 |     - |   37.1 KB |
|   InstantiateEFCoreContext | .NET Core 5.0 | .NET Core 5.0 | 80.69 μs | 0.592 μs | 0.525 μs | 2.4414 | 0.1221 |     - |   45.2 KB |
| InstantiateVenflowDatabase | .NET Core 5.0 | .NET Core 5.0 | 67.64 μs | 0.234 μs | 0.182 μs | 1.9531 | 0.1221 |     - |  37.13 KB |
