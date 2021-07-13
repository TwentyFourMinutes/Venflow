``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                     Method |     Mean |    Error |   StdDev |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|--------------------------- |---------:|---------:|---------:|-------:|-------:|------:|----------:|
|   InstantiateEFCoreContext | 80.39 μs | 1.542 μs | 1.776 μs | 1.7090 |      - |     - |     46 KB |
| InstantiateVenflowDatabase | 62.31 μs | 0.854 μs | 0.713 μs | 1.4038 | 0.0610 |     - |     37 KB |
