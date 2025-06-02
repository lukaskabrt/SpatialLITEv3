using ProtoBuf;
using SpatialLite.Osm;
using SpatialLite.Osm.IO;
using SpatialLite.Osm.IO.Pbf;
using SpatialLITE.Osm.IO.Pbf.Contracts;

namespace SpatialLITE.Osm.IO.Pbf;

/// <summary>
/// Represents IOsmWriter that writes OSM entities to Pbf format.
/// </summary>
public class PbfWriter : IOsmWriter
{

    /// <summary>
    /// Defines maximal allowed size of uncompressed OsmData block. Larger blocks are considered invalid.
    /// </summary>
    public const int MaxDataBlockSize = 16 * 1024 * 1024;

    private readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private bool _disposed = false;
    private readonly Stream _output;
    private readonly bool _ownsOutputStream;

    private readonly EntityBuffer<Node> _nodesBuffer;
    private readonly EntityBuffer<Way> _wayBuffer;
    private readonly EntityBuffer<Relation> _relationBuffer;

    /// <summary>
    /// Initializes a new instance of the PbfWriter class that writes entities to specified stream with given settings.
    /// </summary>
    /// <param name="stream">The stream to write entities to.</param>
    /// <param name="settings">The settings defining behaviour of the writer.</param>
    public PbfWriter(Stream stream, PbfWriterSettings settings)
    {
        Settings = settings;
        _output = stream;
        _ownsOutputStream = false;

        _nodesBuffer = new EntityBuffer<Node>(Settings.WriteMetadata);
        _wayBuffer = new EntityBuffer<Way>(Settings.WriteMetadata);
        _relationBuffer = new EntityBuffer<Relation>(Settings.WriteMetadata);

        WriteHeader();
    }

    /// <summary>
    /// Initializes a new instance of the PbfWriter class that writes entities to specified file with given settings.
    /// </summary>
    /// <param name="filename">Path to the file to write entities to.</param>
    /// <param name="settings">The settings defining behaviour of the writer.</param>
    public PbfWriter(string filename, PbfWriterSettings settings)
    {
        Settings = settings;
        _output = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
        _ownsOutputStream = true;

        _nodesBuffer = new EntityBuffer<Node>(Settings.WriteMetadata);
        _wayBuffer = new EntityBuffer<Way>(Settings.WriteMetadata);
        _relationBuffer = new EntityBuffer<Relation>(Settings.WriteMetadata);

        WriteHeader();
    }

    /// <summary>
    /// Gets settings used by this PbfWriter.
    /// </summary>
    public PbfWriterSettings Settings { get; private set; }

    /// <summary>
    /// Writes specified entity in PBF format to the underlaying stream.
    /// </summary>
    /// <param name="entity">Entity to write.</param>
    /// <remarks>
    /// PbfWriter uses internal buffers and writes entities to the output stream in blocks. To force PbfWriter to clear internal buffers and write data to the underlaying stream use Flush() function.
    /// </remarks>
    public void Write(IOsmEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        if (Settings.WriteMetadata)
        {
            ArgumentNullException.ThrowIfNull(entity.Metadata, nameof(entity.Metadata));
            ArgumentNullException.ThrowIfNull(entity.Metadata.User, nameof(entity.Metadata.User));
        }

        switch (entity.EntityType)
        {
            case EntityType.Node:
                _nodesBuffer.Add((Node)entity);
                break;
            case EntityType.Way:
                _wayBuffer.Add((Way)entity);
                break;
            case EntityType.Relation:
                _relationBuffer.Add((Relation)entity);
                break;
        }

        if (_nodesBuffer.EstimatedMaxSize > MaxDataBlockSize)
        {
            Flush(EntityType.Node);
        }

        if (_wayBuffer.EstimatedMaxSize > MaxDataBlockSize)
        {
            Flush(EntityType.Way);
        }

        if (_relationBuffer.EstimatedMaxSize > MaxDataBlockSize)
        {
            Flush(EntityType.Relation);
        }
    }

    /// <summary>
    /// Clears internal buffers and causes any buffered data to be written to the unerlaying storage.
    /// </summary>
    public void Flush()
    {
        Flush(EntityType.Node);
        Flush(EntityType.Way);
        Flush(EntityType.Relation);
    }

