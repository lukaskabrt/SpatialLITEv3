using SpatialLite.Contracts;
using SpatialLite.Gpx.Geometries;
using SpatialLite.Gpx.IO;
using SpatialLite.UnitTests.Data;

namespace SpatialLite.UnitTests.Gpx.IO;

public class GpxReaderTests
{
    [Fact]
    public void Constructor_StringSettings_ThrowsExceptionIfFileDoesNotExist()
    {
        Assert.Throws<FileNotFoundException>(() => { new GpxReader("non-existing-file.gpx", new GpxReaderSettings() { ReadMetadata = false }); });
    }

    [Fact]
    public void Constructor_StringSettings_SetsSettings()
    {
        var settings = new GpxReaderSettings() { ReadMetadata = false };
        using var target = new GpxReader(TestDataReader.Gpx.GetPath("gpx-real-file.gpx"), settings);

        Assert.Same(settings, target.Settings);
    }

    [Fact]
    public void Constructor_StreamSettings_SetsSettings()
    {
        var settings = new GpxReaderSettings() { ReadMetadata = false };
        using var stream = TestDataReader.Gpx.Open("gpx-real-file.gpx");

        using var target = new GpxReader(stream, settings);

        Assert.Same(settings, target.Settings);
    }

    [Fact]
    public void Read_ThrowsException_IfVersionIsNot10or11()
    {
        using var stream = TestDataReader.Gpx.Open("gpx-version-2_0.gpx");
        using var target = new GpxReader(stream, new());

        Assert.Throws<InvalidDataException>(target.Read);
    }

    [Fact]
    public void Read_ThrowsException_IfXmlContainsInvalidRootElement()
    {
        using var stream = TestDataReader.Gpx.Open("gpx-invalid-root-element.gpx");
        using var target = new GpxReader(stream, new());

        Assert.Throws<InvalidDataException>(target.Read);
    }

    [Fact]
    public void Read_ThrowsExceptionIfWaypointHasNoLat()
    {
        using var stream = TestDataReader.Gpx.Open("gpx-waypoint-without-lat.gpx");
        using var target = new GpxReader(stream, new());

        Assert.Throws<InvalidDataException>(target.Read);
    }

    [Fact]
    public void Read_ThrowsExceptionIfWaypointHasNoLon()
    {
        using var stream = TestDataReader.Gpx.Open("gpx-waypoint-without-lon.gpx");
        GpxReader target = new(stream, new GpxReaderSettings() { ReadMetadata = false });

        Assert.Throws<InvalidDataException>(target.Read);
    }

    [Fact]
    public void Read_SetsMetadataIfReadMetadataIsTrue()
    {
        using var stream = TestDataReader.Gpx.Open("gpx-waypoint-simple.gpx");
        using var target = new GpxReader(stream, new GpxReaderSettings() { ReadMetadata = true });

        var result = target.Read() as GpxWaypoint;

        Assert.NotNull(result);
        Assert.NotNull(result.Metadata);
    }

    [Fact]
    public void Read_DoesNotSetMetadataIfReadMetadataIsFalse()
    {
        using var stream = TestDataReader.Gpx.Open("gpx-waypoint-simple.gpx");
        using var target = new GpxReader(stream, new GpxReaderSettings() { ReadMetadata = false });

        var result = target.Read() as GpxPoint;

        Assert.NotNull(result);
        Assert.Null(result.Metadata);
    }

    [Fact]
    public void Read_ParsesWaypointWithLatLonElevationAndTime()
    {
        var expectedCoordinate = new Coordinate(-71.119277, 42.438878);
        var expectedElevation = 44.586548;
        var expectedTime = new DateTime(2001, 11, 28, 21, 5, 28, DateTimeKind.Utc);

        using var stream = TestDataReader.Gpx.Open("gpx-waypoint-simple.gpx");
        using var target = new GpxReader(stream, new GpxReaderSettings() { ReadMetadata = false });

        var result = target.Read() as GpxPoint;

        Assert.NotNull(result);
        Assert.Equal(result.Position, expectedCoordinate);
        Assert.Equal(result.Timestamp, expectedTime);
        Assert.Equal(result.Elevation, expectedElevation);
    }

    [Fact]
    public void Read_ParsesWaypointWithExtensions()
    {
        var expectedCoordinate = new Coordinate(-71.119277, 42.438878);
        var expectedElevation = 44.586548;
        var expectedTime = new DateTime(2001, 11, 28, 21, 5, 28, DateTimeKind.Utc);

        using var stream = TestDataReader.Gpx.Open("gpx-waypoint-extensions.gpx");
        using var target = new GpxReader(stream, new GpxReaderSettings() { ReadMetadata = false });

        var result = target.Read() as GpxPoint;

        Assert.NotNull(result);
        Assert.Equal(result.Position, expectedCoordinate);
        Assert.Equal(result.Timestamp, expectedTime);
        Assert.Equal(result.Elevation, expectedElevation);
    }

