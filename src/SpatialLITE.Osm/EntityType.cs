namespace SpatialLite.Osm;

/// <summary>
/// Defines possible type of object that <see cref="IOsmEntity" /> can represent.
/// </summary>
public enum EntityType
{
    /// <summary>
    /// Node
    /// </summary>
    Node,

    /// <summary>
    /// Way
    /// </summary>
    Way,

    /// <summary>
    /// Relation
    /// </summary>
    Relation
}
