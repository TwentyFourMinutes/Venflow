``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.7.21379.14
  [Host]   : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.37719), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                   Method |     Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |---------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|  EfCoreInsertSingleAsync | 2.337 ms | 0.0459 ms | 0.0429 ms |  1.00 |    0.00 |     - |     - |     - |     65 KB |
| VenflowInsertSingleAsync | 1.468 ms | 0.0266 ms | 0.0336 ms |  0.62 |    0.02 |     - |     - |     - |     10 KB |
