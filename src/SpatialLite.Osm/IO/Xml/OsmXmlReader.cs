using System.Xml;

namespace SpatialLite.Osm.IO.Xml;

/// <summary>
/// Represents a OsmReader, that can read OSM entities saved in the XML format.
/// </summary>
public class OsmXmlReader : IOsmReader
{
    private bool _disposed = false;

    private readonly XmlReader _xmlReader;

    private readonly Stream? _input;
    private readonly bool _ownsInputStream = false;

    /// <summary>
    /// Initializes a new instance of the OsmXmlReader class that reads data from the specified file.
    /// </summary>
    /// <param name="path">Path to the .osm file.</param>
    /// <param name="settings">The OsmReaderSettings object that determines behaviour of XmlReader.</param>
    public OsmXmlReader(string path, OsmXmlReaderSettings settings)
    {
        _input = new FileStream(path, FileMode.Open, FileAccess.Read);
        _ownsInputStream = true;
        _xmlReader = InitializeReader();

        Settings = settings;
    }

    /// <summary>
    /// Initializes a new instance of the OsmXmlReader class that reads data from the specified stream.
    /// </summary>
    /// <param name="stream">The stream with osm xml data.</param>
    /// <param name="settings">The OsmReaderSettings object that determines behaviour of XmlReader.</param>
    public OsmXmlReader(Stream stream, OsmXmlReaderSettings settings)
    {
        _input = stream;
        _xmlReader = InitializeReader();

        Settings = settings;
    }

    /// <summary>
    /// Gets OsmReaderSettings object that contains properties which determine behaviour of the reader.
    /// </summary>
    public OsmXmlReaderSettings Settings { get; private set; }

    /// <summary>
    /// Reads the next OSM entity from the stream.
    /// </summary>
    /// <returns>IEntityInfo object with information about entity read from the stream, or null if no more entities are available.</returns>
    public IOsmEntity? Read()
    {
        IOsmEntity? result = null;

        while (_xmlReader.NodeType != XmlNodeType.EndElement && result == null)
        {
            switch (_xmlReader.Name)
            {
                case "node":
                    result = ReadNode();
                    break;
                case "way":
                    result = ReadWay();
                    break;
                case "relation":
                    result = ReadRelation();
                    break;
                default:
                    _xmlReader.Read();
                    break;
            }
        }

        return result;
    }

