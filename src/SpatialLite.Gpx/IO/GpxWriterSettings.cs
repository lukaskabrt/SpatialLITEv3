namespace SpatialLite.Gpx.IO;

/// <summary>
/// Contains settings that determine behaviour of the GpxWriter.
/// </summary>
public class GpxWriterSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether GpxWriter should write entity metadata.
    /// </summary>
    public bool WriteMetadata { get; init; } = true;

    /// <summary>
    /// Gets or sets the name of the program that will be saved to the output file.
    /// </summary>
    public string GeneratorName { get; init; } = "SpatialLite";
}
