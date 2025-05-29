namespace SpatialLite.Osm.IO;

/// <summary>
/// Defines functions and properties for classes that can read OSM entities from various sources.
/// </summary>
public interface IOsmReader : IDisposable
{
    /// <summary>
    /// Reads the next OSM entity from a source.
    /// </summary>
    /// <returns><see cref="IOsmEntity"/> object with information about the entity, or null if no more entities are available.</returns>
    public IOsmEntity? Read();
}
