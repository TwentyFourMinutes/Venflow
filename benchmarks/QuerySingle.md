``` ini

BenchmarkDotNet=v0.13.0, OS=ubuntu 20.04
Intel Xeon Platinum 8272CL CPU 2.60GHz, 1 CPU, 2 logical and 2 physical cores
.NET SDK=6.0.100-preview.5.21302.13
  [Host]   : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT
  .NET 6.0 : .NET 6.0.0 (6.0.21.30105), X64 RyuJIT

Job=.NET 6.0  Runtime=.NET 6.0  

```
|                                    Method |     Mean |   Error |   StdDev |   Median | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------------ |---------:|--------:|---------:|---------:|------:|--------:|-------:|------:|------:|----------:|
|                    EfCoreQuerySingleAsync | 251.9 μs | 5.02 μs | 13.82 μs | 255.1 μs |  1.00 |    0.00 |      - |     - |     - |   3,830 B |
|    EfCoreQuerySingleNoChangeTrackingAsync | 262.2 μs | 5.19 μs | 13.03 μs | 263.5 μs |  1.04 |    0.08 |      - |     - |     - |   6,453 B |
| EfCoreQuerySingleRawNoChangeTrackingAsync | 335.4 μs | 8.16 μs | 24.07 μs | 335.7 μs |  1.33 |    0.13 | 0.4883 |     - |     - |  11,936 B |
|                   VenflowQuerySingleAsync | 147.9 μs | 3.88 μs | 11.39 μs | 145.0 μs |  0.59 |    0.06 |      - |     - |     - |   2,057 B |
|   VenflowQuerySingleNoChangeTrackingAsync | 144.9 μs | 4.34 μs | 12.79 μs | 144.8 μs |  0.58 |    0.07 |      - |     - |     - |   2,330 B |
|                    RepoDbQuerySingleAsync | 162.0 μs | 4.76 μs | 13.96 μs | 161.4 μs |  0.65 |    0.06 |      - |     - |     - |   3,219 B |
|                    DapperQuerySingleAsync | 135.8 μs | 2.89 μs |  8.33 μs | 132.1 μs |  0.54 |    0.04 |      - |     - |     - |     720 B |
