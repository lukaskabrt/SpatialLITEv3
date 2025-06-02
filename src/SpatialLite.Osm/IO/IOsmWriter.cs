namespace SpatialLite.Osm.IO;

/// <summary>
/// Defines functions and properties for classes that can write OSM entities to various destinations.
/// </summary>
public interface IOsmWriter : IDisposable
{
    /// <summary>
    /// Writes entity to the target.
    /// </summary>
    /// <param name="entity">The OSM entity to write.</param>
    public void Write(IOsmEntity entity);

    /// <summary>
    /// Clears internal buffers and causes any buffered data to be written to the underlying storage.
    /// </summary>
    public void Flush();
}
