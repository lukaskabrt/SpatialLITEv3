namespace SpatialLite.Core.IO;

/// <summary>
/// Contains settings that determine behaviour of the WkbWriter
/// </summary>
public class WkbWriterSettings
{
    /// <summary>
    /// Initializes a new instance of the WkbWriterSettings class with default values.
    /// </summary>
    public WkbWriterSettings()
        : base()
    {
        Encoding = BinaryEncoding.LittleEndian;
    }

    /// <summary>
    /// Gets or sets a encoding that <c>WkbWriter</c> will use for writing geometries.
    /// </summary>
    /// <remarks>
    /// BigEndian encoding is not supported in current version of the <see cref="WkbWriter"/> class.
    /// </remarks>
    public BinaryEncoding Encoding { get; init; }
}
