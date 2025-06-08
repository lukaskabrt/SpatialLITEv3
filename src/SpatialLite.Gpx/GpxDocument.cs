using SpatialLite.Gpx.Geometries;
using SpatialLite.Gpx.IO;

namespace SpatialLite.Gpx;

/// <summary>
/// Represents an in-memory GPX document with it's waypoints, routes and tracks 
/// </summary>
public class GpxDocument
{
    /// <summary>
    /// Initializes a new instance of the GpxDocument class that is empty
    /// </summary>
    public GpxDocument()
    {
        Waypoints = [];
        Routes = [];
        Tracks = [];
    }

    /// <summary>
    /// Initializes a new instance of the GpxDocument class with given GPX entities
    /// </summary>
    /// <param name="waypoints">A collection of waypoints to add to the document.</param>
    /// <param name="routes">A collection of routes to add to the document.</param>
    /// <param name="tracks">A collection of tracks to add to the document.</param>
    public GpxDocument(List<GpxWaypoint> waypoints, List<GpxRoute> routes, List<GpxTrack> tracks)
    {
        Waypoints = waypoints;
        Routes = routes;
        Tracks = tracks;
    }

    /// <summary>
    /// Gets collection of waypoints from the document.
    /// </summary>
    public List<GpxWaypoint> Waypoints { get; set; }

    /// <summary>
    /// Gets collection of routes from the document.
    /// </summary>
    public List<GpxRoute> Routes { get; set; }

    /// <summary>
    /// Gets collection of tracks from the document.
    /// </summary>
    public List<GpxTrack> Tracks { get; set; }

    /// <summary>
    /// Saves content of the GpxDocument to file.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <param name="settings">Settings for the Gpx writer.</param>
    public void Save(string path, GpxWriterSettings settings)
    {
        using var writer = new GpxWriter(path, settings);

        Save(writer);
    }

    /// <summary>
    /// Saves content of the GpxDocument using supplied writer.
    /// </summary>
    /// <param name="writer">GpxWriter to be used</param>
    public void Save(IGpxWriter writer)
    {
        if (writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }

        foreach (var waypoint in Waypoints)
        {
            writer.Write(waypoint);
        }

        foreach (var route in Routes)
        {
            writer.Write(route);
        }

        foreach (var track in Tracks)
        {
            writer.Write(track);
        }
    }

    /// <summary>
    /// Loads Gpx data from reader to this instance of the GpxDocument class
    /// </summary>
    /// <param name="reader">The reader to read data from</param>
    private void LoadFromReader(IGpxReader reader)
    {
        IGpxGeometry? geometry;
        while ((geometry = reader.Read()) != null)
        {
            switch (geometry.GeometryType)
            {
                case GpxGeometryType.Waypoint:
                    Waypoints.Add((GpxWaypoint)geometry);
                    break;
                case GpxGeometryType.Route:
                    Routes.Add((GpxRoute)geometry);
                    break;
                case GpxGeometryType.Track:
                    Tracks.Add((GpxTrack)geometry);
                    break;
            }
        }
    }

    /// <summary>
    /// Loads Gpx data from a file.
    /// </summary>
    /// <param name="path">Path to the GPX file.</param>
    /// <returns>GpxDocument instance with data from GPX file</returns>
    public static GpxDocument Load(string path)
    {
        GpxDocument result = new GpxDocument();

        using (var reader = new GpxReader(path, new GpxReaderSettings() { ReadMetadata = true }))
        {
            result.LoadFromReader(reader);
        }

        return result;
    }

    /// <summary>
    /// Loads Gpx data from IGpxReader
    /// </summary>
    /// <param name="reader">The reader to read data from</param>
    /// <returns>GpxDocument instance with data from GpxReader</returns>
    public static GpxDocument Load(IGpxReader reader)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        GpxDocument result = new GpxDocument();
        result.LoadFromReader(reader);
        return result;
    }
}
