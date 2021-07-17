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
|  EFCoreDeleteSingleAsync | 1.959 ms | 0.0379 ms | 0.0405 ms |  1.00 |    0.00 |     - |     - |     - |     18 KB |
| VenflowDeleteSingleAsync | 1.703 ms | 0.0330 ms | 0.0451 ms |  0.87 |    0.04 |     - |     - |     - |      8 KB |
|  RepoDbDeleteSingleAsync | 1.574 ms | 0.0312 ms | 0.0359 ms |  0.80 |    0.03 |     - |     - |     - |     11 KB |
