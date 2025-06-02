namespace SpatialLite.Contracts;

/// <summary>
/// Defines properties and methods for line strings. A line string is a curve with linear connection between consecutive points.
/// </summary>
public interface ILineString : IGeometry
{
    /// <summary>
    /// Gets a value indicating whether the <c>ILineString</c> is closed.
    /// </summary>
    /// <remarks>
    /// The ILineString is closed if <see cref="Start"/> and <see cref="End"/> are identical.
    /// </remarks>
    public bool IsClosed { get; }

    /// <summary>
    /// Gets the first coordinate of the <c>ILineString</c> object.
    /// </summary>
    public Coordinate Start { get; }

    /// <summary>
    /// Gets the last coordinate of the <c>ILineString</c> object.
    /// </summary>
    public Coordinate End { get; }

    /// <summary>
    /// Gets the list of coordinates, that define this LineString.
    /// </summary>
    public IReadOnlyList<Coordinate> Coordinates { get; }
}
