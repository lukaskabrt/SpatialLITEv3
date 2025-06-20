﻿namespace SpatialLite.Osm.IO;

/// <summary>
/// Contains settings that determine behaviour of the <see cref="IOsmWriter" />.
/// </summary>
public class OsmWriterSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether OsmWriter should write entity metadata.
    /// </summary>
    public bool WriteMetadata { get; init; } = false;

    /// <summary>
    /// Gets or sets the name of the program that will be save to the output file.
    /// </summary>
    public string ProgramName { get; init; } = "SpatialLite";
}
