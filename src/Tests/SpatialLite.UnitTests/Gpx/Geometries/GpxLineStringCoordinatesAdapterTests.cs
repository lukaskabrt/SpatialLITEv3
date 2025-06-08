using SpatialLite.Contracts;
using SpatialLite.Gpx.Geometries;

namespace SpatialLite.UnitTests.Gpx.Geometries;

public class GpxLineStringCoordinatesAdapterTests
{
    [Fact]
    public void Count_ReturnsZero_ForEmptyUnderlayingCollection()
    {
        var adapter = new GpxLineStringCoordinatesAdapter([]);

        var count = adapter.Count;

        Assert.Equal(0, count);
    }

    [Fact]
    public void Count_ReturnsCorrectCount_ForNonEmptyUnderlayingCollection()
    {
        var points = new List<GpxPoint>
        {
            new(new Coordinate(1, 2)),
            new(new Coordinate(3, 4)),
            new(new Coordinate(5, 6))
        };

        var adapter = new GpxLineStringCoordinatesAdapter(points);

        var count = adapter.Count;

        Assert.Equal(3, count);
    }

    [Fact]
    public void Indexer_ReturnsCorrectCoordinate_FromUnderlayingCollection()
    {
        var points = new List<GpxPoint>
        {
            new(new Coordinate(1, 2)),
            new(new Coordinate(3, 4)),
            new(new Coordinate(5, 6))
        };

        var adapter = new GpxLineStringCoordinatesAdapter(points);

        var coordinate = adapter[1];

        Assert.Equal(points[1].Position, coordinate);
    }

    [Fact]
    public void Indexer_ThrowsArgumentOutOfRangeException_ForInvalidIndex()
    {
        var points = new List<GpxPoint>
        {
            new(new Coordinate(1, 2)),
            new(new Coordinate(3, 4))
        };

        var adapter = new GpxLineStringCoordinatesAdapter(points);

        Assert.Throws<ArgumentOutOfRangeException>(() => _ = adapter[2]);
    }

    [Fact]
    public void GetEnumerator_ReturnsEnumerator_ThatEnumeratesAllCoordinatesFromUnderlayingCollection()
    {
        var points = new List<GpxPoint>
        {
            new(new Coordinate(1, 2)),
            new(new Coordinate(3, 4)),
            new(new Coordinate(5, 6))
        };

        var adapter = new GpxLineStringCoordinatesAdapter(points);

        var enumerator = adapter.GetEnumerator();

        Assert.True(enumerator.MoveNext());
        Assert.Equal(points[0].Position, enumerator.Current);

        Assert.True(enumerator.MoveNext());
        Assert.Equal(points[1].Position, enumerator.Current);

        Assert.True(enumerator.MoveNext());
        Assert.Equal(points[2].Position, enumerator.Current);

        Assert.False(enumerator.MoveNext());
    }
}
