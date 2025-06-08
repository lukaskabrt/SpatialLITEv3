using SpatialLite.Contracts;
using SpatialLite.Gpx.Geometries;
using SpatialLite.Gpx.IO;
using SpatialLite.UnitTests.Data;
using SpatialLite.UnitTests.Extensions;
using System.Xml.Linq;

namespace SpatialLite.UnitTests.Gpx.IO;

public class GpxWriterTests
{
    [Fact]
    public void Constructor_StreamSettings_SetsSettings()
    {
        var settings = new GpxWriterSettings();
        using var stream = new MemoryStream();
        using var target = new GpxWriter(stream, settings);

        Assert.Same(settings, target.Settings);
    }

    [Fact]
    public void Constructor_PathSettings_SetsSettings()
    {
        var path = Path.GetTempFileName();

        try
        {
            var settings = new GpxWriterSettings();
            using var target = new GpxWriter(path, settings);

            Assert.Same(settings, target.Settings);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Constructor_PathSettings_CreatesOutputFile()
    {
        var filename = Path.GetTempFileName();

        try
        {
            var settings = new GpxWriterSettings();
            using var target = new GpxWriter(filename, settings);

            Assert.True(File.Exists(filename));
        }
        finally
        {
            File.Delete(filename);
        }
    }

    [Fact]
    public void Write_WritesWaypointWithoutMetadataIfMetadataIsNull()
    {
        var waypoint = new GpxWaypoint(new Coordinate(-71.119277, 42.438878))
        {
            Elevation = 44.586548,
            Timestamp = new DateTime(2001, 11, 28, 21, 05, 28, DateTimeKind.Utc)
        };

        using var stream = new MemoryStream();
        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = false }))
        {
            target.Write(waypoint);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-waypoint-simple.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesWaypointWithoutMetadata_IfWriteMetadataIsFalse()
    {
        var waypointWithMetadata = new GpxWaypoint(new Coordinate(-71.119277, 42.438878))
        {
            Elevation = 44.586548,
            Timestamp = new DateTime(2001, 11, 28, 21, 05, 28, DateTimeKind.Utc),
            Metadata = new GpxPointMetadata
            {
                Name = "WPT Name"
            }
        };

        using var stream = new MemoryStream();
        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = false }))
        {
            target.Write(waypointWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-waypoint-simple.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesWaypointWithMetadata()
    {
        var waypointWithMetadata = new GpxWaypoint(new Coordinate(-71.119277, 42.438878))
        {
            Elevation = 44.586548,
            Timestamp = new DateTime(2001, 11, 28, 21, 05, 28, DateTimeKind.Utc),
            Metadata = new GpxPointMetadata
            {
                AgeOfDgpsData = 45,
                DgpsId = 124,
                Fix = GpsFix.Fix3D,
                GeoidHeight = 12.5,
                Hdop = 5.1,
                MagVar = 0.98,
                Pdop = 10.8,
                SatellitesCount = 8,
                Symbol = "WPT Symbol",
                Vdop = 8.1,
                Comment = "WPT Comment",
                Description = "WPT Description",
                Name = "WPT Name",
                Source = "WPT Source",
                Links = [new GpxLink(new Uri("http://www.topografix.com")) { Text = "Link text", Type = "plain/text" }]
            }
        };

        using var stream = new MemoryStream();
        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = true }))
        {
            target.Write(waypointWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-waypoint-with-metadata.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesWaypointWithoutUnnecessaryElements()
    {
        var waypointWithMetadata = new GpxWaypoint(new Coordinate(-71.119277, 42.438878))
        {
            Elevation = 44.586548,
            Timestamp = new DateTime(2001, 11, 28, 21, 05, 28, DateTimeKind.Utc),
            Metadata = new GpxPointMetadata
            {
                AgeOfDgpsData = 45,
                DgpsId = 124,
                Fix = GpsFix.Fix3D,
                GeoidHeight = 12.5,
                Hdop = 5.1,
                MagVar = 0.98,
                Pdop = 10.8,
                SatellitesCount = null, // purposely null
                Symbol = "WPT Symbol",
                Vdop = 8.1,
                Comment = "WPT Comment",
                Description = "WPT Description",
                Name = null, // purposely null
                Source = "WPT Source",
                Links = [new GpxLink(new Uri("http://www.topografix.com")) { Text = "Link text", Type = "plain/text" }]
            }
        };

        using var stream = new MemoryStream();
        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = true }))
        {
            target.Write(waypointWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-waypoint-with-metadata-selection.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesRouteWith3Points()
    {
        var route = new GpxRoute([
            new GpxPoint(new Coordinate(-76.638178825, 39.449270368), new DateTime(1970, 1, 1, 7, 10, 23, DateTimeKind.Utc)),
            new GpxPoint(new Coordinate(-76.638012528, 39.449130893), new DateTime(1970, 1, 1, 7, 10, 28, DateTimeKind.Utc)),
            new GpxPoint(new Coordinate(-76.637980342, 39.449098706), new DateTime(1970, 1, 1, 7, 10, 33, DateTimeKind.Utc))
        ]);

        using var stream = new MemoryStream();
        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = false }))
        {
            target.Write(route);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-route-single-route.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesRouteWithMetadata()
    {
        var routeWithMetadata = new GpxRoute([
            new GpxPoint(new Coordinate(-76.638178825, 39.449270368), new DateTime(1970, 1, 1, 7, 10, 23, DateTimeKind.Utc)),
            new GpxPoint(new Coordinate(-76.638012528, 39.449130893), new DateTime(1970, 1, 1, 7, 10, 28, DateTimeKind.Utc)),
            new GpxPoint(new Coordinate(-76.637980342, 39.449098706), new DateTime(1970, 1, 1, 7, 10, 33, DateTimeKind.Utc))
        ])
        {
            Metadata = new GpxTrackMetadata
            {
                Comment = "RTE Comment",
                Description = "RTE Description",
                Name = "RTE Name",
                Source = "RTE Source",
                Type = "RTE Type",
                Links = new List<GpxLink> { new GpxLink(new Uri("http://www.topografix.com")) { Text = "Link text", Type = "plain/text" } }
            }
        };

        using var stream = new MemoryStream();
        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = true }))
        {
            target.Write(routeWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-route-with-metadata.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesRouteWithoutMetadata_IfWriteMetadataIsFalse()
    {
        var routeWithMetadata = new GpxRoute([
            new GpxPoint(new Coordinate(-76.638178825, 39.449270368), new DateTime(1970, 1, 1, 7, 10, 23, DateTimeKind.Utc)),
            new GpxPoint(new Coordinate(-76.638012528, 39.449130893), new DateTime(1970, 1, 1, 7, 10, 28, DateTimeKind.Utc)),
            new GpxPoint(new Coordinate(-76.637980342, 39.449098706), new DateTime(1970, 1, 1, 7, 10, 33, DateTimeKind.Utc))
        ])
        {
            Metadata = new GpxTrackMetadata
            {
                Name = "RTE Name",
            }
        };

        using var stream = new MemoryStream();
        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = false }))
        {
            target.Write(routeWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-route-single-route.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesRouteWithoutUnnecessaryElements()
    {
        var routeWithMetadata = new GpxRoute([
            new GpxPoint(new Coordinate(-76.638178825, 39.449270368), new DateTime(1970, 1, 1, 7, 10, 23, DateTimeKind.Utc)),
            new GpxPoint(new Coordinate(-76.638012528, 39.449130893), new DateTime(1970, 1, 1, 7, 10, 28, DateTimeKind.Utc)),
            new GpxPoint(new Coordinate(-76.637980342, 39.449098706), new DateTime(1970, 1, 1, 7, 10, 33, DateTimeKind.Utc))
        ])
        {
            Metadata = new GpxTrackMetadata
            {
                Comment = "RTE Comment",
                Description = "RTE Description",
                Name = "RTE Name",
                Source = null, // purposely null
                Type = "RTE Type",
                Links = new List<GpxLink> { new GpxLink(new Uri("http://www.topografix.com")) { Text = "Link text", Type = "plain/text" } }
            }
        };

        using var stream = new MemoryStream();
        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = true }))
        {
            target.Write(routeWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-route-with-metadata-selection.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesTrack()
    {
        var segment = new GpxLineString([
            new GpxPoint(new Coordinate(-76.638178825, 39.449270368), new DateTime(1970, 1, 1, 7, 10, 23, DateTimeKind.Utc)),
            new GpxPoint(new Coordinate(-76.638012528, 39.449130893), new DateTime(1970, 1, 1, 7, 10, 28, DateTimeKind.Utc)),
            new GpxPoint(new Coordinate(-76.637980342, 39.449098706), new DateTime(1970, 1, 1, 7, 10, 33, DateTimeKind.Utc))
        ]);
        var track = new GpxTrack([segment]);

        using var stream = new MemoryStream();
        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = false }))
        {
            target.Write(track);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-track-single-track-segment.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_WritesTrackWithMetadata()
    {
        var segment = new GpxLineString([
            new GpxPoint(new Coordinate(-76.638178825, 39.449270368), new DateTime(1970, 1, 1, 7, 10, 23, DateTimeKind.Utc)),
            new GpxPoint(new Coordinate(-76.638012528, 39.449130893), new DateTime(1970, 1, 1, 7, 10, 28, DateTimeKind.Utc)),
            new GpxPoint(new Coordinate(-76.637980342, 39.449098706), new DateTime(1970, 1, 1, 7, 10, 33, DateTimeKind.Utc))
        ]);
        var trackWithMetadata = new GpxTrack(new List<GpxLineString> { segment })
        {
            Metadata = new GpxTrackMetadata
            {
                Comment = "TRK Comment",
                Description = "TRK Description",
                Name = "TRK Name",
                Source = "TRK Source",
                Type = "TRK Type",
                Links = new List<GpxLink> { new GpxLink(new Uri("http://www.topografix.com")) { Text = "Link text", Type = "plain/text" } }
            }
        };

        using var stream = new MemoryStream();
        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = true }))
        {
            target.Write(trackWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-track-with-metadata.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_DoesNotWriteTrackMetadata_IfWriteMetadataIsFalse()
    {
        var segment = new GpxLineString([
            new GpxPoint(new Coordinate(-76.638178825, 39.449270368), new DateTime(1970, 1, 1, 7, 10, 23, DateTimeKind.Utc)),
            new GpxPoint(new Coordinate(-76.638012528, 39.449130893), new DateTime(1970, 1, 1, 7, 10, 28, DateTimeKind.Utc)),
            new GpxPoint(new Coordinate(-76.637980342, 39.449098706), new DateTime(1970, 1, 1, 7, 10, 33, DateTimeKind.Utc))
        ]);
        var trackWithMetadata = new GpxTrack(new List<GpxLineString> { segment })
        {
            Metadata = new GpxTrackMetadata
            {
                Name = "TRK Name"
            }
        };

        using var stream = new MemoryStream();
        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = false }))
        {
            target.Write(trackWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-track-single-track-segment.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }

    [Fact]
    public void Write_TrackWithEntityDetailsButNullValues_WritesTrackWithoutUnnecessaryElements()
    {
        var segment = new GpxLineString([
            new GpxPoint(new Coordinate(-76.638178825, 39.449270368), new DateTime(1970, 1, 1, 7, 10, 23, DateTimeKind.Utc)),
            new GpxPoint(new Coordinate(-76.638012528, 39.449130893), new DateTime(1970, 1, 1, 7, 10, 28, DateTimeKind.Utc)),
            new GpxPoint(new Coordinate(-76.637980342, 39.449098706), new DateTime(1970, 1, 1, 7, 10, 33, DateTimeKind.Utc))
        ]);
        var trackWithMetadata = new GpxTrack(new List<GpxLineString> { segment })
        {
            Metadata = new GpxTrackMetadata
            {
                Comment = "TRK Comment",
                Description = "TRK Description",
                Name = "TRK Name",
                Source = null, // purposely null
                Type = "TRK Type",
                Links = new List<GpxLink> { new GpxLink(new Uri("http://www.topografix.com")) { Text = "Link text", Type = "plain/text" } }
            }
        };

        using var stream = new MemoryStream();
        using (var target = new GpxWriter(stream, new GpxWriterSettings() { WriteMetadata = true }))
        {
            target.Write(trackWithMetadata);
        }

        var written = XDocument.Load(new MemoryStream(stream.ToArray()));
        var expected = XDocument.Load(TestDataReader.Gpx.Open("gpx-track-with-metadata-selection.gpx"));

        Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(written, expected));
    }
}
