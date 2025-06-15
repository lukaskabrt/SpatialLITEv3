namespace SpatialLite.Contracts.Algorithms;

/// <summary>
/// Specifies how algorithms treat lines defined by two points.
/// </summary>
public enum LineMode
{
    /// <summary>
    /// Line is treated as line segment.
    /// </summary>
    LineSegment,

    /// <summary>
    /// Line is treated as infinite line.
    /// </summary>
    Line
}
