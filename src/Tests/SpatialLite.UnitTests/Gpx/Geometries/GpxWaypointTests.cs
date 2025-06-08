using SpatialLite.Contracts;
using SpatialLite.Gpx.Geometries;

namespace SpatialLite.UnitTests.Gpx.Geometries;

public class GpxWaypointTests
{
    [Fact]
    public void Constructor_Parameterless_InitializesClassWithDefaultValues()
    {
        var point = new GpxWaypoint();

        Assert.Equal(Coordinate.Empty, point.Position);
        Assert.Null(point.Timestamp);
        Assert.Null(point.Elevation);
        Assert.Null(point.Metadata);
    }

    [Fact]
    public void Constructor_WithPosition_SetsPositionCorrectly()
    {
        var position = new Coordinate(1.0, 2.0);

        var point = new GpxWaypoint(position);

        Assert.Equal(position, point.Position);
    }
}
