﻿using System.Diagnostics.CodeAnalysis;

namespace SpatialLite.Gpx.Geometries;

/// <summary>
/// Represents a link to an external resource (Web page, digital photo, video clip, etc) with additional information.
/// </summary>
public class GpxLink
{
    /// <summary>
    /// Gets the URL of the link.
    /// </summary>
    public required Uri Url { get; set; }

    /// <summary>
    /// Gets the text of the hyperlink.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets the mime type of the linked content.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Creates a new instance of the GpxLink.
    /// </summary>
    public GpxLink()
    {
    }

    /// <summary>
    /// Creates a new instance of the GpxLink with given url.
    /// </summary>
    /// <param name="url">The url of the link.</param>
    [SetsRequiredMembers]
    public GpxLink(Uri url)
    {
        Url = url;
    }
}
