namespace SpatialLite.Osm;

/// <summary>
/// Defines properties for all OSM entities.
/// </summary>
public interface IOsmEntity
{

    /// <summary>
    /// Gets or sets ID of the object.
    /// </summary>
    public long ID { get; set; }

    /// <summary>
    /// Gets or sets the collection of tags associated with the <see cref="IOsmEntity" />.
    /// </summary>
    public TagsCollection Tags { get; set; }

    /// <summary>
    /// Gets or sets detailed information about OSM entity.
    /// </summary>
    public EntityMetadata? Metadata { get; set; }

    /// <summary>
    /// Gets type of the entity.
    /// </summary>
    public EntityType EntityType { get; }
}