    [Fact]
    public void Read_ParsesMultipleWaypoints()
    {
        using var stream = TestDataReader.Gpx.Open("gpx-waypoint-multiple.gpx");
        using GpxReader target = new(stream, new GpxReaderSettings() { ReadMetadata = false });

        var count = 0;
        while ((_ = target.Read() as GpxPoint) != null)
        {
            count++;
        }

        Assert.Equal(3, count);
    }

    [Fact]
    public void Read_ReadsWaypointMetadata()
    {
        using var data = TestDataReader.Gpx.Open("gpx-waypoint-with-metadata.gpx");
        using var target = new GpxReader(data, new GpxReaderSettings() { ReadMetadata = true });

        var result = target.Read() as GpxPoint;

        Assert.NotNull(result);
        Assert.NotNull(result.Metadata);
        Assert.Equal(0.98, result.Metadata.MagVar);
        Assert.Equal(12.5, result.Metadata.GeoidHeight);
        Assert.Equal(GpsFix.Fix3D, result.Metadata.Fix);
        Assert.Equal(8, result.Metadata.SatellitesCount);
        Assert.Equal(5.1, result.Metadata.Hdop);
        Assert.Equal(8.1, result.Metadata.Vdop);
        Assert.Equal(10.8, result.Metadata.Pdop);
        Assert.Equal(45, result.Metadata.AgeOfDgpsData);
        Assert.Equal(124, result.Metadata.DgpsId);

        Assert.Equal("WPT Comment", result.Metadata.Comment);
        Assert.Equal("WPT Description", result.Metadata.Description);
        Assert.Equal("WPT Name", result.Metadata.Name);
        Assert.Equal("WPT Source", result.Metadata.Source);

        Assert.NotNull(result.Metadata.Links);
        Assert.Single(result.Metadata.Links);

        var link = result.Metadata.Links.Single();
        Assert.Equal("http://www.topografix.com", link.Url.OriginalString);
        Assert.Equal("Link text", link.Text);
        Assert.Equal("plain/text", link.Type);
    }

    [Fact]
    public void Read_ReadsWaypointUnsortedMetadataAndExtension()
    {
        using var data = TestDataReader.Gpx.Open("gpx-waypoint-with-metadata.gpx");
        using var target = new GpxReader(data, new GpxReaderSettings() { ReadMetadata = true });

        var result = target.Read() as GpxPoint;

        Assert.NotNull(result);
        Assert.NotNull(result.Metadata);
        Assert.Equal(0.98, result.Metadata.MagVar);
        Assert.Equal(12.5, result.Metadata.GeoidHeight);
        Assert.Equal(GpsFix.Fix3D, result.Metadata.Fix);
        Assert.Equal(8, result.Metadata.SatellitesCount);
        Assert.Equal(5.1, result.Metadata.Hdop);
        Assert.Equal(8.1, result.Metadata.Vdop);
        Assert.Equal(10.8, result.Metadata.Pdop);
        Assert.Equal(45, result.Metadata.AgeOfDgpsData);
        Assert.Equal(124, result.Metadata.DgpsId);

        Assert.Equal("WPT Comment", result.Metadata.Comment);
        Assert.Equal("WPT Description", result.Metadata.Description);
        Assert.Equal("WPT Name", result.Metadata.Name);
        Assert.Equal("WPT Source", result.Metadata.Source);

        Assert.NotNull(result.Metadata.Links);
        Assert.Single(result.Metadata.Links);

        var link = result.Metadata.Links.Single();
        Assert.Equal("http://www.topografix.com", link.Url.OriginalString);
        Assert.Equal("Link text", link.Text);
        Assert.Equal("plain/text", link.Type);
    }

    [Fact]
    public void Read_ParsesTrackWithSingleSegment()
    {
        using var data = TestDataReader.Gpx.Open("gpx-track-single-track-segment.gpx");
        using var target = new GpxReader(data, new GpxReaderSettings() { ReadMetadata = false });

        var result = target.Read() as GpxTrack;

        Assert.NotNull(result);
        Assert.Single(result.Geometries);

        var segment = result.Geometries[0];
        Assert.Equal(new Coordinate(-76.638178825, 39.449270368), segment.Points[0].Position);
        Assert.Equal(new DateTime(1970, 1, 1, 7, 10, 23, DateTimeKind.Utc), segment.Points[0].Timestamp);
        Assert.Equal(new Coordinate(-76.638012528, 39.449130893), segment.Points[1].Position);
        Assert.Equal(new DateTime(1970, 1, 1, 7, 10, 28, DateTimeKind.Utc), segment.Points[1].Timestamp);
        Assert.Equal(new Coordinate(-76.637980342, 39.449098706), segment.Points[2].Position);
        Assert.Equal(new DateTime(1970, 1, 1, 7, 10, 33, DateTimeKind.Utc), segment.Points[2].Timestamp);
    }

