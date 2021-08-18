``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.7.21379.14
  [Host]   : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                        Method |     Mean |    Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------ |---------:|---------:|---------:|------:|------:|------:|----------:|
|    VenflowQueryWithParameters | 260.6 μs |  7.14 μs | 21.06 μs |     - |     - |     - |      2 KB |
| VenflowQueryWithInterpolation | 259.7 μs |  5.49 μs | 16.18 μs |     - |     - |     - |      3 KB |
|   VenflowQueryWithConstLambda | 308.3 μs |  9.64 μs | 28.26 μs |     - |     - |     - |      5 KB |
|   VenflowQueryWithLocalLambda | 316.3 μs |  6.83 μs | 20.14 μs |     - |     - |     - |      5 KB |
|   VenflowQueryWithFieldLambda | 305.8 μs |  9.54 μs | 28.13 μs |     - |     - |     - |      5 KB |
|     RepoDbQueryWithParameters | 279.8 μs |  5.55 μs | 12.18 μs |     - |     - |     - |      2 KB |
|     DapperQueryWithParameters | 241.3 μs |  6.88 μs | 20.17 μs |     - |     - |     - |      1 KB |
|            DapperQueryWithBag | 254.5 μs |  6.84 μs | 20.16 μs |     - |     - |     - |      2 KB |
|    EFCoreQueryWithConstLambda | 440.5 μs | 11.50 μs | 33.74 μs |     - |     - |     - |      7 KB |
|    EFCoreQueryWithLocalLambda | 526.6 μs | 11.70 μs | 34.49 μs |     - |     - |     - |      7 KB |
|    EFCoreQueryWithFieldLambda | 490.5 μs | 12.21 μs | 36.02 μs |     - |     - |     - |      7 KB |
