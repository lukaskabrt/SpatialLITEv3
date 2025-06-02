using SpatialLite.Contracts;
using SpatialLite.Core.Geometries;

namespace SpatialLite.UnitTests.Core.Geometries;

public class LineStringTests
{
    private readonly Coordinate[] _coordinatesXY = [
        new Coordinate(12, 10),
        new Coordinate(22, 20),
        new Coordinate(32, 30)
    ];

    [Fact]
    public void Constructor__CreatesEmptyLineString()
    {
        var target = new LineString();

        Assert.Empty(target.Coordinates);
    }

    [Fact]
    public void Constructor_IEnumerable_CreatesLineStringFromCoordinates()
    {
        var target = new LineString(_coordinatesXY);

        Assert.Equal(_coordinatesXY, target.Coordinates, (e, a) => e.Equals(a));
    }

    [Fact]
    public void Start_ReturnsEmptyCoordinateForEmptyLineString()
    {
        var target = new LineString();

        Assert.Equal(Coordinate.Empty, target.Start);
    }

    [Fact]
    public void Start_ReturnsFirstCoordinate()
    {
        var target = new LineString(_coordinatesXY);

        Assert.Equal(_coordinatesXY.First(), target.Start);
    }

    [Fact]
    public void End_ReturnsEmptyCoordinateForEmptyLineString()
    {
        var target = new LineString();

        Assert.Equal(Coordinate.Empty, target.End);
    }

    [Fact]
    public void End_ReturnsLastCoordinate()
    {
        var target = new LineString(_coordinatesXY);

        Assert.Equal(_coordinatesXY.Last(), target.End);
    }

    [Fact]
    public void IsClosed_ReturnsTrueForClosedLineString()
    {
        var target = new LineString([.. _coordinatesXY, _coordinatesXY[0]]);

        Assert.True(target.IsClosed);
    }

    [Fact]
    public void IsClosed_ReturnsFalseForOpenLineString()
    {
        var target = new LineString(_coordinatesXY);

        Assert.False(target.IsClosed);
    }

    [Fact]
    public void IsClosed_ReturnsFalseForEmptyLineString()
    {
        var target = new LineString();

        Assert.False(target.IsClosed);
    }

    [Fact]
    public void GetEnvelope_ReturnsEmptyEnvelopeForEmptyLineString()
    {
        var target = new LineString();
        var envelope = target.GetEnvelope();

        Assert.Equal(Envelope.Empty, envelope);
    }

    [Fact]
    public void GetEnvelope_ReturnsEnvelopeOfLineString()
    {
        var target = new LineString(_coordinatesXY);
        var expected = new Envelope(_coordinatesXY);

        Assert.Equal(expected, target.GetEnvelope());
    }
}
