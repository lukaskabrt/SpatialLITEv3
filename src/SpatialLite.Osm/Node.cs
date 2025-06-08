using SpatialLite.Contracts;

namespace SpatialLite.Osm;

/// <summary>
/// Represents information about node.
/// </summary>
public class Node : IOsmEntity
{
    /// <summary>
    /// Gets type of the object that is represented by this <see cref="IOsmEntity" />.
    /// </summary>
    public EntityType EntityType => EntityType.Node;

    /// <summary>
    /// Gets ID of the object.
    /// </summary>
    public required long Id { get; set; }

    /// <summary>
    /// Gets the collection of tags associated with this node.
    /// </summary>
    public required TagsCollection Tags { get; set; }

    /// <summary>
    /// Gets the position of the node.
    /// </summary>
    public required Coordinate Position { get; set; }

    /// <summary>
    /// Gets or sets metadata of this Node.
    /// </summary>
    public EntityMetadata? Metadata { get; set; }
}
