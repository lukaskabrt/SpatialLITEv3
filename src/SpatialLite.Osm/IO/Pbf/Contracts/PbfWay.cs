using PbfLite;
using ProtoBuf;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents data transfer object used by PBF serializer for Ways.
/// </summary>
[ProtoContract(Name = "Way")]
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
    [ProtoMember(1, Name = "id", IsRequired = true)]
    public long ID { get; set; }

    /// <summary>
    /// Gets or sets indexes of tag's keys in string table.
    /// </summary>
    [ProtoMember(2, Name = "keys", Options = MemberSerializationOptions.Packed)]
    public List<uint>? Keys { get; set; }

    /// <summary>
    /// Gets or sets indexes of tag's values in string table.
    /// </summary>
    [ProtoMember(3, Name = "vals", Options = MemberSerializationOptions.Packed)]
    public List<uint>? Values { get; set; }

    /// <summary>
    /// Gets or sets entity metadata.
    /// </summary>
    [ProtoMember(4, Name = "info", IsRequired = false)]
    public PbfMetadata? Metadata { get; set; }

    /// <summary>
    /// Gets or sets IDs of nodes referenced by the way. This property is delta encoded.
    /// </summary>
    [ProtoMember(8, Name = "refs", Options = MemberSerializationOptions.Packed, DataFormat = DataFormat.ZigZag)]
    public List<long> Refs
    {
        get { return _refs; }
        set { _refs = value; }
    }


    public static PbfWay Deserialize(PbfBlockReader pbf)
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
                    result.Metadata = PbfMetadata.Deserialize(PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes()));
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