    /// <summary>
    /// Releases all resources used by the PbfWriter.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Clears internal buffer for specified EntityType and writes all buffered data the underlaying storage.
    /// </summary>
    /// <param name="entityType">The type of entity to process</param>
    private void Flush(EntityType entityType)
    {
        var primitiveBlock = BuildPrimitiveBlock(entityType);
        if (primitiveBlock == null)
        {
            return;
        }

        var primitiveBlockStream = new MemoryStream();
        Serializer.Serialize<PrimitiveBlock>(primitiveBlockStream, primitiveBlock);

        //byte[] buffer = new byte[primitiveBlockStream.Length];
        //Array.Copy(primitiveBlockStream.GetBuffer(), buffer, primitiveBlockStream.Length);

        WriteBlob("OSMData", primitiveBlockStream.ToArray());
    }

    /// <summary>
    /// Writes PBF file header to the underlaying stream.
    /// </summary>
    private void WriteHeader()
    {
        var header = new OsmHeader();
        header.RequiredFeatures.Add("OsmSchema-V0.6");

        if (Settings.UseDenseFormat)
        {
            header.RequiredFeatures.Add("DenseNodes");
        }

        if (Settings.WriteMetadata)
        {
            header.OptionalFeatures.Add("Has_Metadata");
        }

        using (var stream = new MemoryStream())
        {
            Serializer.Serialize<OsmHeader>(stream, header);

            //byte[] buffer = new byte[stream.Length];
            //Array.Copy(stream.GetBuffer(), buffer, stream.Length);

            WriteBlob("OSMHeader", stream.ToArray());
        }
    }

    /// <summary>
    /// Writes blob and it's header to the underlaying stream.
    /// </summary>
    /// <param name="blobType">The type of the blob.</param>
    /// <param name="blobContent">The pbf serialized content of the blob.</param>
    private void WriteBlob(string blobType, byte[] blobContent)
    {
        var blob = new Blob();
        if (Settings.Compression == CompressionMode.None)
        {
            blob.Raw = blobContent;
        }
        else if (Settings.Compression == CompressionMode.ZlibDeflate)
        {
            var stream = new MemoryStream();
            using (var deflateStream = new System.IO.Compression.ZLibStream(stream, System.IO.Compression.CompressionMode.Compress))
            {
                deflateStream.Write(blobContent, 0, blobContent.Length);
            }

            blob.RawSize = blobContent.Length;
            blob.ZlibData = stream.ToArray();
        }

        var blobStream = new MemoryStream();
        Serializer.Serialize<Blob>(blobStream, blob);

        var header = new BlobHeader
        {
            Type = blobType,
            DataSize = (int)blobStream.Length
        };
        Serializer.SerializeWithLengthPrefix(_output, header, PrefixStyle.Fixed32BigEndian);

        blobStream.WriteTo(_output);
    }

    /// <summary>
    /// Creates a PrimitiveBlock with entities of specified type from data in tokens.
    /// </summary>
    /// <param name="entityType">The type of entity to include in PrimitiveBlock.</param>
    /// <returns>PrimitiveBlock with entities of specified type or null if tokens doesn't contain any entities of specified type.</returns>
    private PrimitiveBlock? BuildPrimitiveBlock(EntityType entityType)
    {
        var result = new PrimitiveBlock() { StringTable = new StringTable(), PrimitiveGroup = [] };

        PrimitiveGroup? entityGroup = null;
        switch (entityType)
        {
            case EntityType.Node:
                entityGroup = BuildNodesPrimitiveGroup(result.DateGranularity, result.Granularity, result.LatOffset, result.LonOffset);
                result.StringTable = _nodesBuffer.BuildStringTable();
                _nodesBuffer.Clear();
                break;
            case EntityType.Way:
                entityGroup = BuildWaysPrimitiveGroup(result.DateGranularity);
                result.StringTable = _wayBuffer.BuildStringTable();
                _wayBuffer.Clear();
                break;
            case EntityType.Relation:
                entityGroup = BuildRelationsPrimitiveGroup(result.DateGranularity);
                result.StringTable = _relationBuffer.BuildStringTable();
                _relationBuffer.Clear();
                break;
        }

        if (entityGroup == null)
        {
            return null;
        }

        result.PrimitiveGroup.Add(entityGroup);

        return result;
    }

