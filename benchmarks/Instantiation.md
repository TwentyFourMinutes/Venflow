``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.7.21379.14
  [Host]   : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                     Method |     Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------------- |---------:|---------:|---------:|-------:|------:|------:|----------:|
|   InstantiateEFCoreContext | 73.08 μs | 1.434 μs | 2.057 μs | 1.7090 |     - |     - |     46 KB |
| InstantiateVenflowDatabase | 63.32 μs | 1.226 μs | 1.718 μs | 1.3428 |     - |     - |     37 KB |