    [Fact]
    public void Read_ParsesTrackWithSingleSegmentAndExtensions()
    {
        using var data = TestDataReader.Gpx.Open("gpx-track-single-track-segment.gpx");
        using var target = new GpxReader(data, new GpxReaderSettings() { ReadMetadata = false });

        var result = target.Read() as GpxTrack;

        Assert.NotNull(result);
        Assert.Single(result.Geometries);
    }

    [Fact]
    public void Read_ParsesTrackWithMultipleSegments()
    {
        using var data = TestDataReader.Gpx.Open("gpx-track-2-track-segments.gpx");
        using var target = new GpxReader(data, new GpxReaderSettings() { ReadMetadata = false });

        var result = target.Read() as GpxTrack;

        Assert.NotNull(result);

        Assert.Equal(2, result.Geometries.Count);

        Assert.Equal(3, result.Geometries[0].Points.Count);
        Assert.Equal(2, result.Geometries[1].Points.Count);
    }

    [Fact]
    public void Read_ParsesMultipleTracks()
    {
        using var data = TestDataReader.Gpx.Open("gpx-track-multiple-tracks.gpx");
        using var target = new GpxReader(data, new GpxReaderSettings() { ReadMetadata = false });

        var result1 = target.Read() as GpxTrack;
        var result2 = target.Read() as GpxTrack;

        Assert.NotNull(result1);
        Assert.Equal(2, result1.Geometries.Count);
        Assert.Equal(3, result1.Geometries[0].Points.Count);
        Assert.Equal(2, result1.Geometries[1].Points.Count);

        Assert.NotNull(result2);
        Assert.Single(result2.Geometries);
        Assert.Equal(2, result2.Geometries[0].Points.Count);
    }

    [Fact]
    public void Read_ParsesEmptyTrack()
    {
        using var data = TestDataReader.Gpx.Open("gpx-track-empty.gpx");
        using var target = new GpxReader(data, new GpxReaderSettings() { ReadMetadata = false });

        var result = target.Read() as GpxTrack;

        Assert.NotNull(result);
        Assert.Empty(result.Geometries);
    }

    [Fact]
    public void Read_ParsesTrackWithEmptySegment()
    {
        using var data = TestDataReader.Gpx.Open("gpx-track-empty-track-segment.gpx");
        using var target = new GpxReader(data, new GpxReaderSettings() { ReadMetadata = false });

        var result = target.Read() as GpxTrack;

        Assert.NotNull(result);
        Assert.Single(result.Geometries);
        Assert.Empty(result.Geometries[0].Points);
    }

    [Fact]
    public void Read_ParsesTrackMetadata()
    {
        using var data = TestDataReader.Gpx.Open("gpx-track-with-metadata.gpx");
        using var target = new GpxReader(data, new GpxReaderSettings() { ReadMetadata = true });

        var result = target.Read() as GpxTrack;

        Assert.NotNull(result);
        Assert.NotNull(result.Metadata);
        Assert.Equal("TRK Comment", result.Metadata.Comment);
        Assert.Equal("TRK Description", result.Metadata.Description);
        Assert.Equal("TRK Name", result.Metadata.Name);
        Assert.Equal("TRK Source", result.Metadata.Source);
        Assert.Equal("TRK Type", result.Metadata.Type);

        Assert.NotNull(result.Metadata.Links);
        var link = Assert.Single(result.Metadata.Links);
        Assert.Equal("http://www.topografix.com", link.Url.OriginalString);
        Assert.Equal("Link text", link.Text);
        Assert.Equal("plain/text", link.Type);
    }

    [Fact]
    public void Read_SetsTrackMetadataToNullIfReadMetadataIsFalse()
    {
        using var data = TestDataReader.Gpx.Open("gpx-track-with-metadata.gpx");
        using var target = new GpxReader(data, new GpxReaderSettings() { ReadMetadata = false });

        var result = target.Read() as GpxTrack;

        Assert.NotNull(result);
        Assert.Null(result.Metadata);
    }

