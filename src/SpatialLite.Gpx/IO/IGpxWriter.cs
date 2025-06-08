using SpatialLite.Gpx.Geometries;

namespace SpatialLite.Gpx.IO;

/// <summary>
/// Defines functions and properties for classes that can writes GPX entities to a destination.
/// </summary>
public interface IGpxWriter
{
    /// <summary>
    /// Writes GpxWaypoint
    /// </summary>
    /// <param name="waypoint">The waypoint to write.</param>
    public void Write(GpxWaypoint waypoint);

    /// <summary>
    /// Writes GpxRoute
    /// </summary>
    /// <param name="route">The route to write</param>
    public void Write(GpxRoute route);

    /// <summary>
    /// Writes GpxTrack
    /// </summary>
    /// <param name="track">The track to write</param>
    public void Write(GpxTrack track);
}
