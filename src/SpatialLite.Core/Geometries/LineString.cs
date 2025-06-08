using SpatialLite.Contracts;

namespace SpatialLite.Core.Geometries;

/// <summary>
/// Represents a curve with linear interpolation between consecutive vertices.  
/// </summary>
public class LineString : ILineString
{
    private readonly List<Coordinate> _coordinates;

    /// <summary>
    /// Initializes a new instance of the <c>LineString</c> class that is empty and has assigned WSG84 coordinate reference system.
    /// </summary>
    public LineString()
    {
        _coordinates = [];
    }

    /// <summary>
    /// Initializes a new instance of the <c>LineString</c> class with specified coordinates and WSG84 coordinate reference system.
    /// </summary>
    /// <param name="coordinates">The collection of coordinates to be copied to the new LineString.</param>
    public LineString(IEnumerable<Coordinate> coordinates)
    {
        _coordinates = new(coordinates);
    }

    /// <summary>
    /// Gets the list of coordinates, that define this LineString
    /// </summary>
    public virtual List<Coordinate> Coordinates
    {
        get { return _coordinates; }
    }

    /// <summary>
    /// Gets the list of coordinates, that define this LineString
    /// </summary>
    IReadOnlyList<Coordinate> ILineString.Coordinates
    {
        get { return _coordinates; }
    }

    /// <summary>
    /// Gets the first coordinate of the <c>ILineString</c> object.
    /// </summary>
    public Coordinate Start
    {
        get
        {
            if (_coordinates.Count == 0)
            {
                return Coordinate.Empty;
            }

            return _coordinates[0];
        }
    }

    /// <summary>
    /// Gets the last coordinate of the <c>ILineString</c> object.
    /// </summary>
    public Coordinate End
    {
        get
        {
            if (_coordinates.Count == 0)
            {
                return Coordinate.Empty;
            }

            return _coordinates[^1];
        }
    }

    /// <summary>
    /// Gets a value indicating whether this <c>LineString</c> is closed.
    /// </summary>
    /// <remarks>
    /// The LineStringBase is closed if <see cref="Start"/> and <see cref="End"/> are identical.
    /// </remarks>
    public virtual bool IsClosed
    {
        get
        {
            if (_coordinates.Count == 0)
            {
                return false;
            }
            else
            {
                return Start.Equals(End);
            }
        }
    }

    /// <summary>
    /// Computes envelope of the <c>IGeometry</c> object. The envelope is defined as a minimal bounding box for a geometry.
    /// </summary>
    /// <returns>
    /// Returns an <see cref="Envelope"/> object that specifies the minimal bounding box of the <c>IGeometry</c> object.
    /// </returns>
    public Envelope GetEnvelope()
    {
        return new Envelope(_coordinates);
    }

    /// <summary>
    /// Gets collection of all <see cref="Coordinate"/> of this IGeometry object
    /// </summary>
    /// <returns>the collection of all <see cref="Coordinate"/> of this object</returns>
    public IEnumerable<Coordinate> GetCoordinates()
    {
        return Coordinates;
    }
}
