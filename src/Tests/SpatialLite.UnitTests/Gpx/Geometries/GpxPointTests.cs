using SpatialLite.Contracts;
using SpatialLite.Gpx.Geometries;

namespace SpatialLite.UnitTests.Gpx.Geometries;

public class GpxPointTests
{
    [Fact]
    public void Constructor_Parameterless_InitializesClassWithDefaultValues()
    {
        var point = new GpxPoint();

        Assert.Equal(Coordinate.Empty, point.Position);
        Assert.Null(point.Timestamp);
        Assert.Null(point.Elevation);
        Assert.Null(point.Metadata);
    }

    [Fact]
    public void Constructor_WithPosition_SetsPositionCorrectly()
    {
        var position = new Coordinate(1.0, 2.0);

        var point = new GpxPoint(position);

        Assert.Equal(position, point.Position);
    }

    [Fact]
    public void Constructor_WithPositionAndTime_SetsPositionAndTimestampCorrectly()
    {
        var position = new Coordinate(1.0, 2.0);
        var time = new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc);

        var point = new GpxPoint(position, time);

        Assert.Equal(position, point.Position);
        Assert.Equal(time, point.Timestamp);
    }
}
