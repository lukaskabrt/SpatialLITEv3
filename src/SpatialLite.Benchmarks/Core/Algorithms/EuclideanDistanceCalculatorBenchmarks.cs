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
    private readonly Coordinate _testPoint = new(5.0, 10.0);
    private readonly Coordinate _linePointA = new(0.0, 0.0);
    private readonly Coordinate _linePointB = new(10.0, 0.0);

    // Collections for different N values
    private Coordinate[] _points1 = [];
    private Coordinate[] _points10 = [];
    private Coordinate[] _points100 = [];

    private (Coordinate A, Coordinate B)[] _lines1 = [];
    private (Coordinate A, Coordinate B)[] _lines10 = [];
    private (Coordinate A, Coordinate B)[] _lines100 = [];

    [Params(1, 10, 100)]
    public int N { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // Setup points for point-to-point benchmarks
        _points1 = [new(1.0, 1.0)];

        _points10 = new Coordinate[10];
        for (int i = 0; i < 10; i++)
        {
            _points10[i] = new(i * 2.0, i * 3.0);
        }

        _points100 = new Coordinate[100];
        for (int i = 0; i < 100; i++)
        {
            _points100[i] = new(i * 0.5, i * 0.7);
        }

        // Setup lines for point-to-line benchmarks
        _lines1 = [(new(0.0, 0.0), new(10.0, 0.0))];

        _lines10 = new (Coordinate A, Coordinate B)[10];
        for (int i = 0; i < 10; i++)
        {
            _lines10[i] = (new(i * 2.0, i * 2.0), new(i * 2.0 + 5.0, i * 2.0 + 3.0));
        }

        _lines100 = new (Coordinate A, Coordinate B)[100];
        for (int i = 0; i < 100; i++)
        {
            _lines100[i] = (new(i * 0.5, i * 0.3), new(i * 0.5 + 2.0, i * 0.3 + 1.5));
        }
    }

    [Benchmark]
    public double PointToPoint()
    {
        var points = N switch
        {
            1 => _points1,
            10 => _points10,
            100 => _points100,
            _ => throw new ArgumentException("Invalid N value")
        };

        double totalDistance = 0.0;
        foreach (var point in points)
        {
            totalDistance += _calculator.CalculateDistance(_testPoint, point);
        }

        return totalDistance;
    }

    [Benchmark]
    public double PointToLine()
    {
        var lines = N switch
        {
            1 => _lines1,
            10 => _lines10,
            100 => _lines100,
            _ => throw new ArgumentException("Invalid N value")
        };

        double totalDistance = 0.0;
        foreach (var (a, b) in lines)
        {
            totalDistance += _calculator.CalculateDistance(_testPoint, a, b, LineMode.Line);
        }

        return totalDistance;
    }

    [Benchmark]
    public double PointToLineSegment()
    {
        var lines = N switch
        {
            1 => _lines1,
            10 => _lines10,
            100 => _lines100,
            _ => throw new ArgumentException("Invalid N value")
        };

        double totalDistance = 0.0;
        foreach (var (a, b) in lines)
        {
            totalDistance += _calculator.CalculateDistance(_testPoint, a, b, LineMode.LineSegment);
        }

        return totalDistance;
    }
}