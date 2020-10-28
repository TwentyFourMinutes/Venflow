``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.508 (2004/?/20H1)
Intel Core i7-6700HQ CPU 2.60GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100-rc.1.20452.10
  [Host]        : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT
  .NET 4.8      : .NET Framework 4.8 (4.8.4220.0), X64 RyuJIT
  .NET Core 3.1 : .NET Core 3.1.7 (CoreCLR 4.700.20.36602, CoreFX 4.700.20.37001), X64 RyuJIT
  .NET Core 5.0 : .NET Core 5.0.0 (CoreCLR 5.0.20.45114, CoreFX 5.0.20.45114), X64 RyuJIT


```
|                   Method |           Job |       Runtime |     Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------------------- |-------------- |-------------- |---------:|----------:|----------:|------:|--------:|--------:|-------:|------:|----------:|
|  EfCoreInsertSingleAsync |      .NET 4.8 |      .NET 4.8 | 2.826 ms | 0.0432 ms | 0.0405 ms |  1.00 |    0.00 | 31.2500 | 7.8125 |     - | 101.85 KB |
| VenflowInsertSingleAsync |      .NET 4.8 |      .NET 4.8 | 3.424 ms | 0.0585 ms | 0.1155 ms |  1.21 |    0.06 |  3.9063 |      - |     - |  20.22 KB |
|                          |               |               |          |           |           |       |         |         |        |       |           |
|  EfCoreInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 2.618 ms | 0.0425 ms | 0.0332 ms |  1.00 |    0.00 | 23.4375 | 3.9063 |     - |  81.52 KB |
| VenflowInsertSingleAsync | .NET Core 3.1 | .NET Core 3.1 | 3.502 ms | 0.0808 ms | 0.2318 ms |  1.31 |    0.06 |       - |      - |     - |  10.77 KB |
|                          |               |               |          |           |           |       |         |         |        |       |           |
|  EfCoreInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 2.491 ms | 0.0479 ms | 0.0570 ms |  1.00 |    0.00 | 19.5313 | 3.9063 |     - |  76.64 KB |
| VenflowInsertSingleAsync | .NET Core 5.0 | .NET Core 5.0 | 3.389 ms | 0.0614 ms | 0.1027 ms |  1.36 |    0.06 |       - |      - |     - |  10.71 KB |
