``` ini

BenchmarkDotNet=v0.12.1, OS=ubuntu 18.04
Intel Xeon Platinum 8171M CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET Core SDK=5.0.102
  [Host]        : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.11 (CoreCLR 4.700.20.56602, CoreFX 4.700.20.56604), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT


```
|                   Method |           Job |       Runtime |     Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-------------- |-------------- |---------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|  EfCoreInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 2.257 ms | 0.0435 ms | 0.0610 ms |  1.00 |    0.00 | 3.9063 |     - |     - |  83.07 KB |
| VenflowInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.284 ms | 0.0251 ms | 0.0335 ms |  0.57 |    0.02 |      - |     - |     - |  13.83 KB |
|                          |               |               |          |           |           |       |         |        |       |       |           |
|  EfCoreInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 2.251 ms | 0.0431 ms | 0.0545 ms |  1.00 |    0.00 |      - |     - |     - |  70.72 KB |
| VenflowInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.282 ms | 0.0246 ms | 0.0292 ms |  0.57 |    0.02 |      - |     - |     - |  13.78 KB |
