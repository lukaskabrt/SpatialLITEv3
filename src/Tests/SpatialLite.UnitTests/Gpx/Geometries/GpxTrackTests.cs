using SpatialLite.Contracts;
using SpatialLite.Gpx.Geometries;

namespace SpatialLite.UnitTests.Gpx.Geometries;

public class GpxTrackTests
{
    [Fact]
    public void Constructor_Parameterless_InitializesEmptyTrack()
    {
        var track = new GpxTrack();

        Assert.NotNull(track.Geometries);
        Assert.Empty(track.Geometries);
    }

    [Fact]
    public void Constructor_Segments_InitializesTrackWithSegments()
    {
        var segments = new List<GpxLineString>
        {
            new([new(new Coordinate(1.0, 2.0))]),
            new([new(new Coordinate(3.0, 4.0))])
        };

        var track = new GpxTrack(segments);

        Assert.Equal(segments, track.Geometries);
    }
}
