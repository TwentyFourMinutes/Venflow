``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                                  Method |     Mean |     Error |    StdDev |   Median | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------------- |---------:|----------:|----------:|---------:|------:|--------:|------:|------:|------:|----------:|
|                  EfCoreQuerySingleAsync | 6.111 ms | 0.1200 ms | 0.2255 ms | 5.989 ms |  1.00 |    0.00 |     - |     - |     - |     13 KB |
|  EfCoreQuerySingleNoChangeTrackingAsync | 6.544 ms | 0.0207 ms | 0.0184 ms | 6.547 ms |  1.02 |    0.03 |     - |     - |     - |     18 KB |
|                 VenflowQuerySingleAsync | 5.778 ms | 0.0236 ms | 0.0221 ms | 5.777 ms |  0.91 |    0.03 |     - |     - |     - |      7 KB |
| VenflowQuerySingleNoChangeTrackingAsync | 6.267 ms | 0.0134 ms | 0.0118 ms | 6.265 ms |  0.98 |    0.03 |     - |     - |     - |      7 KB |
|       RecommendedDapperQuerySingleAsync | 5.727 ms | 0.0158 ms | 0.0281 ms | 5.725 ms |  0.94 |    0.03 |     - |     - |     - |      5 KB |
|            CustomDapperQuerySingleAsync | 6.302 ms | 0.0183 ms | 0.0153 ms | 6.306 ms |  0.98 |    0.03 |     - |     - |     - |      5 KB |
