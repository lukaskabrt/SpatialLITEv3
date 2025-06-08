using SpatialLite.Contracts;

namespace SpatialLite.Gpx.Geometries;

public class GpxWaypoint : GpxPoint, IGpxGeometry
{
    /// <summary>
    /// Creates a new, empty instance of the GpxWaypoint.
    /// </summary>
    public GpxWaypoint()
        : base()
    {
    }
    /// <summary>
    /// Creates a new instance of the GpxWaypoint with given position.
    /// </summary>
    /// <param name="position">The position of the waypoint.</param>
    public GpxWaypoint(Coordinate position)
        : base(position)
    {
    }

    /// <summary>
    /// Get the type of geometry
    /// </summary>
    public GpxGeometryType GeometryType => GpxGeometryType.Waypoint;
}
