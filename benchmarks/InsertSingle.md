``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.508 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-rc.1.20452.10
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4220.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT


```
|                   Method |           Job |       Runtime |     Mean |     Error |    StdDev |   Median | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-------------- |-------------- |---------:|----------:|----------:|---------:|------:|--------:|-------:|------:|------:|----------:|
|  EFCoreInsertSingleAsync |      .NET 4.8 |      .NET 4.8 | 1.604 ms | 0.0319 ms | 0.0868 ms | 1.577 ms |  1.00 |    0.00 | 7.8125 |     - |     - |  27.91 KB |
| VenflowInsertSingleAsync |      .NET 4.8 |      .NET 4.8 | 1.109 ms | 0.0221 ms | 0.0369 ms | 1.113 ms |  0.69 |    0.04 | 1.9531 |     - |     - |   6.42 KB |
|  RepoDbInsertSingleAsync |      .NET 4.8 |      .NET 4.8 | 1.084 ms | 0.0215 ms | 0.0409 ms | 1.080 ms |  0.67 |    0.04 | 1.9531 |     - |     - |   6.64 KB |
|                          |               |               |          |           |           |          |       |         |        |       |       |           |
|  EFCoreInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.395 ms | 0.0270 ms | 0.0396 ms | 1.389 ms |  1.00 |    0.00 | 5.8594 |     - |     - |  19.21 KB |
| VenflowInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.080 ms | 0.0207 ms | 0.0238 ms | 1.082 ms |  0.78 |    0.04 |      - |     - |     - |   3.34 KB |
|  RepoDbInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.079 ms | 0.0169 ms | 0.0180 ms | 1.080 ms |  0.78 |    0.03 |      - |     - |     - |   3.59 KB |
|                          |               |               |          |           |           |          |       |         |        |       |       |           |
|  EFCoreInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.375 ms | 0.0272 ms | 0.0537 ms | 1.367 ms |  1.00 |    0.00 | 5.8594 |     - |     - |  17.91 KB |
| VenflowInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.040 ms | 0.0207 ms | 0.0346 ms | 1.030 ms |  0.76 |    0.04 |      - |     - |     - |   3.31 KB |
|  RepoDbInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.048 ms | 0.0209 ms | 0.0485 ms | 1.046 ms |  0.77 |    0.05 |      - |     - |     - |   3.57 KB |
