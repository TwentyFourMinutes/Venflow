``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.388 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.7.20366.6
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4084.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT


```
|                               Method |           Job |       Runtime |     Mean |     Error |    StdDev |   Median | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------- |-------------- |-------------- |---------:|----------:|----------:|---------:|------:|--------:|-------:|------:|------:|----------:|
|              EFCoreInsertSingleAsync |      .NET 4.8 |      .NET 4.8 | 1.558 ms | 0.0203 ms | 0.0170 ms | 1.558 ms |  1.00 |    0.00 | 7.8125 |     - |     - |  27.91 KB |
|             VenflowInsertSingleAsync |      .NET 4.8 |      .NET 4.8 | 1.109 ms | 0.0217 ms | 0.0223 ms | 1.114 ms |  0.71 |    0.01 | 1.9531 |     - |     - |   8.05 KB |
| VenflowInsertSingleWithPKReturnAsync |      .NET 4.8 |      .NET 4.8 | 1.101 ms | 0.0206 ms | 0.0220 ms | 1.103 ms |  0.70 |    0.02 | 1.9531 |     - |     - |   8.03 KB |
|              RepoDbInsertSingleAsync |      .NET 4.8 |      .NET 4.8 | 1.108 ms | 0.0219 ms | 0.0367 ms | 1.094 ms |  0.72 |    0.03 | 1.9531 |     - |     - |   6.41 KB |
|                                      |               |               |          |           |           |          |       |         |        |       |       |           |
|              EFCoreInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.434 ms | 0.0255 ms | 0.0261 ms | 1.429 ms |  1.00 |    0.00 | 5.8594 |     - |     - |  19.21 KB |
|             VenflowInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.116 ms | 0.0219 ms | 0.0204 ms | 1.115 ms |  0.78 |    0.02 |      - |     - |     - |    4.5 KB |
| VenflowInsertSingleWithPKReturnAsync | .NET Core 3.1 | .NET Core 3.1 | 1.129 ms | 0.0153 ms | 0.0136 ms | 1.125 ms |  0.79 |    0.01 |      - |     - |     - |    4.5 KB |
|              RepoDbInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.077 ms | 0.0210 ms | 0.0225 ms | 1.080 ms |  0.75 |    0.03 |      - |     - |     - |   3.46 KB |
|                                      |               |               |          |           |           |          |       |         |        |       |       |           |
|              EFCoreInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.436 ms | 0.0262 ms | 0.0416 ms | 1.430 ms |  1.00 |    0.00 | 5.8594 |     - |     - |  19.48 KB |
|             VenflowInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.136 ms | 0.0227 ms | 0.0478 ms | 1.114 ms |  0.80 |    0.04 |      - |     - |     - |   4.49 KB |
| VenflowInsertSingleWithPKReturnAsync | .NET Core 5.0 | .NET Core 5.0 | 1.170 ms | 0.0215 ms | 0.0179 ms | 1.175 ms |  0.80 |    0.03 |      - |     - |     - |   4.49 KB |
|              RepoDbInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.071 ms | 0.0213 ms | 0.0373 ms | 1.073 ms |  0.74 |    0.03 |      - |     - |     - |   3.43 KB |
