//using PbfLite;
using PbfLite;
using ProtoBuf;
using SpatialLite.Contracts;
using SpatialLite.Osm.IO.Pbf.Contracts;
using System.Buffers;
using System.IO.Compression;
//using System.Buffers;

namespace SpatialLite.Osm.IO.Pbf;

/// <summary>
/// Represents IOsmReader that can read OSM entities from PBF format.
/// </summary>
public class PbfReader : IOsmReader
{
    static PbfReader()
    {
        Serializer.PrepareSerializer<Blob>();
        Serializer.PrepareSerializer<BlobHeader>();
        Serializer.PrepareSerializer<HeaderBBox>();
        Serializer.PrepareSerializer<OsmHeader>();
        Serializer.PrepareSerializer<PbfDenseMetadata>();
        Serializer.PrepareSerializer<PbfDenseNodes>();
        Serializer.PrepareSerializer<PbfChangeset>();
        Serializer.PrepareSerializer<PbfMetadata>();
        Serializer.PrepareSerializer<PbfNode>();
        Serializer.PrepareSerializer<PbfRelation>();
        Serializer.PrepareSerializer<PbfWay>();
        Serializer.PrepareSerializer<PrimitiveBlock>();
        Serializer.PrepareSerializer<PrimitiveGroup>();
        Serializer.PrepareSerializer<StringTable>();
    }

    /// <summary>
    /// Defines maximal allowed size of uncompressed OsmData block. Larger blocks are considered invalid.
    /// </summary>
    public const int MaxDataBlockSize = 32 * 1024 * 1024;

    /// <summary>
    /// Defines maximal allowed size of uncompressed OsmHeader block. Larger blocks are considered invalid.
    /// </summary>
    public const int MaxHeaderBlockSize = 64 * 1024;

    private bool _disposed = false;
    private readonly Stream _input;
    private readonly Queue<IOsmEntity> _cache;
    private readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Initializes a new instance of the PbfReader class that read data form specified stream.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <param name="settings">The OsmReaderSettings object that determines behaviour of PbfReader.</param>
    public PbfReader(Stream input, OsmReaderSettings settings)
    {
        _input = input;
        _cache = new Queue<IOsmEntity>();

        Settings = settings;

        BlobHeader? blobHeader;
        while ((blobHeader = ReadBlobHeader()) != null)
        {
            try
            {
                if (blobHeader.Type == "OSMHeader")
                {
                    var osmHeader = (OsmHeader?)ReadBlob(blobHeader);
                    ProcessOsmHeader(osmHeader);
                    return;
                }
                else if (blobHeader.Type == "OSMData")
                {
                    throw new InvalidDataException("Input stream doesn't contain an 'OSMHeader' block before 'OSMData' block.");
                }
                else
                {
                    _input.Seek(blobHeader.DataSize, SeekOrigin.Current);
                }
            }
            catch (ProtoException ex)
            {
                throw new InvalidDataException("Input stream contains unsupported data", ex);
            }
        }

        throw new InvalidDataException("Input stream doesn't contain an 'OSMHeader' block.");
    }

    /// <summary>
    /// Initializes a new instance of the PbfReader class that read data form specified file.
    /// </summary>
    /// <param name="path">The path to the input file.</param>
    /// <param name="settings">The OsmReaderSettings object that determines behaviour of PbfReader.</param>
    public PbfReader(string path, OsmReaderSettings settings)
    {
        _input = new FileStream(path, FileMode.Open, FileAccess.Read);
        _cache = new Queue<IOsmEntity>();

        Settings = settings;

        BlobHeader? blobHeader;
        while ((blobHeader = ReadBlobHeader()) != null)
        {
            try
            {
                if (blobHeader.Type == "OSMHeader")
                {
                    var osmHeader = (OsmHeader?)ReadBlob(blobHeader);
                    ProcessOsmHeader(osmHeader);
                    return;
                }
                else if (blobHeader.Type == "OSMData")
                {
                    throw new InvalidDataException("Input stream doesn't contain an 'OSMHeader' block before 'OSMData' block.");
                }
                else
                {
                    _input.Seek(blobHeader.DataSize, SeekOrigin.Current);
                }
            }
            catch (ProtoException ex)
            {
                throw new InvalidDataException("Input stream contains unsupported data", ex);
            }
        }

        throw new InvalidDataException("Input stream doesn't contain an 'OSMHeader' block.");
    }

