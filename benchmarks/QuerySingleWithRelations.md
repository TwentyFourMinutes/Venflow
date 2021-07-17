``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                                  Method |     Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------------- |---------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|                  EfCoreQuerySingleAsync | 7.771 ms | 0.1546 ms | 0.2064 ms |  1.00 |    0.00 |     - |     - |     - |     13 KB |
|  EfCoreQuerySingleNoChangeTrackingAsync | 8.328 ms | 0.1663 ms | 0.1780 ms |  1.07 |    0.04 |     - |     - |     - |     18 KB |
|                 VenflowQuerySingleAsync | 8.173 ms | 0.1596 ms | 0.2018 ms |  1.05 |    0.04 |     - |     - |     - |      7 KB |
| VenflowQuerySingleNoChangeTrackingAsync | 8.305 ms | 0.1614 ms | 0.2560 ms |  1.07 |    0.04 |     - |     - |     - |      7 KB |
|       RecommendedDapperQuerySingleAsync | 8.423 ms | 0.1670 ms | 0.1856 ms |  1.08 |    0.04 |     - |     - |     - |      5 KB |
|            CustomDapperQuerySingleAsync | 7.608 ms | 0.1458 ms | 0.1996 ms |  0.98 |    0.02 |     - |     - |     - |      5 KB |
