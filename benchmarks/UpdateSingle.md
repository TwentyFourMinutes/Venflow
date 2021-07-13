``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                   Method |     Mean |    Error |   StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |---------:|---------:|---------:|------:|--------:|------:|------:|------:|----------:|
|  EFCoreUpdateSingleAsync | 721.1 μs |  8.56 μs |  7.59 μs |  1.00 |    0.00 |     - |     - |     - |     13 KB |
| VenflowUpdateSingleAsync | 555.6 μs | 10.99 μs | 18.66 μs |  0.76 |    0.04 |     - |     - |     - |      4 KB |
|  RepoDbUpdateSingleAsync | 437.7 μs |  8.47 μs | 19.62 μs |  0.61 |    0.02 |     - |     - |     - |      7 KB |
