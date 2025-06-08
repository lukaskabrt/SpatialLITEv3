using SpatialLite.Gpx;
using SpatialLite.Gpx.Geometries;
using SpatialLite.Gpx.IO;
using SpatialLite.UnitTests.Data;
using SpatialLite.UnitTests.Extensions;
using System.Xml.Linq;

namespace SpatialLite.UnitTests.Gpx;

public class GpxDocumentTests
{
    [Fact]
    public void Constructor_CreatesEmptyDocument()
    {
        var target = new GpxDocument();

        Assert.Empty(target.Waypoints);
        Assert.Empty(target.Routes);
        Assert.Empty(target.Tracks);
    }

    [Fact]
    public void Constructor_WaypointsRoutesTracks_CreatesDocumentWithGpxEntities()
    {
        IEnumerable<GpxWaypoint> waypoints = [new GpxWaypoint()];
        IEnumerable<GpxRoute> routes = [new GpxRoute()];
        IEnumerable<GpxTrack> tracks = [new GpxTrack()];

        var target = new GpxDocument(waypoints, routes, tracks);

        Assert.Equal(waypoints, target.Waypoints);
        Assert.Equal(routes, target.Routes);
        Assert.Equal(tracks, target.Tracks);
    }

    [Fact]
    public void Load_IGpxReader_LoadsEntitiesFromReader()
    {
        using var reader = new GpxReader(TestDataReader.Gpx.Open("gpx-real-file.gpx"), new GpxReaderSettings() { ReadMetadata = true });

        var target = GpxDocument.Load(reader);

        Assert.Equal(3, target.Waypoints.Count);
        Assert.Equal(2, target.Routes.Count);
        Assert.Single(target.Tracks);
    }

    [Fact]
    public void Load_string_ThrowsExceptionIfFileDoesNotExists()
    {
        var path = "non-existing-file.gpx";

        Assert.Throws<FileNotFoundException>(() => GpxDocument.Load(path));
    }

    [Fact]
    public void Load_LoadsGpxEntitiesFromFile()
    {
        var path = TestDataReader.Gpx.GetPath("gpx-real-file.gpx");

        var target = GpxDocument.Load(path);

        Assert.Equal(3, target.Waypoints.Count);
        Assert.Equal(2, target.Routes.Count);
        Assert.Single(target.Tracks);
    }

    [Fact]
    public void Save_IGpxWriter_WritesDataToWriter()
    {
        var waypoint = new GpxWaypoint();
        var route = new GpxRoute();
        var track = new GpxTrack();

        var writer = new RecordingGpxWriter();
        var target = new GpxDocument([waypoint], [route], [track]);

        target.Save(writer);

        Assert.Single(writer.Waypoints, w => w.Equals(waypoint));
        Assert.Single(writer.Routes, r => r.Equals(route));
        Assert.Single(writer.Tracks, t => t.Equals(track));
    }

    [Fact]
    public void Save_SavesDataToFile()
    {
        var path = Path.GetTempFileName();
        try
        {
            var target = GpxDocument.Load(TestDataReader.Gpx.GetPath("gpx-real-file.gpx"));
            target.Save(path, new GpxWriterSettings());

            var original = XDocument.Load(TestDataReader.Gpx.GetPath("gpx-real-file.gpx"));
            var saved = XDocument.Load(path);

            Assert.True(XDocumentExtensions.DeepEqualsWithNormalization(original, saved));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private class RecordingGpxWriter : IGpxWriter
    {
        public List<GpxWaypoint> Waypoints { get; } = [];
        public List<GpxRoute> Routes { get; } = [];
        public List<GpxTrack> Tracks { get; } = [];
        public void Write(GpxWaypoint waypoint)
        {
            Waypoints.Add(waypoint);
        }
        public void Write(GpxRoute route)
        {
            Routes.Add(route);
        }
        public void Write(GpxTrack track)
        {
            Tracks.Add(track);
        }
    }
}
