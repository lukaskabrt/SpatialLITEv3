using SpatialLite.Contracts;

namespace SpatialLite.Gpx.Geometries;

public class GpxLineString : ILineString
{
    private readonly GpxLineStringCoordinatesAdapter _coordinatesAdapter;
    private readonly List<GpxPoint> _points;

    public List<GpxPoint> Points
    {
        get { return _points; }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="GpxLineString"/> is closed.
    /// </summary>
    /// <remarks>
    /// The LineStringBase is closed if <see cref="Start"/> and <see cref="End"/> are identical.
    /// </remarks>
    public virtual bool IsClosed
    {
        get
        {
            if (Coordinates.Count == 0)
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
    /// Gets the first coordinate of this <see cref="GpxLineString"/> object.
    /// </summary>
    public Coordinate Start
    {
        get
        {
            if (_coordinatesAdapter.Count == 0)
            {
                return Coordinate.Empty;
            }

            return _coordinatesAdapter[0];
        }
    }

    /// <summary>
    /// Gets the last coordinate of this <see cref="GpxLineString"/> object.
    /// </summary>
    public Coordinate End
    {
        get
        {
            if (_coordinatesAdapter.Count == 0)
            {
                return Coordinate.Empty;
            }

            return _coordinatesAdapter[^1];
        }
    }

    public IReadOnlyList<Coordinate> Coordinates => _coordinatesAdapter;

    public IEnumerable<Coordinate> GetCoordinates() => _coordinatesAdapter;

    public Envelope GetEnvelope() => new Envelope(_coordinatesAdapter);

    /// <summary>
    /// Initializes a new, empty instance of the <see cref="GpxLineString"/> class.
    /// </summary>
    public GpxLineString()
    {
        _points = [];
        _coordinatesAdapter = new GpxLineStringCoordinatesAdapter(_points);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GpxLineString"/> class with supplied points.
    /// </summary>
    /// <param name="points">A list of <see cref="GpxPoint"/> objects that define the line string.</param>
    public GpxLineString(List<GpxPoint> points)
    {
        _points = points;
        _coordinatesAdapter = new GpxLineStringCoordinatesAdapter(points);
    }
}
