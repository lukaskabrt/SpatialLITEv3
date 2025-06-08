using SpatialLite.Contracts;

namespace SpatialLite.Gpx.Geometries;

/// <summary>
/// Defines common properties for all GpxGeometry types
/// </summary>
public interface IGpxGeometry : IGeometry
{
    /// <summary>
    /// Get the type of geometry
    /// </summary>
    public GpxGeometryType GeometryType { get; }
}
