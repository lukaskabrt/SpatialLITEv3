namespace SpatialLite.Osm;

/// <summary>
/// Represents information about relation.
/// </summary>
public class Relation : IOsmEntity
{
    /// <summary>
    /// Gets type of the object that is represented by this <see cref="IOsmEntity" />.
    /// </summary>
    public EntityType EntityType => EntityType.Relation;

    /// <summary>
    /// Gets ID of the relation.
    /// </summary>
    public required long Id { get; set; }

    /// <summary>
    /// Gets the collection of tags associated with this relation.
    /// </summary>
    public required TagsCollection Tags { get; set; }

    /// <summary>
    /// Gets list of members of the relation.
    /// </summary>
    public required IList<RelationMember> Members { get; set; }

    /// <summary>
    /// Gets additional information about this relation.
    /// </summary>
    public EntityMetadata? Metadata { get; set; }
}