    /// <summary>
    /// Gets OsmReaderSettings object that contains properties which determine behaviour of the OSM reader.
    /// </summary>
    public OsmReaderSettings Settings { get; private set; }

    /// <summary>
    /// Reads next OSM entity from the stream.
    /// </summary>
    /// <returns>Parsed OSM entity from the stream or null if no other entity is available.</returns>
    public IOsmEntity? Read()
    {
        if (_cache.Count > 0)
        {
            return _cache.Dequeue();
        }
        else
        {
            BlobHeader? blobHeader;
            while (_cache.Count == 0 && (blobHeader = ReadBlobHeader()) != null)
            {
                if (ReadBlob(blobHeader) is PrimitiveBlock data)
                {
                    foreach (PrimitiveGroup group in data.PrimitiveGroup)
                    {
                        ProcessPrimitiveGroup(data, group);
                    }
                }
            }
        }

        if (_cache.Count > 0)
        {
            return _cache.Dequeue();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Releases all resources used by the PbfReader.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Reads and deserializes a BlobHeader from input stream.
    /// </summary>
    /// <returns>Deserialized BlobHeader object or null if end of the stream is reached.</returns>
    private BlobHeader? ReadBlobHeader()
    {
        if (_input.Position < _input.Length)
        {
            return Serializer.DeserializeWithLengthPrefix<BlobHeader>(_input, PrefixStyle.Fixed32BigEndian);
        }

        return null;
    }

    /// <summary>
    /// Read blob and deserializes its content.
    /// </summary>
    /// <param name="header">Header of the blob.</param>
    /// <returns>Deserialized content of the read blob or null if blob contains unknown data.</returns>
    private object? ReadBlob(BlobHeader header)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(header.DataSize);
        _input.ReadExactly(buffer, 0, header.DataSize);

        var pbf = PbfBlock.Create(buffer.AsSpan(0, header.DataSize));
        var blob = DeserializeBlob(pbf);

        byte[] blobContent;
        if (blob.Raw != null)
        {
            blobContent = blob.Raw;
        }
        else if (blob.ZlibData != null)
        {
            using var deflateStreamData = new MemoryStream(blob.ZlibData);
            using var blobContentStream = new System.IO.Compression.ZLibStream(deflateStreamData, System.IO.Compression.CompressionMode.Decompress);
            blobContent = ReadAllBytes(blobContentStream);
        }
        else
        {
            throw new NotSupportedException();
        }

        if (header.Type.Equals("OSMData", StringComparison.OrdinalIgnoreCase))
        {
            if ((blob.RawSize > MaxDataBlockSize) || (blob.RawSize.HasValue == false && blobContent.Length > MaxDataBlockSize))
            {
                throw new InvalidDataException("Invalid OSMData block");
            }

            return DeserializePrimitiveBlock(PbfBlock.Create(blobContent));
        }
        else if (header.Type.Equals("OSMHeader", StringComparison.OrdinalIgnoreCase))
        {
            if ((blob.RawSize.HasValue && blob.RawSize > MaxHeaderBlockSize) || (blob.RawSize.HasValue == false && blobContent.Length > MaxHeaderBlockSize))
            {
                throw new InvalidDataException("Invalid OSMHeader block");
            }

            try
            {
                return DeserializeOsmHeader(PbfBlock.Create(blobContent));
            }
            catch (ProtoException ex)
            {
                throw new InvalidDataException("Invalid OSMData block", ex);
            }
        }
        else
        {
            return null;
        }
    }

    private byte[] ReadAllBytes(ZLibStream blobContentStream)
    {
        using var memoryStream = new MemoryStream();
        blobContentStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    private Blob DeserializeBlob(PbfBlock pbf)
    {
        var result = new Blob();

        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.Raw = pbf.ReadLengthPrefixedBytes().ToArray();
                    break;
                case 2:
                    result.RawSize = pbf.ReadInt();
                    break;
                case 3:
                    result.ZlibData = pbf.ReadLengthPrefixedBytes().ToArray();
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();
        }

        return result;
    }

    private OsmHeader DeserializeOsmHeader(PbfBlock pbf)
    {
        var result = new OsmHeader();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.BBox = DeserializeBBox(PbfBlock.Create(pbf.ReadLengthPrefixedBytes()));
                    break;
                case 4:
                    result.RequiredFeatures.Add(pbf.ReadString());
                    break;
                case 5:
                    result.OptionalFeatures.Add(pbf.ReadString());
                    break;
                case 16:
                    result.Source = pbf.ReadString();
                    break;
                case 17:
                    result.WritingProgram = pbf.ReadString();
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();
        }

        return result;
    }

