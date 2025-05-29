namespace SpatialLite.Osm;

/// <summary>
/// Represents information about way.
/// </summary>
/// <remarks>
/// Nodes are represented with their id's only.
/// </remarks>
public class Way : IOsmEntity
{
    /// <summary>
    /// Gets type of the object that is represented by this WayInfo.
    /// </summary>
    public EntityType EntityType => EntityType.Way;

    /// <summary>
    /// Gets ID of the object.
    /// </summary>
    public required long ID { get; set; }

    /// <summary>
    /// Gets the collection of tags associated with this WayInfo.
    /// </summary>
    public required TagsCollection Tags { get; set; }

    /// <summary>
    /// Gets the list of id's of this way nodes.
    /// </summary>
    public required IList<long> Nodes { get; set; }

    /// <summary>
    /// Gets additional information about this IOsmGeometryInfo.
    /// </summary>
    public EntityMetadata? Metadata { get; set; }
}
