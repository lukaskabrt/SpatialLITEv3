using PbfLite;
using ProtoBuf;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents data transfer object used by PBF serializer for relations.
/// </summary>
[ProtoContract(Name = "Relation")]
internal class PbfRelation
{

    private List<long> _memberIds;
    private List<uint> _rolesIndexes;
    private List<PbfRelationMemberType> _types;

    /// <summary>
    /// Initializes a new instance of the PbfRelation class with internal fields initialized to default capacity.
    /// </summary>
    public PbfRelation()
    {
        _memberIds = new List<long>();
        _rolesIndexes = new List<uint>();
        _types = new List<PbfRelationMemberType>();
    }

    /// <summary>
    /// Initializes a new instance of the PbfRelation class with internal fields initialized to specified capacity.
    /// </summary>
    /// <param name="capacity">The desired capacity of internal fields.</param>
    public PbfRelation(int capacity)
    {
        _memberIds = new List<long>(capacity);
        _rolesIndexes = new List<uint>(capacity);
        _types = new List<PbfRelationMemberType>(capacity);
    }

    /// <summary>
    /// Gets or sets ID of the relation.
    /// </summary>
    [ProtoMember(1, Name = "id", IsRequired = true)]
    public long ID { get; set; }

    /// <summary>
    /// Gets or sets relation metadata.
    /// </summary>
    [ProtoMember(4, Name = "info", IsRequired = false)]
    public PbfMetadata? Metadata { get; set; }

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
    /// Gets or sets IDs of the relation members. This property is delta encoded.
    /// </summary>
    [ProtoMember(9, Name = "memids", Options = MemberSerializationOptions.Packed, DataFormat = DataFormat.ZigZag)]
    public List<long> MemberIds
    {
        get { return _memberIds; }
        set { _memberIds = value; }
    }

    /// <summary>
    /// Gets or sets index of the role in string table for appropriate members.
    /// </summary>
    [ProtoMember(8, Name = "roles_sid", Options = MemberSerializationOptions.Packed)]
    public List<uint> RolesIndexes
    {
        get { return _rolesIndexes; }
        set { _rolesIndexes = value; }
    }

    /// <summary>
    /// Gets or sets type of the relation members.
    /// </summary>
    [ProtoMember(10, Name = "types", Options = MemberSerializationOptions.Packed)]
    public List<PbfRelationMemberType> Types
    {
        get { return _types; }
        set { _types = value; }
    }

    public static PbfRelation Deserialize(PbfBlockReader pbf)
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
                    pbf.ReadUIntCollection(wireType, result.Keys);
                    break;
                case 3:
                    result.Values ??= [];
                    pbf.ReadUIntCollection(wireType, result.Values);
                    break;
                case 8:
                    pbf.ReadUIntCollection(wireType, result.RolesIndexes);
                    break;
                case 9:
                    pbf.ReadSignedLongCollection(wireType, result.MemberIds);
                    break;
                case 10:
                    ReadRelationMemberTypeList(ref pbf, wireType, result.Types);
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

    private static void ReadRelationMemberTypeList(ref PbfBlockReader pbf, PbfLite.WireType wireType, List<PbfRelationMemberType> list)
    {
        if (wireType == PbfLite.WireType.String)
        {
            uint byteLength = pbf.ReadVarInt32();
            var endPosition = pbf.Position + byteLength;
            while (pbf.Position < endPosition)
            {
                list.Add((PbfRelationMemberType)pbf.ReadUint());
            }
        }
        else if (wireType == PbfLite.WireType.VarInt)
        {
            list.Add((PbfRelationMemberType)pbf.ReadUint());
        }
    }
}
