``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                     Method |           Job |       Runtime |     Mean |    Error |   StdDev |   Median |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------------- |-------------- |-------------- |---------:|---------:|---------:|---------:|-------:|------:|------:|----------:|
|   InstantiateEFCoreContext | .NET Core 3.1 | .NET Core 3.1 | 76.11 μs | 1.345 μs | 2.134 μs | 76.03 μs | 1.4648 |     - |     - |  39.12 KB |
| InstantiateVenflowDatabase | .NET Core 3.1 | .NET Core 3.1 | 75.67 μs | 1.811 μs | 5.254 μs | 73.85 μs | 1.3428 |     - |     - |   37.1 KB |
|   InstantiateEFCoreContext | .NET Core 5.0 | .NET Core 5.0 | 74.79 μs | 1.439 μs | 2.109 μs | 74.50 μs | 1.7090 |     - |     - |   45.2 KB |
| InstantiateVenflowDatabase | .NET Core 5.0 | .NET Core 5.0 | 62.89 μs | 1.221 μs | 1.864 μs | 62.89 μs | 1.3428 |     - |     - |  37.13 KB |
