using SpatialLite.Contracts;

namespace SpatialLite.Core.Geometries;

/// <summary>
/// Represents a polygon, which may include holes.
/// </summary>
public class Polygon : Geometry, IPolygon
{
    /// <summary>
    /// Initializes a new instance of the <c>Polygon</c> class in WSG84 coordinate reference system that without ExteriorRing and no InteriorRings.
    /// </summary>
    public Polygon()
        : base()
    {
        ExteriorRing = [];
        InteriorRings = [];
    }

    /// <summary>
    /// Initializes a new instance of the <c>Polygon</c> class with the given exterior boundary in WSG84 coordinate reference system.
    /// </summary>
    /// <param name="exteriorRing">The exterior boundary of the polygon.</param>
    public Polygon(List<Coordinate> exteriorRing)
    {
        ExteriorRing = exteriorRing;
        InteriorRings = [];
    }

    /// <summary>
    /// Gets or sets the exterior boundary of the polygon.
    /// </summary>
    public List<Coordinate> ExteriorRing { get; set; }

    /// <summary>
    /// Gets the exterior boundary of the polygon.
    /// </summary>
    IReadOnlyList<Coordinate> IPolygon.ExteriorRing
    {
        get { return ExteriorRing; }
    }

    /// <summary>
    /// Gets the list of holes in the polygon.
    /// </summary>
    public List<List<Coordinate>> InteriorRings { get; private set; }

    /// <summary>
    /// Gets the list of holes in the polygon.
    /// </summary>
    IEnumerable<IReadOnlyList<Coordinate>> IPolygon.InteriorRings
    {
        get { return InteriorRings; }
    }

    /// <summary>
    /// Computes envelope of the <c>Polygon</c> object. The envelope is defined as a minimal bounding box for a geometry.
    /// </summary>
    /// <returns>
    /// Returns an <see cref="Envelope"/> object that specifies the minimal bounding box of the <c>Polygon</c> object.
    /// </returns>
    public override Envelope GetEnvelope()
    {
        return ExteriorRing.Count == 0 ? new Envelope() : new Envelope(ExteriorRing);
    }

    /// <summary>
    /// Gets collection of all <see cref="Coordinate"/> of this IGeometry object
    /// </summary>
    /// <returns>the collection of all <see cref="Coordinate"/> of this object</returns>
    public override IEnumerable<Coordinate> GetCoordinates()
    {
        return ExteriorRing.Concat(InteriorRings.SelectMany(o => o));
    }
}
