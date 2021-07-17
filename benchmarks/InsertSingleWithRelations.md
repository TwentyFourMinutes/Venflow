``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                   Method |     Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |---------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|  EfCoreInsertSingleAsync | 2.591 ms | 0.0515 ms | 0.0651 ms |  1.00 |    0.00 |     - |     - |     - |     64 KB |
| VenflowInsertSingleAsync | 1.584 ms | 0.0311 ms | 0.0305 ms |  0.61 |    0.02 |     - |     - |     - |     10 KB |
