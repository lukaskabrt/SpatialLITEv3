using SpatialLite.Contracts;
using SpatialLite.Core.Geometries;

namespace SpatialLite.Gpx.Geometries;

/// <summary>
/// Represents a Gpx point
/// </summary>
public class GpxPoint : Point
{
    /// <summary>
    /// Gets or sets time when the point was recorded.
    /// </summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the elevation value in meters above sea level.
    /// </summary>
    public double? Elevation { get; set; }

    /// <summary>
    /// Gets or sets additional information about point.
    /// </summary>
    public GpxPointMetadata? Metadata { get; set; }

    /// <summary>
    /// Creates a new, empty instance of the GpxPoint.
    /// </summary>
    public GpxPoint()
    {
    }

    /// <summary>
    /// Creates a new instance of the GpxPoint with given position.
    /// </summary>
    /// <param name="position">The position of the point.</param>
    public GpxPoint(Coordinate position)
        : base(position)
    {
    }

    /// <summary>
    /// Creates a new instance of the GpxPoint with given position and time.
    /// </summary>
    /// <param name="position">The position of the point.</param>
    /// <param name="time">The time when the point was recorded.</param>
    public GpxPoint(Coordinate position, DateTime time)
        : base(position)
    {
        Timestamp = time;
    }
}
