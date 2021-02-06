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
|  EfCoreInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 2.539 ms | 0.0500 ms | 0.0443 ms |  1.00 |    0.00 | 3.9063 |     - |     - |  83.08 KB |
| VenflowInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.524 ms | 0.0293 ms | 0.0380 ms |  0.60 |    0.02 |      - |     - |     - |  13.85 KB |
|                          |               |               |          |           |           |       |         |        |       |       |           |
|  EfCoreInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 2.543 ms | 0.0497 ms | 0.0532 ms |  1.00 |    0.00 |      - |     - |     - |  70.72 KB |
| VenflowInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.494 ms | 0.0298 ms | 0.0446 ms |  0.59 |    0.02 |      - |     - |     - |  13.81 KB |
