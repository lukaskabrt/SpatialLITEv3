﻿using SpatialLite.Contracts;
using SpatialLite.Gpx.Geometries;
using System.Globalization;
using System.Xml;

namespace SpatialLite.Gpx.IO;

/// <summary>
/// Implements data reader that can read GPX data from streams and files.
/// </summary>
public class GpxReader : IGpxReader, IDisposable
{
    private static readonly XmlReaderSettings XmlReaderSettings = new()
    {
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
        IgnoreWhitespace = true
    };

    private bool _disposed = false;
    private readonly XmlReader _xmlReader;

    /// <summary>
    /// Underlaying stream to read data from
    /// </summary>
    private readonly Stream _input;

    private bool _insideGpx = false;

    /// <summary>
    /// Initializes a new instance of the GpxReader class that reads data from the specified file.
    /// </summary>
    /// <param name="path">Path to the .gpx file.</param>
    /// <param name="settings">The GpxReaderSettings object that determines behaviour of the reader.</param>
    public GpxReader(string path, GpxReaderSettings settings)
    {
        _input = new FileStream(path, FileMode.Open, FileAccess.Read);
        _xmlReader = XmlReader.Create(_input, XmlReaderSettings);

        Settings = settings;
    }

    /// <summary>
    /// Initializes a new instance of the GpxReader class that reads data from the specified stream.
    /// </summary>
    /// <param name="stream">The stream with osm xml data.</param>
    /// <param name="settings">The GpxReaderSettings object that determines behaviour of the reader.</param>
    public GpxReader(Stream stream, GpxReaderSettings settings)
    {
        _input = stream;
        _xmlReader = XmlReader.Create(_input, XmlReaderSettings);

        Settings = settings;
    }

    /// <summary>
    /// Gets GpxReaderSettings object that determines behaviour of the reader.
    /// </summary>
    public GpxReaderSettings Settings { get; init; }

    /// <summary>
    /// Releases all resources used by the GpxReader.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Parses next element of the GPX file
    /// </summary>
    public IGpxGeometry? Read()
    {
        if (!_insideGpx)
        {
            ReadGpxHeader();
        }

        while ((_xmlReader.NodeType == XmlNodeType.EndElement && _xmlReader.Name == "gpx") == false)
        {
            switch (_xmlReader.Name)
            {
                case "wpt":
                    return ReadPoint("wpt");
                case "trk":
                    return ReadTrack();
                case "rte":
                    return ReadRoute();
                default:
                    _xmlReader.Read();
                    break;
            }
        }

        return null;
    }

    /// <summary>
    /// Reads a GPX point from the internal XmlReader
    /// </summary>
    /// <param name="pointElementName">The name of the surrounding xml element</param>
    /// <returns>the point parsed from the XmlReader</returns>
    private GpxWaypoint ReadPoint(string pointElementName)
    {
        var latValue = _xmlReader.GetAttribute("lat");
        if (string.IsNullOrEmpty(latValue))
        {
            throw new InvalidDataException("Requested attribute 'lat' not found.");
        }

        var lonValue = _xmlReader.GetAttribute("lon");
        if (string.IsNullOrEmpty(lonValue))
        {
            throw new InvalidDataException("Requested attribute 'lon' not found.");
        }

        var lat = double.Parse(latValue, CultureInfo.InvariantCulture);
        var lon = double.Parse(lonValue, CultureInfo.InvariantCulture);

        double? ele = null;
        DateTime? timestamp = null;

        GpxPointMetadata? metadata = null;
        if (Settings.ReadMetadata)
        {
            metadata = new GpxPointMetadata();
        }

        if (_xmlReader.IsEmptyElement == false)
        {
            _xmlReader.Read();

            while ((_xmlReader.NodeType == XmlNodeType.EndElement && _xmlReader.Name == pointElementName) == false)
            {
                bool elementParsed = false;

                if (_xmlReader.Name == "ele")
                {
                    string eleValue = _xmlReader.ReadElementContentAsString();
                    ele = double.Parse(eleValue, CultureInfo.InvariantCulture);
                    elementParsed = true;
                }

                if (_xmlReader.Name == "time")
                {
                    string timeValue = _xmlReader.ReadElementContentAsString();
                    timestamp = DateTime.ParseExact(timeValue, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture);
                    elementParsed = true;
                }

                if (Settings.ReadMetadata)
                {
                    elementParsed = elementParsed || TryReadPointMetadata(metadata!);
                }

                if (!elementParsed)
                {
                    _xmlReader.Skip();
                }
            }
        }

        _xmlReader.Skip();

        var result = new GpxWaypoint(new Coordinate(lon, lat))
        {
            Timestamp = timestamp,
            Elevation = ele,
            Metadata = metadata
        };

        return result;
    }

