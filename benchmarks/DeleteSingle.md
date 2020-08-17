``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.388 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.7.20366.6
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4084.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT


```
|                   Method |           Job |       Runtime |     Mean |     Error |    StdDev |   Median | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-------------- |-------------- |---------:|----------:|----------:|---------:|------:|--------:|-------:|------:|------:|----------:|
|  EFCoreDeleteSingleAsync |      .NET 4.8 |      .NET 4.8 | 3.055 ms | 0.0601 ms | 0.0936 ms | 3.068 ms |  1.00 |    0.00 | 7.8125 |     - |     - |  31.63 KB |
| VenflowDeleteSingleAsync |      .NET 4.8 |      .NET 4.8 | 2.494 ms | 0.0747 ms | 0.2167 ms | 2.479 ms |  0.82 |    0.07 | 3.9063 |     - |     - |  13.94 KB |
|  RepoDbDeleteSingleAsync |      .NET 4.8 |      .NET 4.8 | 2.605 ms | 0.0819 ms | 0.2349 ms | 2.573 ms |  0.86 |    0.08 | 3.9063 |     - |     - |  16.69 KB |
|                          |               |               |          |           |           |          |       |         |        |       |       |           |
|  EFCoreDeleteSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 3.042 ms | 0.1030 ms | 0.3021 ms | 2.995 ms |  1.00 |    0.00 | 3.9063 |     - |     - |  19.56 KB |
| VenflowDeleteSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 2.530 ms | 0.0706 ms | 0.1968 ms | 2.506 ms |  0.84 |    0.09 |      - |     - |     - |   7.47 KB |
|  RepoDbDeleteSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 2.536 ms | 0.0572 ms | 0.1545 ms | 2.541 ms |  0.85 |    0.10 |      - |     - |     - |   9.57 KB |
|                          |               |               |          |           |           |          |       |         |        |       |       |           |
|  EFCoreDeleteSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 3.078 ms | 0.1211 ms | 0.3572 ms | 3.003 ms |  1.00 |    0.00 | 3.9063 |     - |     - |  19.96 KB |
| VenflowDeleteSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 2.648 ms | 0.1188 ms | 0.3445 ms | 2.519 ms |  0.87 |    0.15 |      - |     - |     - |   7.44 KB |
|  RepoDbDeleteSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 2.632 ms | 0.0515 ms | 0.0754 ms | 2.632 ms |  0.88 |    0.11 | 3.9063 |     - |     - |  17.23 KB |