    /// <summary>
    /// Creates a PrimitiveGroup with serialized nodes from tokens.
    /// </summary>
    /// <param name="timestampGranularity">Timestamp granularity defined in PrimitiveBlock.</param>
    /// <param name="positionGranularity">Granularity defined in PrimitiveBlock.</param>
    /// <param name="latOffset">Latitude offset defined in PrimitiveBlock.</param>
    /// <param name="lonOffset">Longitude offset defined in PrimitiveBlock.</param>
    /// <returns>PrimitiveGroup with nodes from tokens or null if tokens is empty.</returns>
    private PrimitiveGroup? BuildNodesPrimitiveGroup(int timestampGranularity, int positionGranularity, long latOffset, long lonOffset)
    {
        PrimitiveGroup? result = null;

        if (_nodesBuffer.Count > 0)
        {
            result = new PrimitiveGroup();

            if (Settings.UseDenseFormat)
            {
                result.DenseNodes = BuildDenseNodes(timestampGranularity, positionGranularity, latOffset, lonOffset);
            }
            else
            {
                result.Nodes = BuildNodes(timestampGranularity, positionGranularity, latOffset, lonOffset);
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a PrimitiveGroup with serialized ways from tokens.
    /// </summary>
    /// <param name="timestampGranularity">Timestamp granularity defined in PrimitiveBlock.</param>
    /// <returns>PrimitiveGroup with ways from tokens or null if tokens is empty.</returns>
    private PrimitiveGroup? BuildWaysPrimitiveGroup(int timestampGranularity)
    {
        PrimitiveGroup? result = null;

        if (_wayBuffer.Count > 0)
        {
            result = new PrimitiveGroup
            {
                Ways = BuildWays(timestampGranularity)
            };
        }

        return result;
    }

    /// <summary>
    ///  Creates a PrimitiveGroup with serialized relations objects from relation tokens.
    /// </summary>
    /// <param name="timestampGranularity">Timestamp granularity defined in PrimitiveBlock.</param>
    /// <returns>PrimitiveGroup with relations from tokens or null if tokens is empty.</returns>
    private PrimitiveGroup? BuildRelationsPrimitiveGroup(int timestampGranularity)
    {
        PrimitiveGroup? result = null;

        if (_relationBuffer.Count > 0)
        {
            result = new PrimitiveGroup
            {
                Relations = BuildRelations(timestampGranularity)
            };
        }

        return result;
    }

    /// <summary>
    /// Creates a collection of Node objects from nodes in tokens.
    /// </summary>
    /// <param name="timestampGranularity">Timestamp granularity as defined in PrimitiveBlock.</param>
    /// <param name="positionGranularity">Granularity as defined in PrimitiveBlock.</param>
    /// <param name="latOffset">Latitude offset as defined in PrimitiveBlock.</param>
    /// <param name="lonOffset">Longitude offset as defined in PrimitiveBlock.</param>
    /// <returns>The DenseNode object with data from nodes in tokens.</returns>
    private List<PbfNode> BuildNodes(int timestampGranularity, int positionGranularity, long latOffset, long lonOffset)
    {
        var result = new List<PbfNode>(_nodesBuffer.Count);
        foreach (var node in _nodesBuffer)
        {
            var toAdd = new PbfNode
            {
                ID = node.Id,
                Latitude = (long)Math.Round((node.Latitude / 1E-09 - latOffset) / positionGranularity),
                Longitude = (long)Math.Round((node.Longitude / 1E-09 - lonOffset) / positionGranularity)
            };

            if (node.Tags.Count > 0)
            {
                toAdd.Keys = new List<uint>();
                toAdd.Values = new List<uint>();

                foreach (var tag in node.Tags)
                {
                    toAdd.Keys.Add(_nodesBuffer.GetStringIndex(tag.Key));
                    toAdd.Values.Add(_nodesBuffer.GetStringIndex(tag.Value));
                }
            }

            if (node.Metadata != null && Settings.WriteMetadata)
            {
                toAdd.Metadata = BuildInfo(node.Metadata, timestampGranularity, _nodesBuffer);
            }

            result.Add(toAdd);
        }

        return result;
    }

    /// <summary>
    /// Creates DenseNode object from nodes in tokens.
    /// </summary>
    /// <param name="timestampGranularity">Timestamp granularity as defined in PrimitiveBlock.</param>
    /// <param name="positionGranularity">Granularity as defined in PrimitiveBlock.</param>
    /// <param name="latOffset">Latitude offset as defined in PrimitiveBlock.</param>
    /// <param name="lonOffset">Longitude offset as defined in PrimitiveBlock.</param>
    /// <returns>The DenseNode object with data from nodes in tokens.</returns>
    private PbfDenseNodes BuildDenseNodes(int timestampGranularity, int positionGranularity, long latOffset, long lonOffset)
    {
        var result = new PbfDenseNodes(_nodesBuffer.Count);

        long lastID = 0;
        long lastLat = 0;
        long lastLon = 0;

        foreach (var node in _nodesBuffer)
        {
            result.Id.Add(node.Id - lastID);
            lastID = node.Id;

            var latValue = (long)Math.Round((node.Latitude / 1E-09 - latOffset) / positionGranularity);
            result.Latitude.Add(latValue - lastLat);
            lastLat = latValue;

            var lonValue = (long)Math.Round((node.Longitude / 1E-09 - lonOffset) / positionGranularity);
            result.Longitude.Add(lonValue - lastLon);
            lastLon = lonValue;

            foreach (var tag in node.Tags)
            {
                result.KeysVals.Add((int)_nodesBuffer.GetStringIndex(tag.Key));
                result.KeysVals.Add((int)_nodesBuffer.GetStringIndex(tag.Value));
            }

            result.KeysVals.Add(0);
        }

        if (Settings.WriteMetadata)
        {
            result.DenseInfo = BuildDenseInfo(timestampGranularity);
        }

        return result;
    }

    /// <summary>
    /// Creates collection of PbfWay objects from ways in tokens.
    /// </summary>
    /// <param name="timestampGranularity">Timestamp granularity as defined in PrimitiveBlock.</param>
    /// <returns>The collection of PbfWay objects created from relations in tokens.</returns>
    private List<PbfWay> BuildWays(int timestampGranularity)
    {
        var result = new List<PbfWay>();

        foreach (var way in _wayBuffer)
        {
            long lastRef = 0;

            var toAdd = new PbfWay
            {
                ID = way.Id
            };

            if (way.Nodes.Count > 0)
            {
                toAdd.Refs = new List<long>(way.Nodes.Count);

                for (var i = 0; i < way.Nodes.Count; i++)
                {
                    toAdd.Refs.Add(way.Nodes[i] - lastRef);
                    lastRef = way.Nodes[i];
                }
            }

            if (way.Tags.Count > 0)
            {
                toAdd.Keys = new List<uint>();
                toAdd.Values = new List<uint>();

                foreach (var tag in way.Tags)
                {
                    toAdd.Keys.Add(_wayBuffer.GetStringIndex(tag.Key));
                    toAdd.Values.Add(_wayBuffer.GetStringIndex(tag.Value));
                }
            }

            if (way.Metadata != null && Settings.WriteMetadata)
            {
                toAdd.Metadata = BuildInfo(way.Metadata, timestampGranularity, _wayBuffer);
            }

            result.Add(toAdd);
        }

        return result;
    }

    /// <summary>
    /// Creates collection of PbfRelation objects from relations in tokens.
    /// </summary>
    /// <param name="timestampGranularity">Timestamp granularity as defined in PrimitiveBlock.</param>
    /// <returns>The collection of PbfRelation objects created from relations in tokens.</returns>
    private List<PbfRelation> BuildRelations(int timestampGranularity)
    {
        var result = new List<PbfRelation>();

        foreach (var relation in _relationBuffer)
        {
            var toAdd = new PbfRelation
            {
                ID = relation.Id
            };

            long lastRef = 0;
            foreach (var member in relation.Members)
            {
                toAdd.MemberIds.Add(member.MemberId - lastRef);
                lastRef = member.MemberId;

                toAdd.RolesIndexes.Add((int)_relationBuffer.GetStringIndex(member.Role ?? string.Empty));
                PbfRelationMemberType memberType = 0;
                switch (member.MemberType)
                {
                    case EntityType.Node:
                        memberType = PbfRelationMemberType.Node;
                        break;
                    case EntityType.Way:
                        memberType = PbfRelationMemberType.Way;
                        break;
                    case EntityType.Relation:
                        memberType = PbfRelationMemberType.Relation;
                        break;
                }

                toAdd.Types.Add(memberType);
            }

            toAdd.Keys = new List<uint>(relation.Tags.Count);
            toAdd.Values = new List<uint>(relation.Tags.Count);

            if (relation.Tags.Count > 0)
            {
                foreach (var tag in relation.Tags)
                {
                    toAdd.Keys.Add(_relationBuffer.GetStringIndex(tag.Key));
                    toAdd.Values.Add(_relationBuffer.GetStringIndex(tag.Value));
                }
            }

            if (relation.Metadata != null && Settings.WriteMetadata)
            {
                toAdd.Metadata = BuildInfo(relation.Metadata, timestampGranularity, _relationBuffer);
            }

            result.Add(toAdd);
        }

        return result;
    }

    /// <summary>
    /// Creates a DenseInfo object with metadata for nodes in tokens.
    /// </summary>
    /// <param name="timestampGranularity">Timestamp granularity as defined in PrimitiveBlock.</param>
    /// <returns>DenseInfo object with metadata for noes in tokens.</returns>
    private PbfDenseMetadata BuildDenseInfo(int timestampGranularity)
    {
        var result = new PbfDenseMetadata(_nodesBuffer.Count);

        long lastChangeset = 0;
        long lastTimestamp = 0;
        var lastUid = 0;
        var lastUserNameIndex = 0;

        foreach (var node in _nodesBuffer)
        {
            EntityMetadata metadata = node.Metadata ?? new EntityMetadata();

            result.Changeset.Add(metadata.Changeset!.Value - lastChangeset);
            lastChangeset = metadata.Changeset!.Value;

            var timestampValue = (long)Math.Round((metadata.Timestamp - _unixEpoch).TotalMilliseconds / timestampGranularity);
            result.Timestamp.Add(timestampValue - lastTimestamp);
            lastTimestamp = timestampValue;

            result.UserId.Add(metadata.Uid!.Value - lastUid);
            lastUid = metadata.Uid!.Value;

            var userNameIndex = (int)_nodesBuffer.GetStringIndex(metadata.User!);
            result.UserNameIndex.Add(userNameIndex - lastUserNameIndex);
            lastUserNameIndex = userNameIndex;

            result.Version.Add(metadata.Version!);
            result.Visible.Add(metadata.Visible!.Value);
        }

        return result;
    }

    /// <summary>
    /// Creates an PbfMetadata object from given Metadata.
    /// </summary>
    /// <param name="metadata">Metadata object to be used as source of data.</param>
    /// <param name="timestampGranularity">Timestamp granularity as defined in PrimitiveBlock.</param>
    /// <param name="stringTableBuilder">IStringTable object to save string values to.</param>
    /// <returns>PbfMetadata object with data from specified metadata object.</returns>
    private PbfMetadata BuildInfo(EntityMetadata metadata, int timestampGranularity, IStringTableBuilder stringTableBuilder)
    {
        var result = new PbfMetadata();
        result.Changeset = metadata.Changeset;
        result.Timestamp = (long)Math.Round((metadata.Timestamp - _unixEpoch).TotalMilliseconds / timestampGranularity);
        result.UserID = metadata.Uid;
        result.UserNameIndex = (int)stringTableBuilder.GetStringIndex(metadata.User!);
        result.Version = metadata.Version;

        return result;
    }

    /// <summary>
    /// Releases the unmanaged resources used by the PbfWriter and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Flush();

                if (_output != null)
                {
                    if (_ownsOutputStream)
                    {
                        _output.Dispose();
                    }
                }
            }

            _disposed = true;
        }
    }
}
