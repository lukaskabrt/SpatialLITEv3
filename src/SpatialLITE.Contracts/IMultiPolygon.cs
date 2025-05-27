namespace SpatialLITE.Contracts;

/// <summary>
/// Defines properties and methods for collections of polygons.
/// </summary>
public interface IMultiPolygon : IGeometryCollection<IPolygon>
{
}
