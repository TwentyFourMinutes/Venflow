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
|  EFCoreDeleteSingleAsync | 2.053 ms | 0.0406 ms | 0.0570 ms |  1.00 |    0.00 |     - |     - |     - |     18 KB |
| VenflowDeleteSingleAsync | 1.816 ms | 0.0361 ms | 0.0784 ms |  0.88 |    0.05 |     - |     - |     - |      8 KB |
|  RepoDbDeleteSingleAsync | 1.646 ms | 0.0328 ms | 0.0556 ms |  0.80 |    0.04 |     - |     - |     - |     11 KB |
