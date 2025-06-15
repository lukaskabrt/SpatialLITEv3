namespace SpatialLite.Contracts.Algorithms;

/// <summary>
/// Provides methods for calculating distances between coordinates and between a coordinate and a line.
/// </summary>
public interface IDistanceCalculator
{
    /// <summary>
    /// Calculates the distance between two points.
    /// </summary>
    /// <param name="c1">The first point.</param>
    /// <param name="c2">The second point.</param>
    /// <returns>The distance between the two points.</returns>
    public double CalculateDistance(Coordinate c1, Coordinate c2);

    /// <summary>
    /// Calculates the distance between a point and a line defined by two points.
    /// </summary>
    /// <param name="c">The coordinate to compute the distance for.</param>
    /// <param name="a">One point of the line.</param>
    /// <param name="b">Another point of the line.</param>
    /// <param name="mode">A <see cref="LineMode"/> value that specifies whether AB should be treated as an infinite line or as a line segment.</param>
    /// <returns>The distance from <paramref name="c"/> to the line AB.</returns>
    public double CalculateDistance(Coordinate c, Coordinate a, Coordinate b, LineMode mode);
}
