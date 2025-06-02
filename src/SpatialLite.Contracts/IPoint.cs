namespace SpatialLite.Contracts;

/// <summary>
/// Defines properties and methods for points.
/// </summary>
public interface IPoint : IGeometry
{
    /// <summary>
    /// Gets position of the point.
    /// </summary>
    public Coordinate Position { get; }
}