    /// <summary>
    /// Reads a GPX track from the internal XmlReader
    /// </summary>
    /// <returns>the track parsed form the XmlReader</returns>
    private GpxTrack ReadTrack()
    {
        var result = new GpxTrack();

        if (_xmlReader.IsEmptyElement == false)
        {
            _xmlReader.Read();

            GpxTrackMetadata? metadata = null;
            if (Settings.ReadMetadata)
            {
                metadata = new GpxTrackMetadata();
            }

            while ((_xmlReader.NodeType == XmlNodeType.EndElement && _xmlReader.Name == "trk") == false)
            {
                bool elementParsed = false;
                if (_xmlReader.Name == "trkseg")
                {
                    var segment = ReadTrackSegment();
                    result.Geometries.Add(segment);
                    elementParsed = true;
                }

                if (Settings.ReadMetadata)
                {
                    elementParsed = elementParsed || TryReadTrackMetadata(metadata!);
                }

                if (!elementParsed)
                {
                    _xmlReader.Skip();
                }
            }

            result.Metadata = metadata;
        }

        _xmlReader.Skip();

        return result;
    }

    /// <summary>
    /// Reads a GPX track segment from the internal XmlReader.
    /// </summary>
    /// <returns>the track parsed from the XmlReader</returns>
    private GpxLineString ReadTrackSegment()
    {
        var result = new GpxLineString();

        if (_xmlReader.IsEmptyElement == false)
        {
            _xmlReader.Read();

            while ((_xmlReader.NodeType == XmlNodeType.EndElement && _xmlReader.Name == "trkseg") == false)
            {
                if (_xmlReader.Name == "trkpt")
                {
                    GpxPoint point = ReadPoint("trkpt");
                    result.Points.Add(point);
                }
                else
                {
                    _xmlReader.Skip();
                }
            }
        }

        _xmlReader.Skip();

        return result;
    }

    /// <summary>
    /// Reads a GPX route from the internal XmlReader
    /// </summary>
    /// <returns>the route parsed from the XmlReader</returns>
    private GpxRoute ReadRoute()
    {
        GpxRoute result = new GpxRoute();

        if (_xmlReader.IsEmptyElement == false)
        {
            _xmlReader.Read();

            GpxTrackMetadata? metadata = null;
            if (Settings.ReadMetadata)
            {
                metadata = new GpxTrackMetadata();
            }

            while ((_xmlReader.NodeType == XmlNodeType.EndElement && _xmlReader.Name == "rte") == false)
            {
                bool elementParsed = false;

                if (_xmlReader.Name == "rtept")
                {
                    result.Points.Add(ReadPoint("rtept"));
                    elementParsed = true;
                }

                if (Settings.ReadMetadata)
                {
                    elementParsed = elementParsed || TryReadTrackMetadata(metadata!);
                }

                if (!elementParsed)
                {
                    _xmlReader.Skip();
                }
            }

            result.Metadata = metadata;
        }

        _xmlReader.Skip();

        return result;
    }

