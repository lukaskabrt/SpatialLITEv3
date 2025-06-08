namespace SpatialLite.Gpx.IO;

/// <summary>
/// Contains settings that determine behaviour of GpxReader.
/// </summary>
public class GpxReaderSettings
{
    /// <summary>
    /// Gets a value indicating whether GpxReader should read and parse metadata.
    /// </summary>
    public bool ReadMetadata { get; init; } = true;
}
