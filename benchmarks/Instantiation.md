``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                     Method |     Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------------- |---------:|---------:|---------:|-------:|------:|------:|----------:|
|   InstantiateEFCoreContext | 87.16 μs | 1.651 μs | 1.621 μs | 1.7090 |     - |     - |     46 KB |
| InstantiateVenflowDatabase | 69.47 μs | 1.309 μs | 1.607 μs | 1.3428 |     - |     - |     37 KB |
