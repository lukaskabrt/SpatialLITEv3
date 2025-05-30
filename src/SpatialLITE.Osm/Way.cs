namespace SpatialLite.Osm;

/// <summary>
/// Represents information about way.
/// </summary>
/// <remarks>
/// Nodes are represented with their IDs only.
/// </remarks>
public class Way : IOsmEntity
{
    /// <summary>
    /// Gets type of the object that is represented by this <see cref="IOsmEntity"/>.
    /// </summary>
    public EntityType EntityType => EntityType.Way;

    /// <summary>
    /// Gets ID of the object.
    /// </summary>
    public required long Id { get; set; }

    /// <summary>
    /// Gets the collection of tags associated with this way.
    /// </summary>
    public required TagsCollection Tags { get; set; }

    /// <summary>
    /// Gets the IDs of the nodes comprising this way.
    /// </summary>
    public required IList<long> Nodes { get; set; }

    /// <summary>
    /// Gets additional information about this way.
    /// </summary>
    public EntityMetadata? Metadata { get; set; }
}
