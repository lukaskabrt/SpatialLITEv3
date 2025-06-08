using SpatialLite.Contracts; // Required for Coordinate
using SpatialLite.Gpx.Geometries;
using SpatialLite.Gpx.IO;
using SpatialLite.UnitTests.Data;
using System.Xml.Linq;

namespace SpatialLite.UnitTests.Gpx.IO;

public class GpxWriterTests
{
    private readonly GpxWaypoint _waypoint = new(new Coordinate(-71.119277, 42.438878)) { Elevation = 44.586548, Timestamp = new DateTime(2001, 11, 28, 21, 05, 28, DateTimeKind.Utc) };
    private readonly GpxWaypoint _waypointWithMetadata = new(new Coordinate(-71.119277, 42.438878)) { Elevation = 44.586548, Timestamp = new DateTime(2001, 11, 28, 21, 05, 28, DateTimeKind.Utc) };
    private readonly GpxPointMetadata _pointMetadata;
    private readonly GpxRoute _route = new(new List<GpxPoint> {
        new(new Coordinate(-76.638178825, 39.449270368), new DateTime(1970, 1, 1, 7, 10, 23, DateTimeKind.Utc)),
        new(new Coordinate(-76.638012528, 39.449130893), new DateTime(1970, 1, 1, 7, 10, 28, DateTimeKind.Utc)),
        new(new Coordinate(-76.637980342, 39.449098706), new DateTime(1970, 1, 1, 7, 10, 33, DateTimeKind.Utc))
    });
    private readonly GpxRoute _routeWithMetadata = new(new List<GpxPoint> {
        new(new Coordinate(-76.638178825, 39.449270368), new DateTime(1970, 1, 1, 7, 10, 23, DateTimeKind.Utc)),
        new(new Coordinate(-76.638012528, 39.449130893), new DateTime(1970, 1, 1, 7, 10, 28, DateTimeKind.Utc)),
        new(new Coordinate(-76.637980342, 39.449098706), new DateTime(1970, 1, 1, 7, 10, 33, DateTimeKind.Utc))
    });
    private readonly GpxTrackMetadata _routeMetadata;
    private readonly GpxLineString _segment = new(new List<GpxPoint> {
        new(new Coordinate(-76.638178825, 39.449270368), new DateTime(1970, 1, 1, 7, 10, 23, DateTimeKind.Utc)),
        new(new Coordinate(-76.638012528, 39.449130893), new DateTime(1970, 1, 1, 7, 10, 28, DateTimeKind.Utc)),
        new(new Coordinate(-76.637980342, 39.449098706), new DateTime(1970, 1, 1, 7, 10, 33, DateTimeKind.Utc))
    });
    private readonly GpxTrackMetadata _trackMetadata;
    private readonly GpxTrack _track;
    private readonly GpxTrack _trackWithMetadata;

    public GpxWriterTests()
    {
        _pointMetadata = new GpxPointMetadata();
        _pointMetadata.AgeOfDgpsData = 45;
        _pointMetadata.DgpsId = 124;
        _pointMetadata.Fix = GpsFix.Fix3D;
        _pointMetadata.GeoidHeight = 12.5;
        _pointMetadata.Hdop = 5.1;
        _pointMetadata.MagVar = 0.98;
        _pointMetadata.Pdop = 10.8;
        _pointMetadata.SatellitesCount = 8;
        _pointMetadata.Symbol = "WPT Symbol";
        _pointMetadata.Vdop = 8.1;

        _pointMetadata.Comment = "WPT Comment";
        _pointMetadata.Description = "WPT Description";
        _pointMetadata.Name = "WPT Name";
        _pointMetadata.Source = "WPT Source";
        _pointMetadata.Links = new List<GpxLink>();
        _pointMetadata.Links.Add(new GpxLink(new Uri("http://www.topografix.com")) { Text = "Link text", Type = "plain/text" });
        _waypointWithMetadata.Metadata = _pointMetadata;

        _routeMetadata = new GpxTrackMetadata();
        _routeMetadata.Comment = "RTE Comment";
        _routeMetadata.Description = "RTE Description";
        _routeMetadata.Name = "RTE Name";
        _routeMetadata.Source = "RTE Source";
        _routeMetadata.Type = "RTE Type";
        _routeMetadata.Links = new List<GpxLink>();
        _routeMetadata.Links.Add(new GpxLink(new Uri("http://www.topografix.com")) { Text = "Link text", Type = "plain/text" });
        _routeWithMetadata.Metadata = _routeMetadata;

        _trackMetadata = new GpxTrackMetadata();
        _trackMetadata.Comment = "TRK Comment";
        _trackMetadata.Description = "TRK Description";
        _trackMetadata.Name = "TRK Name";
        _trackMetadata.Source = "TRK Source";
        _trackMetadata.Type = "TRK Type";
        _trackMetadata.Links = new List<GpxLink>();
        _trackMetadata.Links.Add(new GpxLink(new Uri("http://www.topografix.com")) { Text = "Link text", Type = "plain/text" });

        _track = new GpxTrack(new List<GpxLineString> { _segment });
        _trackWithMetadata = new GpxTrack(new List<GpxLineString> { _segment });
        _trackWithMetadata.Metadata = _trackMetadata;
    }

