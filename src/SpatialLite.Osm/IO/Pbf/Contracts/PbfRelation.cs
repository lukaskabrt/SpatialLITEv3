using PbfLite;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents data transfer object used by PBF serializer for relations.
/// </summary>
internal class PbfRelation
{
    /// <summary>
    /// Gets or sets ID of the relation.
    /// </summary>
    public long ID { get; set; }

    /// <summary>
    /// Gets or sets relation metadata.
    /// </summary>
    public PbfMetadata? Metadata { get; set; }

    /// <summary>
    /// Gets or sets indexes of tag's keys in string table.
    /// </summary>
    public List<uint>? Keys { get; set; }

    /// <summary>
    /// Gets or sets indexes of tag's values in string table.
    /// </summary>
    public List<uint>? Values { get; set; }

    /// <summary>
    /// Gets or sets IDs of the relation members. This property is delta encoded.
    /// </summary>
    public List<long> MemberIds { get; set; } = [];

    /// <summary>
    /// Gets or sets index of the role in string table for appropriate members.
    /// </summary>
    public List<uint> RolesIndexes { get; set; } = [];

    /// <summary>
    /// Gets or sets type of the relation members.
    /// </summary>
    public List<PbfRelationMemberType> Types { get; set; } = [];
    /// <summary>
    /// Serializes the relation to a PBF block writer.
    /// </summary>
    /// <param name="pbf">The PBF block writer to serialize to.</param>
    public void Serialize(ref PbfBlockWriter pbf)
    {
        pbf.WriteFieldHeader(1, WireType.VarInt);
        pbf.WriteLong(ID);

        if (Keys != null && Keys.Count > 0)
        {
            pbf.WriteFieldHeader(2, WireType.String);
            pbf.WriteUIntCollection(Keys);
        }

        if (Values != null && Values.Count > 0)
        {
            pbf.WriteFieldHeader(3, WireType.String);
            pbf.WriteUIntCollection(Values);
        }

        if (Metadata != null)
        {
            pbf.WriteFieldHeader(4, WireType.String);
            var metadataBlock = pbf.StartLengthPrefixedBlock(64);
            Metadata.Serialize(ref pbf);
            pbf.FinalizeLengthPrefixedBlock(metadataBlock);
        }

        if (RolesIndexes.Count > 0)
        {
            pbf.WriteFieldHeader(8, WireType.String);
            pbf.WriteUIntCollection(RolesIndexes);
        }

        if (MemberIds.Count > 0)
        {
            pbf.WriteFieldHeader(9, WireType.String);
            pbf.WriteSignedLongCollection(MemberIds);
        }

        if (Types.Count > 0)
        {
            pbf.WriteFieldHeader(10, WireType.String);
            var typesBlock = pbf.StartLengthPrefixedBlock(Types.Count);
            foreach (var type in Types)
            {
                pbf.WriteUint((uint)type);
            }

            pbf.FinalizeLengthPrefixedBlock(typesBlock);
        }
    }

    /// <summary>
    /// Deserializes a relation from a PBF block reader.
    /// </summary>
    /// <param name="pbf">The PBF block reader to deserialize from.</param>
    /// <returns>A new PbfRelation instance containing the deserialized relation data.</returns>
    public static PbfRelation Deserialize(ref PbfBlockReader pbf)
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

    private static void ReadRelationMemberTypeList(ref PbfBlockReader pbf, PbfLite.WireType wireType, List<PbfRelationMemberType> list)
    {
        if (wireType == WireType.String)
        {
            uint byteLength = pbf.ReadVarInt32();
            var endPosition = pbf.Position + byteLength;
            while (pbf.Position < endPosition)
            {
                list.Add((PbfRelationMemberType)pbf.ReadUint());
            }
        }
        else if (wireType == WireType.VarInt)
        {
            list.Add((PbfRelationMemberType)pbf.ReadUint());
        }
    }
}
