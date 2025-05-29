```

BenchmarkDotNet v0.15.0, Linux Ubuntu 24.04.2 LTS (Noble Numbat)
AMD EPYC 7763, 1 CPU, 2 logical cores and 1 physical core
.NET SDK 8.0.115
  [Host]     : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.15 (8.0.1525.16413), X64 RyuJIT AVX2


```
| Method                         | Mean          | Error       | StdDev        | Median        | Gen0   | Allocated |
|------------------------------- |--------------:|------------:|--------------:|--------------:|-------:|----------:|
| SingleCoordinateConstructor    |     0.0202 ns |   0.0058 ns |     0.0049 ns |     0.0189 ns |      - |         - |
| MultipleCoordinatesConstructor |    51.2554 ns |   0.3676 ns |     0.3438 ns |    51.2565 ns | 0.0038 |      64 B |
| ExtendWithCoordinate           |     6.2890 ns |   0.0458 ns |     0.0358 ns |     6.2856 ns |      - |         - |
| ExtendWithCoordinates          |    39.8002 ns |   0.4653 ns |     0.3885 ns |    39.6652 ns | 0.0019 |      32 B |
| ExtendWithEnvelope             |    10.6731 ns |   0.7344 ns |     2.1653 ns |     9.0587 ns |      - |         - |
| IntersectsEnvelope             |     6.3650 ns |   0.1644 ns |     0.1759 ns |     6.3004 ns |      - |         - |
| CoversXY                       |     4.8431 ns |   0.8758 ns |     2.5824 ns |     3.9830 ns |      - |         - |
| CoversCoordinate               | 8,119.4018 ns | 727.5789 ns | 2,145.2828 ns | 9,216.2263 ns | 0.2747 |    4832 B |
| CoversEnvelope                 |   893.5817 ns |  93.5202 ns |   271.3191 ns |   802.6248 ns | 0.0429 |     728 B |
