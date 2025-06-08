namespace SpatialLite.Gpx.Geometries;

/// <summary>
/// Contains additional information about Gpx tracks and routes
/// </summary>
public class GpxTrackMetadata : GpxMetadata
{
    /// <summary>
    /// Gets or sets type (classification) of route.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets GPS route number.
    /// </summary>
    public int? Number { get; set; }
}
