using SpatialLite.Contracts;
using SpatialLite.Core.Geometries;

namespace SpatialLite.UnitTests.Core.Geometries;

public class PointTests
{
    private readonly double _xOrdinate = 3.5;
    private readonly double _yOrdinate = 4.2;

    private readonly Coordinate _coordinate = new(3.5, 4.2);

    [Fact]
    public void Constructor__CreatesPointWithEmptyPosition()
    {
        var target = new Point();

        Assert.Equal(Coordinate.Empty, target.Position);
    }

    [Fact]
    public void Constructor_XY_SetsCoordinates()
    {
        var target = new Point(_xOrdinate, _yOrdinate);

        Assert.Equal(_xOrdinate, target.Position.X);
        Assert.Equal(_yOrdinate, target.Position.Y);
    }

    [Fact]
    public void Constructor_Coordinate_SetsCoordinates()
    {
        var target = new Point(_coordinate);

        Assert.Equal(_coordinate, target.Position);
    }

    [Fact]
    public void GetEnvelope_ReturnsEmptyEnvelopeForEmptyPoint()
    {
        var target = new Point();
        Envelope envelope = target.GetEnvelope();

        Assert.Equal(Envelope.Empty, envelope);
    }

    [Fact]
    public void GetEnvelope_ReturnsEnvelopeThatCoversOnePoint()
    {
        var target = new Point(_coordinate);
        Envelope envelope = target.GetEnvelope();

        Assert.Equal(_coordinate.X, envelope.MinX);
        Assert.Equal(_coordinate.X, envelope.MaxX);
        Assert.Equal(_coordinate.Y, envelope.MinY);
        Assert.Equal(_coordinate.Y, envelope.MaxY);
    }
}
