``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                   Method |       Mean |    Error |   StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-----------:|---------:|---------:|------:|--------:|------:|------:|------:|----------:|
|  EFCoreUpdateSingleAsync | 1,123.4 μs | 22.42 μs | 51.07 μs |  1.00 |    0.00 |     - |     - |     - |     13 KB |
| VenflowUpdateSingleAsync |   902.7 μs | 17.93 μs | 29.45 μs |  0.80 |    0.04 |     - |     - |     - |      4 KB |
|  RepoDbUpdateSingleAsync |   762.7 μs | 15.25 μs | 37.41 μs |  0.68 |    0.04 |     - |     - |     - |      7 KB |
