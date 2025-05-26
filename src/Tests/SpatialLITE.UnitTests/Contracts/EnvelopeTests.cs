using SpatialLITE.Contracts;

namespace SpatialLITE.UnitTests.Contracts;

public class EnvelopeTests
{
    private void AssertBoundaries(Envelope target, double minX, double maxX, double minY, double maxY)
    {
        Assert.Equal(minX, target.MinX);
        Assert.Equal(maxX, target.MaxX);
        Assert.Equal(minY, target.MinY);
        Assert.Equal(maxY, target.MaxY);
    }

    [Fact]
    public void Constructor__InitializesBoundsToNaNValues()
    {
        var target = new Envelope();

        AssertBoundaries(target, double.NaN, double.NaN, double.NaN, double.NaN);
    }

    [Fact]
    public void Constructor_Coordinate_SetsMinMaxValues()
    {
        var coordinate = new Coordinate(1, 10);

        var target = new Envelope(coordinate);

        AssertBoundaries(target, coordinate.X, coordinate.X, coordinate.Y, coordinate.Y);
    }

    [Fact]
    public void Constructor_IEnumerableCoordinate_SetsMinMaxValues()
    {
        var coordinates = new Coordinate[] { new(1, 10), new(-1, -10) };

        var target = new Envelope(coordinates);

        AssertBoundaries(target, -1, 1, -10, 10);
    }

    [Fact]
    public void Extend_Coordinate_SetsMinMaxValuesOnEmptyEnvelope()
    {
        var target = new Envelope();
        var coordinate = new Coordinate(1, 10);

        var result = target.Extend(coordinate);

        AssertBoundaries(result, coordinate.X, coordinate.X, coordinate.Y, coordinate.Y);
    }

    [Fact]
    public void Extend_Coordinate_DoNothingIfEnvelopeIsEmptyAndCoordinateIsEmpty()
    {
        var target = new Envelope();

        var result = target.Extend(Coordinate.Empty);

        AssertBoundaries(result, double.NaN, double.NaN, double.NaN, double.NaN);
    }

    [Fact]
    public void Extend_Coordinate_DoNothingIfCoordinateIsEmpty()
    {
        var coordinates = new Coordinate[] { new(1, 10), new(-1, -10) };
        var target = new Envelope(coordinates);

        var result = target.Extend(Coordinate.Empty);

        AssertBoundaries(result, -1, 1, -10, 10);
    }

    [Fact]
    public void Extend_Coordinate_ExtendsEnvelopeToLowerValues()
    {
        var coordinates = new Coordinate[] { new(1, 10), new(-1, -10) };
        var target = new Envelope(coordinates);

        var result = target.Extend(new Coordinate(-2, -20));

        AssertBoundaries(result, -2, 1, -20, 10);
    }

    [Fact]
    public void Extend_Coordinate_ExtendsEnvelopeToHigherValues()
    {
        var coordinates = new Coordinate[] { new(1, 10), new(-1, -10) };
        var target = new Envelope(coordinates);

        var result = target.Extend(new Coordinate(2, 20));

        AssertBoundaries(result, -1, 2, -10, 20);
    }

    [Fact]
    public void Extend_Coordinate_DoNothingForCoordinateInsideEnvelope()
    {
        var coordinates = new Coordinate[] { new(1, 10), new(-1, -10) };
        var target = new Envelope(coordinates);

        var result = target.Extend(new Coordinate(0, 0));

        AssertBoundaries(result, -1, 1, -10, 10);
    }

    [Fact]
    public void Extend_IEnumerableCoordinate_SetsMinMaxValuesOnEmptyEnvelope()
    {
        var target = new Envelope();

        var coordinates = new Coordinate[] { new(1, 10), new(-1, -10) };
        var result = target.Extend(coordinates);

        AssertBoundaries(result, -1, 1, -10, 10);
    }

    [Fact]
    public void Extend_IEnumerableCoordinate_DoNothingForEmptyCollection()
    {
        var coordinates = new Coordinate[] { new(1, 10), new(-1, -10) };
        var target = new Envelope(coordinates);

        var result = target.Extend(Array.Empty<Coordinate>());

        AssertBoundaries(result, -1, 1, -10, 10);
    }

    [Fact]
    public void Extend_IEnumerableCoordinate_ExtendsEnvelope()
    {
        var coordinates = new Coordinate[] { new(1, 10), new(-1, -10) };
        var target = new Envelope(coordinates);

        var result = target.Extend(new[] { new Coordinate(-2, -20), new Coordinate(2, 20) });

        AssertBoundaries(result, -2, 2, -20, 20);
    }

    [Fact]
    public void Extend_Envelope_SetsMinMaxValuesOnEmptyEnvelope()
    {
        var target = new Envelope();

        var coordinates = new Coordinate[] { new(1, 10), new(-1, -10) };
        var result = target.Extend(new Envelope(coordinates));

        AssertBoundaries(result, -1, 1, -10, 10);
    }

    [Fact]
    public void Extend_Envelope_DoNothingIfEnvelopeIsInsideTargetEnvelope()
    {
        var coordinates = new Coordinate[] { new(1, 10), new(-1, -10) };
        var target = new Envelope(coordinates);

        var result = target.Extend(new Envelope(new[] { new Coordinate(0.5, 5), new Coordinate(-0.5, -5) }));

        AssertBoundaries(result, -1, 1, -10, 10);
    }

    [Fact]
    public void Extend_Envelope_ExtendsEnvelope()
    {
        var coordinates = new Coordinate[] { new(1, 10), new(-1, -10) };
        var target = new Envelope(coordinates);

        var result = target.Extend(new Envelope(new[] { new Coordinate(-2, -20), new Coordinate(2, 20) }));

        AssertBoundaries(result, -2, 2, -20, 20);
    }
}
