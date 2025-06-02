using BenchmarkDotNet.Attributes;
using SpatialLITE.Contracts;

namespace SpatialLITE.Benchmarks.Contracts;

[MemoryDiagnoser]
public class EnvelopeBenchmarks
{
    private readonly Coordinate _singleCoordinate = new(10.0, 20.0);

    private readonly IEnumerable<Coordinate> _multipleCoordinates =
    [
        new(10.0, 20.0),
        new(30.0, 40.0),
        new(5.0, 15.0)
    ];

    private readonly Envelope _envelope = new([new(5, 5), new(-5, -5)]);
    private readonly Envelope _otherEnvelope = new([new(0, 0), new(-10, -10)]);

    [Benchmark]
    public Envelope SingleCoordinateConstructor()
    {
        return new Envelope(_singleCoordinate);
    }

    [Benchmark]
    public Envelope MultipleCoordinatesConstructor()
    {
        return new Envelope(_multipleCoordinates);
    }

    [Benchmark]
    public Envelope ExtendWithCoordinate()
    {
        return _envelope.Extend(_singleCoordinate);
    }

    [Benchmark]
    public Envelope ExtendWithCoordinates()
    {
        return _envelope.Extend(_multipleCoordinates);
    }

    [Benchmark]
    public Envelope ExtendWithEnvelope()
    {
        return _envelope.Extend(_otherEnvelope);
    }

    [Benchmark]
    public bool IntersectsEnvelope()
    {
        return _envelope.Intersects(_otherEnvelope);
    }

    [Benchmark]
    public bool CoversXY()
    {
        return _envelope.Covers(15.0, 25.0);
    }

    [Benchmark]
    public bool CoversCoordinate()
    {
        return _envelope.Covers(_singleCoordinate);
    }

    [Benchmark]
    public bool CoversEnvelope()
    {
        return _envelope.Covers(_otherEnvelope);
    }
}
