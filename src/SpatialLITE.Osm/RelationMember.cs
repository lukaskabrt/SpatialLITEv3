namespace SpatialLite.Osm;

/// <summary>
/// Represents information about relation member.
/// </summary>
public record struct RelationMember
{
    /// <summary>
    /// The type of the member (node, way, relation).
    /// </summary>
    public required EntityType MemberType { get; set; }

    /// <summary>
    /// The role of the member in relation.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// The ID of the member entity.
    /// </summary>
    public required long MemberId { get; set; }
}