    /// <summary>
    /// Releases all resources used by the OsmXmlReader.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Reads Node from the current element in the _xmlReader.
    /// </summary>
    /// <returns>Information about parsed node.</returns>
    private Node ReadNode()
    {
        // id
        var attId = _xmlReader.GetAttribute("id") ?? throw new XmlException("Attribute 'id' is missing.");
        var nodeId = long.Parse(attId, System.Globalization.CultureInfo.InvariantCulture);

        // latitude
        var attLat = _xmlReader.GetAttribute("lat") ?? throw new XmlException("Attribute 'lat' is missing.");
        var nodeLat = double.Parse(attLat, System.Globalization.CultureInfo.InvariantCulture);

        // longitude
        var attLon = _xmlReader.GetAttribute("lon") ?? throw new XmlException("Attribute 'lon' is missing.");
        var nodeLon = double.Parse(attLon, System.Globalization.CultureInfo.InvariantCulture);

        EntityMetadata? additionalInfo = null;
        if (Settings.ReadMetadata)
        {
            additionalInfo = ReadMetadata();
        }

        var result = new Node { Id = nodeId, Latitude = nodeLat, Longitude = nodeLon, Tags = [], Metadata = additionalInfo };

        if (_xmlReader.IsEmptyElement == false)
        {
            _xmlReader.Read();

            while (_xmlReader.NodeType != XmlNodeType.EndElement)
            {
                if (_xmlReader.NodeType == XmlNodeType.Element && _xmlReader.Name == "tag")
                {
                    result.Tags.Add(ReadTag());
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
    /// Reads Way from the current element in the _xmlReader.
    /// </summary>
    /// <returns>Information about parsed way.</returns>
    private Way ReadWay()
    {
        var attId = _xmlReader.GetAttribute("id") ?? throw new XmlException("Attribute 'id' is missing.");
        var wayId = long.Parse(attId, System.Globalization.CultureInfo.InvariantCulture);

        EntityMetadata? additionalInfo = null;
        if (Settings.ReadMetadata)
        {
            additionalInfo = ReadMetadata();
        }

        var way = new Way { Id = wayId, Tags = [], Nodes = [], Metadata = additionalInfo };

        if (_xmlReader.IsEmptyElement == false)
        {
            _xmlReader.Read();

            while (_xmlReader.NodeType != XmlNodeType.EndElement)
            {
                switch (_xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (_xmlReader.Name)
                        {
                            case "nd":
                                way.Nodes.Add(ReadWayNd());
                                continue;
                            case "tag":
                                way.Tags.Add(ReadTag());
                                continue;
                            default:
                                _xmlReader.Skip();
                                continue;
                        }

                    default:
                        _xmlReader.Skip();
                        break;
                }
            }
        }

        _xmlReader.Skip();
        return way;
    }

    /// <summary>
    /// Reads Node reference from the current Way element in the _xmlReader.
    /// </summary>
    /// <returns>Reference to the node.</returns>
    private long ReadWayNd()
    {
        var attRef = _xmlReader.GetAttribute("ref") ?? throw new XmlException("Attribute 'ref' is missing.");
        var nodeId = long.Parse(attRef, System.Globalization.CultureInfo.InvariantCulture);

        _xmlReader.Skip();

        return nodeId;
    }

    /// <summary>
    /// Reads Relation from the current element in the _xmlReader.
    /// </summary>
    /// <returns>Information about parsed relation.</returns>
    private Relation ReadRelation()
    {
        var attId = _xmlReader.GetAttribute("id") ?? throw new XmlException("Attribute 'id' is missing.");
        var relationId = long.Parse(attId, System.Globalization.CultureInfo.InvariantCulture);

        EntityMetadata? additionalInfo = null;
        if (Settings.ReadMetadata)
        {
            additionalInfo = ReadMetadata();
        }

        var relation = new Relation { Id = relationId, Tags = [], Members = [], Metadata = additionalInfo };

        if (false == _xmlReader.IsEmptyElement)
        {
            _xmlReader.Read();

            while (_xmlReader.NodeType != XmlNodeType.EndElement)
            {
                switch (_xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (_xmlReader.Name)
                        {
                            case "member":
                                relation.Members.Add(ReadRelationMember());
                                continue;
                            case "tag":
                                relation.Tags.Add(ReadTag());
                                continue;
                            default:
                                _xmlReader.Skip();
                                continue;
                        }

                    default:
                        _xmlReader.Skip();
                        break;
                }
            }
        }

        _xmlReader.Skip();

        return relation;
    }

    /// <summary>
    /// Reads RelationMember from the current Relation in the _xmlReader.
    /// </summary>
    /// <returns>Information about parsed relation member</returns>
    private RelationMember ReadRelationMember()
    {
        var attType = _xmlReader.GetAttribute("type") ?? throw new XmlException("Attribute 'type' is missing.");

        var attRef = _xmlReader.GetAttribute("ref") ?? throw new XmlException("Attribute 'ref' is missing.");
        var refId = long.Parse(attRef, System.Globalization.CultureInfo.InvariantCulture);

        var attRole = _xmlReader.GetAttribute("role");

        var memberType = attType switch
        {
            "node" => EntityType.Node,
            "way" => EntityType.Way,
            "relation" => EntityType.Relation,
            _ => throw new XmlException("Unknown relation member type"),
        };
        _xmlReader.Skip();

        return new RelationMember() { MemberType = memberType, MemberId = refId, Role = attRole };
    }

    /// <summary>
    /// Reads Tag from the current element in the _xmlReader.
    /// </summary>
    /// <returns>the parsed Tag.</returns>
    private KeyValuePair<string, string> ReadTag()
    {
        var attK = _xmlReader.GetAttribute("k") ?? throw new XmlException("Attribute 'k' is missing.");
        var attV = _xmlReader.GetAttribute("v") ?? throw new XmlException("Attribute 'v' is missing.");

        _xmlReader.Skip();

        return new(attK, attV);
    }

    /// <summary>
    /// Reads metadata of the osm entity
    /// </summary>
    /// <returns>Metadata of the entity read from the reader.</returns>
    private EntityMetadata ReadMetadata()
    {
        var result = new EntityMetadata();

        // version
        var attVersion = _xmlReader.GetAttribute("version");
        if (attVersion == null)
        {
            if (Settings.StrictMode)
            {
                throw new XmlException("Attribute 'version' is missing.");
            }
        }
        else
        {
            result.Version = int.Parse(attVersion, System.Globalization.CultureInfo.InvariantCulture);
        }

        // changeset
        var attChangeset = _xmlReader.GetAttribute("changeset");
        if (attChangeset == null)
        {
            if (Settings.StrictMode)
            {
                throw new XmlException("Attribute 'changeset' is missing.");
            }
        }
        else
        {
            result.Changeset = int.Parse(attChangeset, System.Globalization.CultureInfo.InvariantCulture);
        }

        // uid
        var attUid = _xmlReader.GetAttribute("uid");
        if (attUid == null)
        {
            if (Settings.StrictMode)
            {
                throw new XmlException("Attribute 'uid' is missing.");
            }
        }
        else
        {
            result.Uid = int.Parse(attUid, System.Globalization.CultureInfo.InvariantCulture);
        }

        // user		
        var attrUser = _xmlReader.GetAttribute("user");
        if (attrUser == null)
        {
            if (Settings.StrictMode)
            {
                throw new XmlException("Attribute 'user' is missing.");
            }
        }
        else
        {
            result.User = attrUser;
        }

        // visible
        var attVisible = _xmlReader.GetAttribute("visible");
        if (attVisible == null)
        {
            if (Settings.StrictMode)
            {
                result.Visible = true;
            }
        }
        else
        {
            result.Visible = bool.Parse(attVisible);
        }

        // timestamp
        var attTimestamp = _xmlReader.GetAttribute("timestamp");
        if (attTimestamp == null)
        {
            if (Settings.StrictMode)
            {
                throw new XmlException("Attribute 'timestamp' is missing.");
            }
        }
        else
        {
            result.Timestamp = DateTimeOffset.ParseExact(attTimestamp, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal);
        }

        return result;
    }

    /// <summary>
    /// Initializes internal properties
    /// </summary>
    private XmlReader InitializeReader()
    {
        var xmlReaderSettings = new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true
        };

        var xmlReader = XmlReader.Create(_input!, xmlReaderSettings);
        xmlReader.Read();

        var insideOsm = false;
        while (xmlReader.EOF == false && insideOsm == false)
        {
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    if (xmlReader.Name != "osm")
                    {
                        throw new XmlException("Invalid xml root element. Expected <osm>.");
                    }

                    insideOsm = true;
                    break;

                default:
                    xmlReader.Read();
                    break;
            }
        }

        return xmlReader;
    }

    /// <summary>
    /// Releases the unmanaged resources used by the ComponentLibrary and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_ownsInputStream)
                {
                    _input?.Dispose();
                }
            }

            _disposed = true;
        }
    }
}