    [Fact]
    public void Constructor_StreamSettings_SetsSettings()
    {
        var settings = new GpxWriterSettings();
        var stream = new MemoryStream();
        var target = new GpxWriter(stream, settings);

        Assert.Same(settings, target.Settings);
    }

    [Fact]
    public void Constructor_StreamSettings_CreatesGpxFileWithRootElement()
    {
        var generatorName = "SpatialLite";
        var stream = new MemoryStream();

        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = false, GeneratorName = generatorName }))
        {
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-empty-file.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Constructor_PathSettings_SetsSettings()
    {
        var path = Path.GetTempFileName();
        var settings = new GpxWriterSettings();
        var target = new GpxWriter(path, settings);
        Assert.Same(settings, target.Settings);
    }

    [Fact]
    public void Constructor_PathSettings_CreatesOutputFile()
    {
        var filename = Path.GetTempFileName();
        var settings = new GpxWriterSettings();
        new GpxWriter(filename, settings);

        Assert.True(File.Exists(filename));
    }

    [Fact]
    public void Constructor_PathSettings_CreatesGpxFileWithRootElement()
    {
        var path = Path.GetTempFileName();
        var generatorName = "SpatialLite";

        using (var target = new GpxWriter(path, new GpxWriterSettings() { WriteMetadata = false, GeneratorName = generatorName }))
        {
        }

        var written = XDocument.Load(path);
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-empty-file.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesWaypointWithoutMetadataIfMetadataIsNull()
    {
        var stream = new MemoryStream();

        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_waypoint);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-waypoint-simple.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesWaypointWithoutMetadataIfWriteMetadataIsFalse()
    {
        var stream = new MemoryStream();

        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_waypointWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-waypoint-simple.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesWaypointWithMetadata()
    {
        var stream = new MemoryStream();

        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = true }))
        {
            target.Write(_waypointWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-waypoint-with-metadata.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesWaypointWithoutUnnecessaryElements()
    {
        if (_waypointWithMetadata.Metadata != null)
        {
            _waypointWithMetadata.Metadata.SatellitesCount = null;
            _waypointWithMetadata.Metadata.Name = null;
        }

        var stream = new MemoryStream();

        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = true }))
        {
            target.Write(_waypointWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-waypoint-with-metadata-selection.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesRouteWith3Points()
    {
        var stream = new MemoryStream();

        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_route);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-route-single-route.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesRouteWithMetadata()
    {
        var stream = new MemoryStream();

        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = true }))
        {
            target.Write(_routeWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-route-with-metadata.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesRouteWithoutMetadataIfWriteMetadataIsFalse()
    {
        var stream = new MemoryStream();

        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_routeWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-route-single-route.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesRouteWithoutUnnecessaryElements()
    {
        if (_routeWithMetadata.Metadata != null)
        {
            _routeWithMetadata.Metadata.Source = null;
        }

        var stream = new MemoryStream();

        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = true }))
        {
            target.Write(_routeWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-route-with-metadata-selection.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesTrack()
    {
        var stream = new MemoryStream();

        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_track);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-track-single-track-segment.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesTrackWithMetadata()
    {
        var stream = new MemoryStream();

        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = true }))
        {
            target.Write(_trackWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-track-with-metadata.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_DoesntWriteTrackMetadataIfWriteMetadataIsFalse()
    {
        var stream = new MemoryStream();

        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_trackWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-track-single-track-segment.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_TrackWithEntityDetailsButNullValues_WritesTrackWithoutUnnecessaryElements()
    {
        if (_trackWithMetadata.Metadata != null)
        {
            _trackWithMetadata.Metadata.Source = null;
        }

        var stream = new MemoryStream();

        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = true }))
        {
            target.Write(_trackWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-track-with-metadata-selection.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Dispose_ClosesOutputStreamIfWritingToFiles()
    {
        var path = Path.GetTempFileName();

        var target = new GpxWriter(path, new GpxWriterSettings());
        target.Dispose();

        using var _ = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
    }

    [Fact]
    public void Dispose_ClosesOutputStreamIfWritingToStream()
    {
        var stream = new MemoryStream();

        var target = new GpxWriter(stream, new GpxWriterSettings());
        target.Dispose();

        Assert.False(stream.CanRead);
    }
}
