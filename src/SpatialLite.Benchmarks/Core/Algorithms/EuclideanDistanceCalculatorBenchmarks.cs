using BenchmarkDotNet.Attributes;
using SpatialLite.Contracts;
using SpatialLite.Contracts.Algorithms;
using SpatialLite.Core.Algorithms;

namespace SpatialLite.Benchmarks.Core.Algorithms;

[MemoryDiagnoser]
public class EuclideanDistanceCalculatorBenchmarks
{
    private readonly EuclideanDistanceCalculator _calculator = new();

    // Test data for benchmarks
    private static readonly Coordinate C = new(5.0, 10.0);
    private static readonly Coordinate A = new(3.0, 5.0);
    private static readonly Coordinate B = new(-3.0, -5.0);

    [Benchmark]
    public double PointToPoint()
    {
        var distance = 0.0;
        for (var i = 0; i < 1000; i++)
        {
            distance += _calculator.CalculateDistance(C, A);
        }

        return distance;
    }

    [Benchmark]
    public double PointToLine()
    {
        var distance = 0.0;
        for (var i = 0; i < 1000; i++)
        {
            distance += _calculator.CalculateDistance(C, A, B, LineMode.Line);
        }

        return distance;
    }

    [Benchmark]
    public double PointToLineSegment()
    {
        var distance = 0.0;
        for (var i = 0; i < 1000; i++)
        {
            distance += _calculator.CalculateDistance(C, A, B, LineMode.LineSegment);
        }

        return distance;
    }
}
