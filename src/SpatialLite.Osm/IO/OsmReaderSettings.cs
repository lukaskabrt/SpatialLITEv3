namespace SpatialLite.Osm.IO;

/// <summary>
/// Contains settings that determine behaviour of <see cref="IOsmReader" />.
/// </summary>
public class OsmReaderSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether <see cref="IOsmReader"/> should read and parse entity metadata.
    /// </summary>
    public bool ReadMetadata { get; init; } = false;
}
