using SpatialLite.Osm.IO;

namespace SpatialLITE.Osm.IO.Pbf;

/// <summary>
///  Contains settings that determine behaviour of the PbfWriter.
/// </summary>
public class PbfWriterSettings : OsmWriterSettings
{
    /// <summary>
    /// Initializes a new instance of the PbfWriterSettings class with default values.
    /// </summary>
    public PbfWriterSettings()
        : base()
    {
        UseDenseFormat = true;
        Compression = CompressionMode.ZlibDeflate;
    }

    /// <summary>
    /// Gets or sets a value indicating whether PbfWriter should use dense format for serializing nodes.
    /// </summary>
    public bool UseDenseFormat { get; init; } = true;

    /// <summary>
    /// Gets or sets a compression to be used by PbfWriter.
    /// </summary>
    public CompressionMode Compression { get; init; } = CompressionMode.ZlibDeflate;
}
