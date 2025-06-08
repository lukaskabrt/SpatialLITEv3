using System.Globalization;
using System.Text;
using System.Xml;

namespace SpatialLite.Osm.IO.Xml;

/// <summary>
/// Represents an IOsmWriter, that can write OSM entities to XML format.
/// </summary>
public class OsmXmlWriter : IOsmWriter
{
    private readonly Stream _output;
    private readonly bool _ownsOutputStream;

    private readonly XmlWriter _writer;
    private bool _isInsideOsm = false;
    private bool _disposed = false;

    // underlying stream writer for writing to files
    private readonly StreamWriter? _streamWriter;

    /// <summary>
    /// Initializes a new instance of the OsmXmlWriter class that writes OSM entities to the specified stream.
    /// </summary>
    /// <param name="stream">The Stream to write OSM entities to.</param>
    /// <param name="settings">The settings defining behaviour of the writer.</param>
    public OsmXmlWriter(Stream stream, OsmWriterSettings settings)
    {
        _output = stream;
        _ownsOutputStream = false;

        var writerSetting = new XmlWriterSettings
        {
            Indent = true
        };

        _writer = XmlWriter.Create(stream, writerSetting);

        Settings = settings;
    }

    /// <summary>
    /// Initializes a new instance of the OsmXmlWriter class that writes OSM entities to the specified file.
    /// </summary>
    /// <param name="path">Path to the OSM file.</param>
    /// <param name="settings">The settings defining behaviour of the writer.</param>
    /// <remarks>If the file exists, it is overwritten, otherwise, a new file is created.</remarks>
    public OsmXmlWriter(string path, OsmWriterSettings settings)
    {
        _output = new FileStream(path, FileMode.Create, FileAccess.Write);
        _ownsOutputStream = true;

        var writerSetting = new XmlWriterSettings
        {
            Indent = true
        };

        _streamWriter = new StreamWriter(_output, new UTF8Encoding(false));
        _writer = XmlWriter.Create(_streamWriter, writerSetting);

        Settings = settings;
    }

    /// <summary>
    /// Gets settings used by this XmlWriter.
    /// </summary>
    public OsmWriterSettings Settings { get; private set; }

    /// <summary>
    /// Releases all resources used by the OsmXmlWriter.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Writes specified entity data-transfer object in XML format to the underlaying stream.
    /// </summary>
    /// <param name="entity">Entity to write.</param>
    public void Write(IOsmEntity entity)
    {
        if (Settings.WriteMetadata)
        {
            if (entity.Metadata == null)
            {
                throw new ArgumentException("Entity doesn't contain metadata object, but writer was created with WriteMetadata setting.");
            }
        }

        if (_isInsideOsm == false)
        {
            StartDocument();
        }

        switch (entity.EntityType)
        {
            case EntityType.Node:
                WriteNode((Node)entity);
                break;
            case EntityType.Way:
                WriteWay((Way)entity);
                break;
            case EntityType.Relation:
                WriteRelation((Relation)entity);
                break;
        }
    }

    /// <summary>
    /// Causes any buffered data to be written to the underlaying storage.
    /// </summary>
    public void Flush()
    {
        _writer.Flush();
    }

    /// <summary>
    /// Writes &lt;osm&gt; start element to the output stream.
    /// </summary>
    private void StartDocument()
    {
        _writer.WriteStartElement("osm");
        _writer.WriteAttributeString("version", "0.6");
        if (string.IsNullOrEmpty(Settings.ProgramName) == false)
        {
            _writer.WriteAttributeString("generator", Settings.ProgramName);
        }

        _isInsideOsm = true;
    }

    /// <summary>
    /// Writes &lt;osm&gt; end element to the output stream.
    /// </summary>
    private void EndDocument()
    {
        _writer.WriteEndElement();
        _isInsideOsm = false;
    }

