``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.508 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-rc.1.20452.10
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4220.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT


```
|                   Method |           Job |       Runtime |     Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-------------- |-------------- |---------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|  EFCoreDeleteSingleAsync |      .NET 4.8 |      .NET 4.8 | 2.951 ms | 0.0713 ms | 0.2102 ms |  1.00 |    0.00 | 7.8125 |     - |     - |  30.83 KB |
| VenflowDeleteSingleAsync |      .NET 4.8 |      .NET 4.8 | 2.453 ms | 0.0490 ms | 0.1382 ms |  0.84 |    0.08 | 3.9063 |     - |     - |  13.44 KB |
|  RepoDbDeleteSingleAsync |      .NET 4.8 |      .NET 4.8 | 2.496 ms | 0.0689 ms | 0.2032 ms |  0.85 |    0.09 | 3.9063 |     - |     - |  16.67 KB |
|                          |               |               |          |           |           |       |         |        |       |       |           |
|  EFCoreDeleteSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 2.862 ms | 0.0638 ms | 0.1829 ms |  1.00 |    0.00 | 3.9063 |     - |     - |   18.9 KB |
| VenflowDeleteSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 2.390 ms | 0.0473 ms | 0.0999 ms |  0.84 |    0.06 |      - |     - |     - |   6.86 KB |
|  RepoDbDeleteSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 2.444 ms | 0.0486 ms | 0.1046 ms |  0.86 |    0.07 |      - |     - |     - |   9.28 KB |
|                          |               |               |          |           |           |       |         |        |       |       |           |
|  EFCoreDeleteSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 2.829 ms | 0.0774 ms | 0.2281 ms |  1.00 |    0.00 | 3.9063 |     - |     - |  19.55 KB |
| VenflowDeleteSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 2.324 ms | 0.0464 ms | 0.1102 ms |  0.82 |    0.08 |      - |     - |     - |   6.82 KB |
|  RepoDbDeleteSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 2.352 ms | 0.0468 ms | 0.1156 ms |  0.83 |    0.07 | 3.9063 |     - |     - |  16.93 KB |
