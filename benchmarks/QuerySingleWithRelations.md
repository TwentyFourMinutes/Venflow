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
|                  EfCoreQuerySingleAsync | 7.488 ms | 0.1161 ms | 0.0970 ms |  1.00 |    0.00 |     - |     - |     - |     13 KB |
|  EfCoreQuerySingleNoChangeTrackingAsync | 7.134 ms | 0.1334 ms | 0.2230 ms |  0.97 |    0.03 |     - |     - |     - |     19 KB |
|                 VenflowQuerySingleAsync | 7.127 ms | 0.1359 ms | 0.1454 ms |  0.95 |    0.02 |     - |     - |     - |      7 KB |
| VenflowQuerySingleNoChangeTrackingAsync | 7.134 ms | 0.1345 ms | 0.1321 ms |  0.96 |    0.03 |     - |     - |     - |      7 KB |
|       RecommendedDapperQuerySingleAsync | 7.112 ms | 0.1119 ms | 0.0935 ms |  0.95 |    0.02 |     - |     - |     - |      5 KB |
|            CustomDapperQuerySingleAsync | 6.799 ms | 0.1168 ms | 0.1519 ms |  0.91 |    0.02 |     - |     - |     - |      5 KB |
