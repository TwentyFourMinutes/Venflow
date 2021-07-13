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
|  EFCoreInsertSingleAsync | 737.3 μs | 14.19 μs | 15.77 μs |  1.00 |    0.00 |     - |     - |     - |     16 KB |
| VenflowInsertSingleAsync | 536.2 μs | 10.48 μs | 13.25 μs |  0.73 |    0.03 |     - |     - |     - |      4 KB |
|  RepoDbInsertSingleAsync | 384.1 μs |  7.68 μs | 22.51 μs |  0.50 |    0.02 |     - |     - |     - |      3 KB |
