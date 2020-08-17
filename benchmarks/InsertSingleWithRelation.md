``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.388 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.7.20366.6
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4084.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT


```
|                   Method |           Job |       Runtime |     Mean |     Error |    StdDev |   Median | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------------------- |-------------- |-------------- |---------:|----------:|----------:|---------:|------:|--------:|--------:|-------:|------:|----------:|
|  EfCoreInsertSingleAsync |      .NET 4.8 |      .NET 4.8 | 3.063 ms | 0.0612 ms | 0.1569 ms | 3.003 ms |  1.00 |    0.00 | 31.2500 | 7.8125 |     - | 101.88 KB |
| VenflowInsertSingleAsync |      .NET 4.8 |      .NET 4.8 | 3.795 ms | 0.0724 ms | 0.2007 ms | 3.737 ms |  1.24 |    0.08 |  3.9063 |      - |     - |  22.13 KB |
|                          |               |               |          |           |           |          |       |         |         |        |       |           |
|  EfCoreInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 2.626 ms | 0.0509 ms | 0.0566 ms | 2.613 ms |  1.00 |    0.00 | 19.5313 | 3.9063 |     - |  85.26 KB |
| VenflowInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 3.492 ms | 0.0698 ms | 0.1442 ms | 3.465 ms |  1.34 |    0.07 |  3.9063 |      - |     - |  12.08 KB |
|                          |               |               |          |           |           |          |       |         |         |        |       |           |
|  EfCoreInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 2.615 ms | 0.0519 ms | 0.0792 ms | 2.591 ms |  1.00 |    0.00 | 23.4375 | 3.9063 |     - |  81.23 KB |
| VenflowInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 3.448 ms | 0.0689 ms | 0.0676 ms | 3.440 ms |  1.31 |    0.05 |  3.9063 |      - |     - |  12.03 KB |
