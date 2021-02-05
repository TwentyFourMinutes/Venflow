``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon CPU E5-2673 v4 2.30GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                   Method |           Job |       Runtime |     Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-------------- |-------------- |---------:|----------:|----------:|------:|--------:|------:|------:|------:|----------:|
|  EfCoreInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 2.257 ms | 0.0446 ms | 0.0978 ms |  1.00 |    0.00 |     - |     - |     - |  83.05 KB |
| VenflowInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.322 ms | 0.0258 ms | 0.0353 ms |  0.59 |    0.03 |     - |     - |     - |  13.76 KB |
|                          |               |               |          |           |           |       |         |       |       |       |           |
|  EfCoreInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 2.136 ms | 0.0415 ms | 0.0368 ms |  1.00 |    0.00 |     - |     - |     - |  70.73 KB |
| VenflowInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.317 ms | 0.0263 ms | 0.0447 ms |  0.61 |    0.02 |     - |     - |     - |  13.72 KB |
