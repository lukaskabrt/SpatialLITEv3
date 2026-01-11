using PbfLite;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents data transfer object used by PBF serializer for Ways.
/// </summary>
internal class PbfWay
{

    private List<long> _refs;

    /// <summary>
    /// Initializes a new instance of the PbfWay class with internal fields initialized to default capacity.
    /// </summary>
    public PbfWay()
    {
        _refs = new List<long>();
    }

    /// <summary>
    /// Initializes a new instance of the PbfWay class with internal fields initialized to specified capacity.
    /// </summary>
    /// <param name="capacity">The desired capacity of internal fields.</param>
    public PbfWay(int capacity)
    {
        _refs = new List<long>(capacity);
    }

    /// <summary>
    /// Gets or sets ID of the way.
    /// </summary>
    public long ID { get; set; }

    /// <summary>
    /// Gets or sets indexes of tag's keys in string table.
    /// </summary>
    public List<uint>? Keys { get; set; }

    /// <summary>
    /// Gets or sets indexes of tag's values in string table.
    /// </summary>
    public List<uint>? Values { get; set; }

    /// <summary>
    /// Gets or sets entity metadata.
    /// </summary>
    public PbfMetadata? Metadata { get; set; }

    /// <summary>
    /// Gets or sets IDs of nodes referenced by the way. This property is delta encoded.
    /// </summary>
    public List<long> Refs
    {
        get { return _refs; }
        set { _refs = value; }
    }

    public static PbfWay Deserialize(ref PbfBlockReader pbf)
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
                    pbf.ReadUIntCollection(wireType, result.Keys);
                    break;
                case 3:
                    result.Values ??= [];
                    pbf.ReadUIntCollection(wireType, result.Values);
                    break;
                case 8:
                    pbf.ReadSignedLongCollection(wireType, result.Refs);
                    break;
                case 4:
                    var metadataPbf = PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes());
                    result.Metadata = PbfMetadata.Deserialize(ref metadataPbf);
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();
        }

        return result;
    }

    public void Serialize(ref PbfBlockWriter pbf)
    {
        pbf.WriteFieldHeader(1, PbfLite.WireType.VarInt);
        pbf.WriteLong(ID);

        if (Keys != null && Keys.Count > 0)
        {
            pbf.WriteFieldHeader(2, PbfLite.WireType.String);
            pbf.WriteUIntCollection(Keys.ToArray());
        }

        if (Values != null && Values.Count > 0)
        {
            pbf.WriteFieldHeader(3, PbfLite.WireType.String);
            pbf.WriteUIntCollection(Values.ToArray());
        }

        if (Metadata != null)
        {
            pbf.WriteFieldHeader(4, PbfLite.WireType.String);
            var metadataBlock = pbf.StartLengthPrefixedBlock(64);
            Metadata.Serialize(ref pbf);
            pbf.FinalizeLengthPrefixedBlock(metadataBlock);
        }

        if (Refs.Count > 0)
        {
            pbf.WriteFieldHeader(8, PbfLite.WireType.String);
            pbf.WriteSignedLongCollection(Refs.ToArray());
        }
    }
}
