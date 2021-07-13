``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                   Method |       Mean |    Error |   StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-----------:|---------:|---------:|------:|--------:|------:|------:|------:|----------:|
|  EfCoreInsertSingleAsync | 1,740.7 μs | 29.70 μs | 26.33 μs |  1.00 |    0.00 |     - |     - |     - |     64 KB |
| VenflowInsertSingleAsync |   986.1 μs | 15.15 μs | 14.17 μs |  0.57 |    0.02 |     - |     - |     - |     10 KB |
