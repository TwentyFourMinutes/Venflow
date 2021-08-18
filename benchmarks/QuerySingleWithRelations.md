``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.7.21379.14
  [Host]   : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                                  Method |     Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------------- |---------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                  EfCoreQuerySingleAsync | 7.900 ms | 0.1567 ms | 0.4155 ms |  1.00 |    0.00 |     - |     - |     - |     13 KB |
|  EfCoreQuerySingleNoChangeTrackingAsync | 8.026 ms | 0.1575 ms | 0.2997 ms |  0.98 |    0.04 |     - |     - |     - |     17 KB |
|                 VenflowQuerySingleAsync | 7.085 ms | 0.1408 ms | 0.2466 ms |  0.86 |    0.05 |     - |     - |     - |      7 KB |
| VenflowQuerySingleNoChangeTrackingAsync | 7.814 ms | 0.1543 ms | 0.2060 ms |  0.96 |    0.05 |     - |     - |     - |      6 KB |
|       RecommendedDapperQuerySingleAsync | 7.650 ms | 0.1091 ms | 0.0911 ms |  0.92 |    0.04 |     - |     - |     - |      5 KB |
|            CustomDapperQuerySingleAsync | 7.757 ms | 0.1551 ms | 0.2017 ms |  0.96 |    0.05 |     - |     - |     - |      5 KB |