    /// <summary>
    /// Initializes internal properties
    /// </summary>
    private void ReadGpxHeader()
    {
        _xmlReader.Read();
        while (_xmlReader.EOF == false && _insideGpx == false)
        {
            if (_xmlReader.NodeType == XmlNodeType.Element)
            {
                if (_xmlReader.Name != "gpx")
                {
                    throw new InvalidDataException("Invalid gpx root element. Expected <gpx>.");
                }

                var version = _xmlReader.GetAttribute("version");
                if (version == null || (version != "1.0" && version != "1.1"))
                {
                    throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "Invalid version of GPX document. Expected '1.0' or '1.1' found {0}.", version));
                }

                _insideGpx = true;

            }
            else
            {
                _xmlReader.Read();
            }
        }
    }

    /// <summary>
    /// Reads a GPX Link from the internal XmlReader
    /// </summary>
    /// <returns>the link parsed from the XmlReader</returns>
    private GpxLink ReadLink()
    {
        var href = _xmlReader.GetAttribute("href")
            ?? throw new InvalidDataException("Requested attribute 'href' not found.");

        string? linkText = null;
        string? linkType = null;

        if (_xmlReader.IsEmptyElement == false)
        {
            _xmlReader.Read();

            while ((_xmlReader.NodeType == XmlNodeType.EndElement && _xmlReader.Name == "link") == false)
            {
                switch (_xmlReader.Name)
                {
                    case "text":
                        linkText = _xmlReader.ReadElementContentAsString();
                        break;
                    case "type":
                        linkType = _xmlReader.ReadElementContentAsString();
                        break;
                    default:
                        _xmlReader.Read();
                        break;
                }
            }
        }

        _xmlReader.Skip();

        return new GpxLink(new Uri(href)) { Text = linkText, Type = linkType };
    }

    /// <summary>
    /// Reads track/route metadata from the internal XmlReader.
    /// </summary>
    /// <param name="metadata">Object to store read metadata</param>
    /// <returns>true if piece of metadata was read, otherwise returns false</returns>
    private bool TryReadTrackMetadata(GpxTrackMetadata metadata)
    {
        switch (_xmlReader.Name)
        {
            case "name":
                metadata.Name = _xmlReader.ReadElementContentAsString();
                return true;
            case "cmt":
                metadata.Comment = _xmlReader.ReadElementContentAsString();
                return true;
            case "desc":
                metadata.Description = _xmlReader.ReadElementContentAsString();
                return true;
            case "src":
                metadata.Source = _xmlReader.ReadElementContentAsString();
                return true;
            case "type":
                metadata.Type = _xmlReader.ReadElementContentAsString();
                return true;
            case "link":
                metadata.Links ??= [];
                metadata.Links.Add(ReadLink());
                return true;
        }

        return false;
    }

    /// <summary>
    /// Reads waypoint metadata from the internal XmlReader.
    /// </summary>
    /// <param name="metadata">Object to store read metadata</param>
    /// <returns>true if piece of metadata was read, otherwise returns false</returns>
    private bool TryReadPointMetadata(GpxPointMetadata metadata)
    {
        switch (_xmlReader.Name)
        {
            case "name":
                metadata.Name = _xmlReader.ReadElementContentAsString();
                return true;
            case "cmt":
                metadata.Comment = _xmlReader.ReadElementContentAsString();
                return true;
            case "desc":
                metadata.Description = _xmlReader.ReadElementContentAsString();
                return true;
            case "src":
                metadata.Source = _xmlReader.ReadElementContentAsString();
                return true;
            case "link":
                metadata.Links ??= [];
                metadata.Links.Add(ReadLink());
                return true;
            case "magvar":
                string magvarValue = _xmlReader.ReadElementContentAsString();
                metadata.MagVar = double.Parse(magvarValue, CultureInfo.InvariantCulture);
                return true;
            case "geoidheight":
                string geoidHeightValue = _xmlReader.ReadElementContentAsString();
                metadata.GeoidHeight = double.Parse(geoidHeightValue, CultureInfo.InvariantCulture);
                return true;
            case "hdop":
                string HdopValue = _xmlReader.ReadElementContentAsString();
                metadata.Hdop = double.Parse(HdopValue, CultureInfo.InvariantCulture);
                return true;
            case "vdop":
                string vdopValue = _xmlReader.ReadElementContentAsString();
                metadata.Vdop = double.Parse(vdopValue, CultureInfo.InvariantCulture);
                return true;
            case "pdop":
                string pdopValue = _xmlReader.ReadElementContentAsString();
                metadata.Pdop = double.Parse(pdopValue, CultureInfo.InvariantCulture);
                return true;
            case "ageofdgpsdata":
                string ageValue = _xmlReader.ReadElementContentAsString();
                metadata.AgeOfDgpsData = double.Parse(ageValue, CultureInfo.InvariantCulture);
                return true;
            case "sat":
                string satValue = _xmlReader.ReadElementContentAsString();
                metadata.SatellitesCount = int.Parse(satValue, CultureInfo.InvariantCulture);
                return true;
            case "dgpsid":
                string dgpsidValue = _xmlReader.ReadElementContentAsString();
                metadata.DgpsId = int.Parse(dgpsidValue, CultureInfo.InvariantCulture);
                return true;
            case "fix":
                string fixValue = _xmlReader.ReadElementContentAsString();
                metadata.Fix = GpxFixHelper.ParseGpsFix(fixValue);
                return true;
        }

        return false;
    }

    /// <summary>
    /// Releases the unmanaged resources used by the GpxReader and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _xmlReader.Dispose();
                _input.Dispose();
            }

            _disposed = true;
        }
    }
}
