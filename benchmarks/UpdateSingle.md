``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.388 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-preview.7.20366.6
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4084.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.6 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.31603), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.36411, CoreFX 5.0.20.36411), X64 RyuJIT


```
|                   Method |           Job |       Runtime |     Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-------------- |-------------- |---------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|  EFCoreUpdateSingleAsync |      .NET 4.8 |      .NET 4.8 | 1.367 ms | 0.0393 ms | 0.1158 ms |  1.00 |    0.00 | 7.8125 |     - |     - |  24.09 KB |
| VenflowUpdateSingleAsync |      .NET 4.8 |      .NET 4.8 | 1.044 ms | 0.0407 ms | 0.1200 ms |  0.77 |    0.10 | 1.9531 |     - |     - |   6.52 KB |
|  RepoDbUpdateSingleAsync |      .NET 4.8 |      .NET 4.8 | 1.136 ms | 0.0222 ms | 0.0297 ms |  0.87 |    0.07 | 1.9531 |     - |     - |   8.69 KB |
|                          |               |               |          |           |           |       |         |        |       |       |           |
|  EFCoreUpdateSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.444 ms | 0.0259 ms | 0.0319 ms |  1.00 |    0.00 | 3.9063 |     - |     - |  15.79 KB |
| VenflowUpdateSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.248 ms | 0.0249 ms | 0.0277 ms |  0.87 |    0.03 |      - |     - |     - |   3.45 KB |
|  RepoDbUpdateSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.267 ms | 0.0248 ms | 0.0255 ms |  0.88 |    0.03 |      - |     - |     - |   5.01 KB |
|                          |               |               |          |           |           |       |         |        |       |       |           |
|  EFCoreUpdateSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.428 ms | 0.0252 ms | 0.0223 ms |  1.00 |    0.00 | 3.9063 |     - |     - |  16.11 KB |
| VenflowUpdateSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.211 ms | 0.0240 ms | 0.0636 ms |  0.88 |    0.03 |      - |     - |     - |   3.44 KB |
|  RepoDbUpdateSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.230 ms | 0.0241 ms | 0.0295 ms |  0.87 |    0.02 | 1.9531 |     - |     - |    8.5 KB |
