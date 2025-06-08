﻿namespace SpatialLite.Gpx.Geometries;

/// <summary>
/// Contains additional information about Gpx entity
/// </summary>
public abstract class GpxMetadata
{
    /// <summary>
    /// Gets or sets the name of the entity
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the comment for the entity
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the description of the entity. Holds additional information about the element intended for the user, not the GPS.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the source of the data. Included to give user some idea of reliability and accuracy of data.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Get or sets the list of link associated with the entity
    /// </summary>
    public ICollection<GpxLink>? Links { get; set; }
}
