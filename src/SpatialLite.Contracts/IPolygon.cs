namespace SpatialLite.Contracts;

/// <summary>
/// Defines properties and methods for polygons.
/// </summary>
public interface IPolygon : IGeometry
{
    /// <summary>
    /// Gets the exterior boundary of the polygon
    /// </summary>
    public IReadOnlyList<Coordinate> ExteriorRing { get; }

    /// <summary>
    /// Gets a collection of interior boundaries that define holes in the polygon
    /// </summary>
    public IEnumerable<IReadOnlyList<Coordinate>> InteriorRings { get; }
}
