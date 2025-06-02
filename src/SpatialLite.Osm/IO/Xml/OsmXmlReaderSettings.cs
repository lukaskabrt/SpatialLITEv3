using SpatialLite.Osm.IO;

namespace SpatialLITE.Osm.IO.Xml;

/// <summary>
/// Contains settings that determine behaviour of the OsmXmlReader.
/// </summary>
public class OsmXmlReaderSettings : OsmReaderSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether OsmXmlReader should run in strict mode.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Default value is false.
    /// </para>
    /// <para>
    /// In strict mode missing attributes for entity metadata causes OsmXmlReader to throw an exception. If strict mode is off and some metadata attributes are missing, missing attributes are set to their default values. 
    /// </para>
    /// </remarks>
    public bool StrictMode { get; init; }
}