    /// <summary>
    /// Writes node to the output stream.
    /// </summary>
    /// <param name="node">The Node to be written.</param>
    private void WriteNode(Node node)
    {
        _writer.WriteStartElement("node");
        _writer.WriteAttributeString("lon", node.Position.X.ToString(CultureInfo.InvariantCulture));
        _writer.WriteAttributeString("lat", node.Position.Y.ToString(CultureInfo.InvariantCulture));
        _writer.WriteAttributeString("id", node.Id.ToString(CultureInfo.InvariantCulture));

        if (Settings.WriteMetadata)
        {
            WriteMetadata(node.Metadata);
        }

        WriteTags(node.Tags);

        _writer.WriteEndElement();
    }

    /// <summary>
    /// Writes way to the output stream
    /// </summary>
    /// <param name="way">The Way to be written</param>
    private void WriteWay(Way way)
    {
        _writer.WriteStartElement("way");
        _writer.WriteAttributeString("id", way.Id.ToString(CultureInfo.InvariantCulture));

        if (Settings.WriteMetadata)
        {
            WriteMetadata(way.Metadata);
        }

        WriteTags(way.Tags);

        for (var i = 0; i < way.Nodes.Count; i++)
        {
            _writer.WriteStartElement("nd");
            _writer.WriteAttributeString("ref", way.Nodes[i].ToString(CultureInfo.InvariantCulture));
            _writer.WriteEndElement();
        }

        _writer.WriteEndElement();
    }

    /// <summary>
    /// Writes relation to the output stream.
    /// </summary>
    /// <param name="relation">The relation to be written.</param>
    private void WriteRelation(Relation relation)
    {
        _writer.WriteStartElement("relation");
        _writer.WriteAttributeString("id", relation.Id.ToString(CultureInfo.InvariantCulture));

        if (Settings.WriteMetadata)
        {
            WriteMetadata(relation.Metadata);
        }

        WriteTags(relation.Tags);

        for (var i = 0; i < relation.Members.Count; i++)
        {
            _writer.WriteStartElement("member");
            _writer.WriteAttributeString("ref", relation.Members[i].MemberId.ToString(CultureInfo.InvariantCulture));
            _writer.WriteAttributeString("role", relation.Members[i].Role);
            _writer.WriteAttributeString("type", relation.Members[i].MemberType.ToString().ToLower(CultureInfo.InvariantCulture));
            _writer.WriteEndElement();
        }

        _writer.WriteEndElement();
    }

    /// <summary>
    /// Writes collection of tags to the output stream.
    /// </summary>
    /// <param name="tags">The collection of tags to write.</param>
    private void WriteTags(TagsCollection tags)
    {
        foreach (var tag in tags)
        {
            _writer.WriteStartElement("tag");
            _writer.WriteAttributeString("k", tag.Key);
            _writer.WriteAttributeString("v", tag.Value);
            _writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Writes detailed attributes of the OSM entity.
    /// </summary>
    /// <param name="metadata">The entity whose attributes to be written.</param>
    private void WriteMetadata(EntityMetadata? metadata)
    {
        if (metadata == null)
        {
            return;
        }

        _writer.WriteAttributeString("version", metadata.Version.ToString(CultureInfo.InvariantCulture));

        if (metadata.Changeset.HasValue)
        {
            _writer.WriteAttributeString("changeset", metadata.Changeset.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (metadata.Uid > 0)
        {
            _writer.WriteAttributeString("uid", metadata.Uid.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (!string.IsNullOrEmpty(metadata.User))
        {
            _writer.WriteAttributeString("user", metadata.User);
        }

        if (metadata.Visible.HasValue)
        {
            _writer.WriteAttributeString("visible", metadata.Visible.Value.ToString().ToLower(CultureInfo.InvariantCulture));
        }

        _writer.WriteAttributeString("timestamp", metadata.Timestamp.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Releases the unmanaged resources used by the OsmXmlWriter and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (_isInsideOsm)
            {
                EndDocument();
            }

            _writer?.Dispose();

            if (disposing)
            {
                _streamWriter?.Dispose();

                if (_ownsOutputStream)
                {
                    _output.Dispose();
                }
            }

            _disposed = true;
        }
    }
}
