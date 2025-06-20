﻿using SpatialLite.Core.Geometries;

namespace SpatialLite.Gpx.Geometries;

/// <summary>
/// Represents a track - an ordered list of points describing a path. Points can be grouped in more GpxTrackSegments.
/// </summary>
public class GpxTrack : GeometryCollection<GpxLineString>, IGpxGeometry
{
    /// <summary>
    /// Get or sets additional info about GPX entity
    /// </summary>
    public GpxTrackMetadata? Metadata { get; set; }

    /// <summary>
    /// Creates a new, empty instance of the GpxTrack class instance.
    /// </summary>
    public GpxTrack()
        : base()
    {
    }

    /// <summary>
    /// Creates a new instance of the GpxTrack with given segments
    /// </summary>
    /// <param name="segments">The segments to add into track</param>
    public GpxTrack(IEnumerable<GpxLineString> segments)
        : base(segments)
    {
    }

    /// <summary>
    /// Get the type of geometry
    /// </summary>
    public GpxGeometryType GeometryType
    {
        get { return GpxGeometryType.Track; }
    }
}
