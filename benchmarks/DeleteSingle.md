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
|  EFCoreDeleteSingleAsync | 1.827 ms | 0.0364 ms | 0.0419 ms |  1.00 |    0.00 |     - |     - |     - |     18 KB |
| VenflowDeleteSingleAsync | 1.575 ms | 0.0314 ms | 0.0397 ms |  0.86 |    0.03 |     - |     - |     - |      8 KB |
|  RepoDbDeleteSingleAsync | 1.437 ms | 0.0285 ms | 0.0390 ms |  0.78 |    0.03 |     - |     - |     - |     11 KB |
