``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.508 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-rc.1.20452.10
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4220.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT


```
|                   Method |           Job |       Runtime |     Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------------------- |-------------- |-------------- |---------:|----------:|----------:|------:|--------:|-------:|-------:|------:|----------:|
|  EFCoreUpdateSingleAsync |      .NET 4.8 |      .NET 4.8 | 1.590 ms | 0.0315 ms | 0.0899 ms |  1.00 |    0.00 | 7.8125 |      - |     - |  24.08 KB |
| VenflowUpdateSingleAsync |      .NET 4.8 |      .NET 4.8 | 1.209 ms | 0.0240 ms | 0.0257 ms |  0.77 |    0.05 | 1.9531 |      - |     - |   6.58 KB |
|  RepoDbUpdateSingleAsync |      .NET 4.8 |      .NET 4.8 | 1.211 ms | 0.0242 ms | 0.0278 ms |  0.77 |    0.04 | 1.9531 |      - |     - |   9.34 KB |
|                          |               |               |          |           |           |       |         |        |        |       |           |
|  EFCoreUpdateSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.433 ms | 0.0286 ms | 0.0623 ms |  1.00 |    0.00 | 3.9063 |      - |     - |  15.76 KB |
| VenflowUpdateSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.173 ms | 0.0227 ms | 0.0262 ms |  0.82 |    0.04 |      - |      - |     - |   3.47 KB |
|  RepoDbUpdateSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 1.173 ms | 0.0230 ms | 0.0338 ms |  0.83 |    0.04 |      - |      - |     - |   5.53 KB |
|                          |               |               |          |           |           |       |         |        |        |       |           |
|  EFCoreUpdateSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.397 ms | 0.0278 ms | 0.0298 ms |  1.00 |    0.00 | 3.9063 | 1.9531 |     - |  15.04 KB |
| VenflowUpdateSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.134 ms | 0.0196 ms | 0.0299 ms |  0.82 |    0.03 |      - |      - |     - |   3.45 KB |
|  RepoDbUpdateSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 1.233 ms | 0.0243 ms | 0.0308 ms |  0.88 |    0.03 | 1.9531 |      - |     - |   9.02 KB |