    private HeaderBBox DeserializeBBox(PbfBlock pbf)
    {
        var result = new HeaderBBox();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.Left = PbfBlock.Zag(pbf.ReadULongint());
                    break;
                case 2:
                    result.Right = PbfBlock.Zag(pbf.ReadULongint());
                    break;
                case 3:
                    result.Top = PbfBlock.Zag(pbf.ReadULongint());
                    break;
                case 4:
                    result.Bottom = PbfBlock.Zag(pbf.ReadULongint());
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();
        }

        return result;
    }

    private StringTable DeserializeStringTable(PbfBlock pbf)
    {
        var result = new StringTable();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.Storage.Add(pbf.ReadLengthPrefixedBytes().ToArray());
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();

        }

        return result;
    }

    private PrimitiveBlock DeserializePrimitiveBlock(PbfBlock pbf)
    {
        var result = new PrimitiveBlock();

        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.StringTable = DeserializeStringTable(PbfBlock.Create(pbf.ReadLengthPrefixedBytes()));
                    break;
                case 2:
                {
                    result.PrimitiveGroup.Add(DeserializePrimitiveGroup(PbfBlock.Create(pbf.ReadLengthPrefixedBytes())));
                }

                break;
                case 16:
                    result.Granularity = pbf.ReadInt();
                    break;
                case 18:
                    result.DateGranularity = pbf.ReadInt();
                    break;
                case 19:
                    result.LatOffset = pbf.ReadLong();
                    break;
                case 20:
                    result.LonOffset = pbf.ReadLong();
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();
        }

        return result;
    }

    /// <summary>
    /// Gets or sets Bottom boundary of the BBox.
    /// </summary>
    [ProtoMember(4, Name = "bottom", IsRequired = true, DataFormat = DataFormat.ZigZag)]
    public long Bottom { get; set; }

    /// <summary>
    /// Gets or sets Left boundary of the BBox.
    /// </summary>
    [ProtoMember(1, Name = "left", IsRequired = true, DataFormat = DataFormat.ZigZag)]
    public long Left { get; set; }

    /// <summary>
    /// Gets or sets Right boundary of the BBox.
    /// </summary>
    [ProtoMember(2, Name = "right", IsRequired = true, DataFormat = DataFormat.ZigZag)]
    public long Right { get; set; }

    /// <summary>
    /// Gets or sets Top boundary of the BBox.
    /// </summary>
    [ProtoMember(3, Name = "top", IsRequired = true, DataFormat = DataFormat.ZigZag)]
    public long Top { get; set; }

    /// <summary>
    /// Checks OsmHeader required features and if any of required features isn't supported, NotSupportedException is thrown.
    /// </summary>
    /// <param name="header">OsmHeader object to process.</param>
    private void ProcessOsmHeader(OsmHeader? header)
    {
        if (header == null)
        {
            throw new ArgumentNullException(nameof(header), "Header cannot be null.");
        }

        var supportedFeatures = new string[] { "OsmSchema-V0.6", "DenseNodes" };
        foreach (var required in header.RequiredFeatures)
        {
            if (supportedFeatures.Contains(required) == false)
            {
                throw new InvalidDataException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "Processing specified PBF file requires '{0}' feature which isn't supported by PbfReader.", required));
            }
        }
    }

    /// <summary>
    /// Processes OSM entities in Primitive group.
    /// </summary>
    /// <param name="block">The PrimitiveBlock that contains specified PrimitiveGroup.</param>
    /// <param name="group">The PrimitiveGroup to process.</param>
    private void ProcessPrimitiveGroup(PrimitiveBlock block, PrimitiveGroup group)
    {
        ProcessNodes(block, group);
        ProcessDenseNodes(block, group);
        ProcessWays(block, group);
        ProcessRelations(block, group);
    }

    /// <summary>
    /// Processes all nodes in non-dense format from the PrimitiveGroup and adds them to the output queue.
    /// </summary>
    /// <param name="block">The PrimitiveBlock that contains specified PrimitiveGroup.</param>
    /// <param name="group">The PrimitiveGroup with nodes to process.</param>
    private void ProcessNodes(PrimitiveBlock block, PrimitiveGroup group)
    {
        if (group.Nodes == null)
        {
            return;
        }

        foreach (PbfNode node in group.Nodes)
        {
            double lat = 1E-09 * (block.LatOffset + block.Granularity * node.Latitude);
            double lon = 1E-09 * (block.LonOffset + block.Granularity * node.Longitude);

            var tags = new TagsCollection(node.Keys?.Count ?? 0);
            if (node.Keys != null)
            {
                for (var i = 0; i < node.Keys.Count; i++)
                {
                    tags.Add(block.StringTable[node.Keys[i]], block.StringTable[node.Values![i]]);
                }
            }

            var metadata = ProcessMetadata(node.Metadata, block);

            var parsed = new Node { Id = node.ID, Position = new Coordinate(lon, lat), Tags = tags, Metadata = metadata };
            _cache.Enqueue(parsed);
        }
    }

    /// <summary>
    /// Processes all nodes in dense format from the PrimitiveGroup and adds them to the output queue.
    /// </summary>
    /// <param name="block">The PrimitiveBlock that contains specified PrimitiveGroup.</param>
    /// <param name="group">The PrimitiveGroup with nodes to process.</param>
    private void ProcessDenseNodes(PrimitiveBlock block, PrimitiveGroup group)
    {
        if (group.DenseNodes == null)
        {
            return;
        }

        long idStore = 0;
        long latStore = 0;
        long lonStore = 0;

        var keyValueIndex = 0;

        long timestampStore = 0;
        long changesetStore = 0;
        var userIdStore = 0;
        var usernameIdStore = 0;

        for (var i = 0; i < group.DenseNodes.Id.Count; i++)
        {
            idStore += group.DenseNodes.Id[i];
            lonStore += group.DenseNodes.Longitude[i];
            latStore += group.DenseNodes.Latitude[i];

            double lat = 1E-09 * (block.LatOffset + block.Granularity * latStore);
            double lon = 1E-09 * (block.LonOffset + block.Granularity * lonStore);

            var tags = new TagsCollection();
            if (group.DenseNodes.KeysVals.Count > 0)
            {
                while (group.DenseNodes.KeysVals[keyValueIndex] != 0)
                {
                    string key = block.StringTable[group.DenseNodes.KeysVals[keyValueIndex++]];
                    string value = block.StringTable[group.DenseNodes.KeysVals[keyValueIndex++]];

                    tags.Add(key, value);
                }

                //Skip '0' used as delimiter
                keyValueIndex++;
            }

            EntityMetadata? metadata = null;
            if (Settings.ReadMetadata && group.DenseNodes.DenseInfo != null)
            {
                timestampStore += group.DenseNodes.DenseInfo.Timestamp[i];
                changesetStore += group.DenseNodes.DenseInfo.Changeset[i];
                userIdStore += group.DenseNodes.DenseInfo.UserId[i];
                usernameIdStore += group.DenseNodes.DenseInfo.UserNameIndex[i];

                metadata = new EntityMetadata()
                {
                    Changeset = (int)changesetStore,
                    Timestamp = _unixEpoch.AddMilliseconds(timestampStore * block.DateGranularity),
                    Uid = userIdStore,
                    User = block.StringTable[usernameIdStore],
                    Version = group.DenseNodes.DenseInfo.Version[i],
                    Visible = true
                };

                if (group.DenseNodes.DenseInfo.Visible.Count > 0)
                {
                    metadata.Visible = group.DenseNodes.DenseInfo.Visible[i];
                }
            }

            var parsed = new Node { Id = idStore, Position = new Coordinate(lon, lat), Tags = tags, Metadata = metadata };
            _cache.Enqueue(parsed);
        }
    }

    /// <summary>
    /// Processes all ways from the PrimitiveGroup and adds them to the output queue.
    /// </summary>
    /// <param name="block">The PrimitiveBlock that contains specified PrimitiveGroup.</param>
    /// <param name="group">The PrimitiveGroup with nodes to process.</param>
    private void ProcessWays(PrimitiveBlock block, PrimitiveGroup group)
    {
        if (group.Ways == null)
        {
            return;
        }

        foreach (var way in group.Ways)
        {
            long refStore = 0;
            var refs = new List<long>(way.Refs.Count);

            for (var i = 0; i < way.Refs.Count; i++)
            {
                refStore += way.Refs[i];
                refs.Add(refStore);
            }

            var tags = new TagsCollection(way.Keys?.Count ?? 0);
            if (way.Keys != null)
            {
                for (var i = 0; i < way.Keys.Count; i++)
                {
                    tags.Add(block.StringTable[way.Keys[i]], block.StringTable[way.Values![i]]);
                }
            }

            var metadata = ProcessMetadata(way.Metadata, block);

            var parsed = new Way { Id = way.ID, Tags = tags, Nodes = refs, Metadata = metadata };
            _cache.Enqueue(parsed);
        }
    }

    /// <summary>
    /// Processes all relations from the PrimitiveGroup and adds them to the output queue.
    /// </summary>
    /// <param name="block">The PrimitiveBlock that contains specified PrimitiveGroup.</param>
    /// <param name="group">The PrimitiveGroup with nodes to process.</param>
    private void ProcessRelations(PrimitiveBlock block, PrimitiveGroup group)
    {
        if (group.Relations == null)
        {
            return;
        }

        foreach (var relation in group.Relations)
        {
            long memberRefStore = 0;

            var members = new List<RelationMember>();
            for (var i = 0; i < relation.MemberIds.Count; i++)
            {
                memberRefStore += relation.MemberIds[i];
                string role = block.StringTable[relation.RolesIndexes[i]];

                EntityType memberType = 0;
                switch (relation.Types[i])
                {
                    case PbfRelationMemberType.Node:
                        memberType = EntityType.Node;
                        break;
                    case PbfRelationMemberType.Way:
                        memberType = EntityType.Way;
                        break;
                    case PbfRelationMemberType.Relation:
                        memberType = EntityType.Relation;
                        break;
                }

                members.Add(new RelationMember() { MemberType = memberType, MemberId = memberRefStore, Role = role });
            }

            var tags = new TagsCollection(relation.Keys?.Count ?? 0);
            if (relation.Keys != null)
            {
                for (var i = 0; i < relation.Keys.Count; i++)
                {
                    tags.Add(block.StringTable[relation.Keys[i]], block.StringTable[relation.Values![i]]);
                }
            }

            var metadata = ProcessMetadata(relation.Metadata, block);

            var parsed = new Relation { Id = relation.ID, Tags = tags, Members = members, Metadata = metadata };
            _cache.Enqueue(parsed);
        }
    }

    /// <summary>
    /// Processes entity metadata.
    /// </summary>
    /// <param name="serializedMetadata">Serialized metadata.</param>
    /// <param name="block">PrimitiveBlock that contains metadata being processed.</param>
    /// <returns>Parsed metadata.</returns>
    private EntityMetadata? ProcessMetadata(PbfMetadata? serializedMetadata, PrimitiveBlock block)
    {
        EntityMetadata? metadata = null;

        if (Settings.ReadMetadata && serializedMetadata != null)
        {
            //PBF has no field for 'visible' property, true is default value for OSM entity read from PBF file
            metadata = new EntityMetadata() { Visible = true };
            if (serializedMetadata.Changeset.HasValue)
            {
                metadata.Changeset = (int)serializedMetadata.Changeset.Value;
            }

            if (serializedMetadata.Timestamp.HasValue)
            {
                metadata.Timestamp = _unixEpoch.AddMilliseconds(serializedMetadata.Timestamp.Value * block.DateGranularity);
            }

            if (serializedMetadata.UserID.HasValue)
            {
                metadata.Uid = serializedMetadata.UserID.Value;
            }

            if (serializedMetadata.UserNameIndex.HasValue)
            {
                metadata.User = block.StringTable[serializedMetadata.UserNameIndex.Value];
            }

            if (serializedMetadata.Version.HasValue)
            {
                metadata.Version = serializedMetadata.Version.Value;
            }
        }

        return metadata;
    }

    /// <summary>
    /// Releases the unmanaged resources used by the PbfReader and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _input?.Dispose();
            }

            _disposed = true;
        }
    }

    private PrimitiveGroup DeserializePrimitiveGroup(PbfBlock pbf)
    {
        var result = new PrimitiveGroup();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.Nodes ??= [];
                    result.Nodes.Add(DeserializePbfNode(PbfBlock.Create(pbf.ReadLengthPrefixedBytes())));
                    break;
                case 2:
                    result.DenseNodes = DeserializePbfDenseNodes(PbfBlock.Create(pbf.ReadLengthPrefixedBytes()));
                    break;
                case 3:
                    result.Ways ??= [];
                    result.Ways.Add(DeserializePbfWay(PbfBlock.Create(pbf.ReadLengthPrefixedBytes())));
                    break;
                case 4:
                    result.Relations ??= [];
                    result.Relations.Add(DeserializePbfRelation(PbfBlock.Create(pbf.ReadLengthPrefixedBytes())));
                    break;
                case 5:
                    result.Changesets ??= [];

                    result.Changesets.Add(DeserializePbfChangeset(PbfBlock.Create(pbf.ReadLengthPrefixedBytes())));
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();
        }

        return result;
    }

    private PbfNode DeserializePbfNode(PbfBlock pbf)
    {
        var result = new PbfNode();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.ID = pbf.ReadSignedLong();
                    break;
                case 8:
                    result.Latitude = pbf.ReadSignedLong();
                    break;
                case 9:
                    result.Longitude = pbf.ReadSignedLong();
                    break;
                case 2:
                    result.Keys ??= [];
                    ReadUintList(ref pbf, wireType, result.Keys);

                    break;
                case 3:
                    result.Values ??= [];
                    ReadUintList(ref pbf, wireType, result.Values);

                    break;
                case 4:
                    result.Metadata = DeserializePbfMetadata(PbfBlock.Create(pbf.ReadLengthPrefixedBytes()));
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();
        }

        return result;
    }

    private void ReadUintList(ref PbfBlock pbf, PbfLite.WireType wireType, List<uint> list)
    {
        if (wireType == PbfLite.WireType.String)
        {
            uint byteLength = pbf.ReadVarint32();
            var endPosition = pbf.Position + byteLength;
            while (pbf.Position < endPosition)
            {
                list.Add(pbf.ReadUint());
            }
        }
        else if (wireType == PbfLite.WireType.Variant)
        {
            list.Add(pbf.ReadUint());
        }
    }

    private void ReadLongList(ref PbfBlock pbf, PbfLite.WireType wireType, List<long> list)
    {
        if (wireType == PbfLite.WireType.String)
        {
            uint byteLength = pbf.ReadVarint32();
            var endPosition = pbf.Position + byteLength;
            while (pbf.Position < endPosition)
            {
                list.Add(pbf.ReadSignedLong());
            }
        }
        else if (wireType == PbfLite.WireType.Variant)
        {
            list.Add(pbf.ReadSignedLong());
        }
    }

    private void ReadRelationMemberTypeList(ref PbfBlock pbf, PbfLite.WireType wireType, List<PbfRelationMemberType> list)
    {
        if (wireType == PbfLite.WireType.String)
        {
            uint byteLength = pbf.ReadVarint32();
            var endPosition = pbf.Position + byteLength;
            while (pbf.Position < endPosition)
            {
                list.Add((PbfRelationMemberType)pbf.ReadUint());
            }
        }
        else if (wireType == PbfLite.WireType.Variant)
        {
            list.Add((PbfRelationMemberType)pbf.ReadUint());
        }
    }

    private void ReadIntList(ref PbfBlock pbf, PbfLite.WireType wireType, List<int> list)
    {
        if (wireType == PbfLite.WireType.String)
        {
            uint byteLength = pbf.ReadVarint32();
            var endPosition = pbf.Position + byteLength;
            while (pbf.Position < endPosition)
            {
                list.Add(pbf.ReadInt());
            }
        }
        else if (wireType == PbfLite.WireType.Variant)
        {
            list.Add(pbf.ReadInt());
        }
    }

    private void ReadBoolList(ref PbfBlock pbf, PbfLite.WireType wireType, List<bool> list)
    {
        if (wireType == PbfLite.WireType.String)
        {
            uint byteLength = pbf.ReadVarint32();
            var endPosition = pbf.Position + byteLength;
            while (pbf.Position < endPosition)
            {
                list.Add(pbf.ReadInt() != 0);
            }
        }
        else if (wireType == PbfLite.WireType.Variant)
        {
            list.Add(pbf.ReadInt() != 0);
        }
    }

    private void ReadSignedLongList(ref PbfBlock pbf, PbfLite.WireType wireType, List<long> list)
    {
        if (wireType == PbfLite.WireType.String)
        {
            uint byteLength = pbf.ReadVarint32();
            var endPosition = pbf.Position + byteLength;
            while (pbf.Position < endPosition)
            {
                list.Add(pbf.ReadSignedLong());
            }
        }
        else if (wireType == PbfLite.WireType.Variant)
        {
            list.Add(pbf.ReadSignedLong());
        }
    }

    private void ReadSignedIntList(ref PbfBlock pbf, PbfLite.WireType wireType, List<int> list)
    {
        if (wireType == PbfLite.WireType.String)
        {
            uint byteLength = pbf.ReadVarint32();
            var endPosition = pbf.Position + byteLength;
            while (pbf.Position < endPosition)
            {
                list.Add(pbf.ReadSignedInt());
            }
        }
        else if (wireType == PbfLite.WireType.Variant)
        {
            list.Add(pbf.ReadSignedInt());
        }
    }

    private PbfDenseNodes DeserializePbfDenseNodes(PbfBlock pbf)
    {
        var result = new PbfDenseNodes();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    ReadLongList(ref pbf, wireType, result.Id);
                    break;
                case 8:
                    ReadLongList(ref pbf, wireType, result.Latitude);
                    break;
                case 9:
                    ReadLongList(ref pbf, wireType, result.Longitude);
                    break;
                case 10:
                    ReadUintList(ref pbf, wireType, result.KeysVals);
                    break;
                case 5:
                    result.DenseInfo = DeserializePbfDenseMetadata(PbfBlock.Create(pbf.ReadLengthPrefixedBytes()));
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();
        }

        return result;
    }

    private PbfWay DeserializePbfWay(PbfBlock pbf)
    {
        var result = new PbfWay();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.ID = pbf.ReadLong();
                    break;
                case 2:
                    result.Keys ??= [];
                    ReadUintList(ref pbf, wireType, result.Keys);
                    break;
                case 3:
                    result.Values ??= [];
                    ReadUintList(ref pbf, wireType, result.Values);
                    break;
                case 8:
                    ReadLongList(ref pbf, wireType, result.Refs);
                    break;
                case 4:
                    result.Metadata = DeserializePbfMetadata(PbfBlock.Create(pbf.ReadLengthPrefixedBytes()));
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();
        }

        return result;
    }

    private PbfRelation DeserializePbfRelation(PbfBlock pbf)
    {
        var result = new PbfRelation();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.ID = pbf.ReadLong();
                    break;
                case 2:
                    result.Keys ??= [];
                    ReadUintList(ref pbf, wireType, result.Keys);
                    break;
                case 3:
                    result.Values ??= [];
                    ReadUintList(ref pbf, wireType, result.Values);
                    break;
                case 8:
                    ReadUintList(ref pbf, wireType, result.RolesIndexes);
                    break;
                case 9:
                    ReadLongList(ref pbf, wireType, result.MemberIds);
                    break;
                case 10:
                    ReadRelationMemberTypeList(ref pbf, wireType, result.Types);
                    break;
                case 4:
                    result.Metadata = DeserializePbfMetadata(PbfBlock.Create(pbf.ReadLengthPrefixedBytes()));
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();
        }

        return result;
    }

    private PbfChangeset DeserializePbfChangeset(PbfBlock pbf)
    {
        var result = new PbfChangeset();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.ID = pbf.ReadLong();
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();
        }

        return result;
    }

    private PbfMetadata DeserializePbfMetadata(PbfBlock pbf)
    {
        var result = new PbfMetadata();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.Version = pbf.ReadInt();
                    break;
                case 2:
                    result.Timestamp = pbf.ReadLong();
                    break;
                case 3:
                    result.Changeset = pbf.ReadLong();
                    break;
                case 4:
                    result.UserID = pbf.ReadInt();
                    break;
                case 5:
                    result.UserNameIndex = pbf.ReadInt();
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();
        }

        return result;
    }

    private PbfDenseMetadata DeserializePbfDenseMetadata(PbfBlock pbf)
    {
        var result = new PbfDenseMetadata();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1: // version - not ZigZag encoded
                    ReadIntList(ref pbf, wireType, (List<int>)result.Version);
                    break;
                case 2: // timestamp - ZigZag encoded
                    ReadSignedLongList(ref pbf, wireType, (List<long>)result.Timestamp);
                    break;
                case 3: // changeset - ZigZag encoded
                    ReadSignedLongList(ref pbf, wireType, (List<long>)result.Changeset);
                    break;
                case 4: // uid - ZigZag encoded
                    ReadSignedIntList(ref pbf, wireType, (List<int>)result.UserId);
                    break;
                case 5: // user_sid - ZigZag encoded
                    ReadSignedIntList(ref pbf, wireType, (List<int>)result.UserNameIndex);
                    break;
                case 6: // visible - not ZigZag encoded
                    ReadBoolList(ref pbf, wireType, (List<bool>)result.Visible);
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();
        }

        return result;
    }
}
