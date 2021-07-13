``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                   Method |     Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |---------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|  EFCoreDeleteSingleAsync | 1.249 ms | 0.0246 ms | 0.0481 ms |  1.00 |    0.00 |     - |     - |     - |     18 KB |
| VenflowDeleteSingleAsync | 1.065 ms | 0.0193 ms | 0.0207 ms |  0.87 |    0.06 |     - |     - |     - |      8 KB |
|  RepoDbDeleteSingleAsync | 1.020 ms | 0.0169 ms | 0.0158 ms |  0.84 |    0.05 |     - |     - |     - |     11 KB |