    [Fact]
    public void Read_ParsesEmptyRoute()
    {
        using var data = TestDataReader.Gpx.Open("gpx-route-empty.gpx");
        using var target = new GpxReader(data, new GpxReaderSettings() { ReadMetadata = false });

        var result = target.Read() as GpxRoute;

        Assert.NotNull(result);
        Assert.Empty(result.Points);
    }

    [Fact]
    public void Read_ParsesSingleRoute()
    {
        using var data = TestDataReader.Gpx.Open("gpx-route-single-route.gpx");
        using var target = new GpxReader(data, new GpxReaderSettings() { ReadMetadata = false });

        var result = target.Read() as GpxRoute;

        Assert.NotNull(result);
        Assert.Equal(new Coordinate(-76.638178825, 39.449270368), result.Points[0].Position);
        Assert.Equal(new DateTime(1970, 1, 1, 7, 10, 23, DateTimeKind.Utc), result.Points[0].Timestamp);
        Assert.Equal(new Coordinate(-76.638012528, 39.449130893), result.Points[1].Position);
        Assert.Equal(new DateTime(1970, 1, 1, 7, 10, 28, DateTimeKind.Utc), result.Points[1].Timestamp);
        Assert.Equal(new Coordinate(-76.637980342, 39.449098706), result.Points[2].Position);
        Assert.Equal(new DateTime(1970, 1, 1, 7, 10, 33, DateTimeKind.Utc), result.Points[2].Timestamp);
    }

    [Fact]
    public void Read_ParsesSingleRouteWithExtensions()
    {
        using var data = TestDataReader.Gpx.Open("gpx-route-with-metadata-and-extensions.gpx");
        using var target = new GpxReader(data, new GpxReaderSettings() { ReadMetadata = false });

        var result = target.Read() as GpxRoute;

        Assert.NotNull(result);
        Assert.Equal(3, result.Points.Count);
    }

    [Fact]
    public void Read_ParsesMultipleRoutes()
    {
        using var data = TestDataReader.Gpx.Open("gpx-route-multiple-routes.gpx");
        using var target = new GpxReader(data, new GpxReaderSettings() { ReadMetadata = false });

        var result1 = target.Read() as GpxRoute;
        var result2 = target.Read() as GpxRoute;

        Assert.NotNull(result1);
        Assert.Equal(3, result1.Points.Count);

        Assert.NotNull(result2);
        Assert.Equal(2, result2.Points.Count);
    }

    [Fact]
    public void Read_ParsesRouteWithMetadata()
    {
        using var data = TestDataReader.Gpx.Open("gpx-route-with-metadata.gpx");
        using GpxReader target = new(data, new GpxReaderSettings() { ReadMetadata = true });

        var result = target.Read() as GpxRoute;

        Assert.NotNull(result);
        Assert.NotNull(result.Metadata);
        Assert.Equal("RTE Comment", result.Metadata.Comment);
        Assert.Equal("RTE Description", result.Metadata.Description);
        Assert.Equal("RTE Name", result.Metadata.Name);
        Assert.Equal("RTE Source", result.Metadata.Source);
        Assert.Equal("RTE Type", result.Metadata.Type);

        Assert.NotNull(result.Metadata.Links);
        var link = Assert.Single(result.Metadata.Links);

        Assert.Equal("http://www.topografix.com", link.Url.OriginalString);
        Assert.Equal("Link text", link.Text);
        Assert.Equal("plain/text", link.Type);
    }

    [Fact]
    public void Read_SetsRouteMetadataToNullIfReadMetadataIsFalse()
    {
        using var data = TestDataReader.Gpx.Open("gpx-route-with-metadata.gpx");
        using var target = new GpxReader(data, new GpxReaderSettings() { ReadMetadata = false });

        var result = target.Read() as GpxRoute;

        Assert.NotNull(result);
        Assert.Null(result.Metadata);
    }

    [Fact]
    public void Read_ReadsAllEntitiesFromRealGpxFile()
    {
        using var data = TestDataReader.Gpx.Open("gpx-real-file.gpx");
        using var target = new GpxReader(data, new GpxReaderSettings() { ReadMetadata = true });

        var parsed = new List<IGpxGeometry>();

        IGpxGeometry? geometry = null;
        while ((geometry = target.Read()) != null)
        {
            parsed.Add(geometry);
        }

        Assert.Equal(3, parsed.Count(g => g.GeometryType == GpxGeometryType.Waypoint));
        Assert.Equal(2, parsed.Count(g => g.GeometryType == GpxGeometryType.Route));
        Assert.Single(parsed.Where(g => g.GeometryType == GpxGeometryType.Track));
    }
}
