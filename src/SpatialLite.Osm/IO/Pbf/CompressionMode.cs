namespace SpatialLite.Osm.IO.Pbf;

/// <summary>
/// Defines compressions that can be used in the PBF format.
/// </summary>
public enum CompressionMode
{
    /// <summary>
    /// No compression is used.
    /// </summary>
    None,

    /// <summary>
    /// Zlib compression.
    /// </summary>
    ZlibDeflate
}
