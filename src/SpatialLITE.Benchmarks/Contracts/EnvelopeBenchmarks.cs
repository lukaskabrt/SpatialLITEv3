using BenchmarkDotNet.Attributes;
using SpatialLITE.Contracts;

namespace SpatialLITE.Benchmarks.Contracts;

[MemoryDiagnoser]
public class EnvelopeBenchmarks
{
    private readonly Coordinate _singleCoordinate = new(10.0, 20.0);
    private readonly IEnumerable<Coordinate> _multipleCoordinates;
    private readonly Envelope _envelope;

    public EnvelopeBenchmarks()
    {
        _multipleCoordinates = new List<Coordinate>
        {
            new(10.0, 20.0),
            new(30.0, 40.0),
            new(5.0, 15.0)
        };

        _envelope = new Envelope
        {
            MinX = 5.0,
            MaxX = 30.0,
            MinY = 15.0,
            MaxY = 40.0
        };
    }

    [Benchmark]
    public Envelope EmptyConstructor()
    {
        return new Envelope();
    }

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
    public double Width()
    {
        return _envelope.Width;
    }

    [Benchmark]
    public double Height()
    {
        return _envelope.Height;
    }

    [Benchmark]
    public bool CheckIsEmpty()
    {
        return _envelope.IsEmpty;
    }

    [Benchmark]
    public Envelope ExtendWithCoordinate()
    {
        return _envelope.Extend(new Coordinate(50.0, 60.0));
    }

    [Benchmark]
    public Envelope ExtendWithCoordinates()
    {
        return _envelope.Extend(_multipleCoordinates);
    }

    [Benchmark]
    public Envelope ExtendWithEnvelope()
    {
        return _envelope.Extend(new Envelope
        {
            MinX = 20.0,
            MaxX = 40.0,
            MinY = 30.0,
            MaxY = 50.0
        });
    }

    [Benchmark]
    public bool IntersectsEnvelope()
    {
        return _envelope.Intersects(new Envelope
        {
            MinX = 20.0,
            MaxX = 40.0,
            MinY = 30.0,
            MaxY = 50.0
        });
    }

    [Benchmark]
    public bool CoversXY()
    {
        return _envelope.Covers(15.0, 25.0);
    }

    [Benchmark]
    public bool CoversCoordinate()
    {
        return _envelope.Covers(new Coordinate(15.0, 25.0));
    }

    [Benchmark]
    public bool CoversEnvelope()
    {
        return _envelope.Covers(new Envelope
        {
            MinX = 10.0,
            MaxX = 20.0,
            MinY = 20.0,
            MaxY = 30.0
        });
    }
}