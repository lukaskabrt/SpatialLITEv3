namespace SpatialLite.Contracts;

/// <summary>
/// Represents a location in the coordinate space.
/// </summary>
public readonly record struct Coordinate(double X, double Y)
{

    /// <summary>
    /// Represents an empty coordinate.
    /// </summary>
    /// <remarks>
    /// The empty coordinate has all coordinates equal to NaN.
    /// </remarks>
    public static readonly Coordinate Empty = new(double.NaN, double.NaN);

    /// <summary>
    /// Returns a <c>string</c> that represents the current <c>Coordinate</c>.
    /// </summary>
    /// <returns>A <c>string</c> that represents the current <c>Coordinate</c></returns>
    public override readonly string ToString()
    {
        return string.Format(System.Globalization.CultureInfo.InvariantCulture, "[{0}, {1}]", X, Y);
    }
}
