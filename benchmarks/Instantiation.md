``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                     Method |     Mean |    Error |   StdDev |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|--------------------------- |---------:|---------:|---------:|-------:|-------:|------:|----------:|
|   InstantiateEFCoreContext | 66.25 μs | 0.336 μs | 0.314 μs | 2.4414 | 0.1221 |     - |     46 KB |
| InstantiateVenflowDatabase | 54.62 μs | 0.220 μs | 0.206 μs | 2.0142 | 0.1221 |     - |     37 KB |
