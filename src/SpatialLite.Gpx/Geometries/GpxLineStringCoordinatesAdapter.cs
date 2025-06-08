using SpatialLite.Contracts;
using System.Collections;

namespace SpatialLite.Gpx.Geometries;

internal class GpxLineStringCoordinatesAdapter : IReadOnlyList<Coordinate>
{
    private readonly List<GpxPoint> _points;

    public Coordinate this[int index] => _points[index].Position;

    public int Count => _points.Count;

    public IEnumerator<Coordinate> GetEnumerator()
    {
        foreach (var point in _points)
        {
            yield return point.Position;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public GpxLineStringCoordinatesAdapter(List<GpxPoint> points)
    {
        _points = points;
    }
}
