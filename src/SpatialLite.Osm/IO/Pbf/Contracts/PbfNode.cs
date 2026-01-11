using PbfLite;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents data transfer object used by PBF serializer for nodes.
/// </summary>
internal class PbfNode
{
    /// <summary>
    /// Gets or sets ID of the node.
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
    /// Gets or sets Latitude of the node as number of granularity steps from LatOffset.
    /// </summary>
    public long Latitude { get; set; }

    /// <summary>
    /// Gets or sets Longitude of the node as number of granularity steps from LonOffset.
    /// </summary>
    public long Longitude { get; set; }

    public static PbfNode Deserialize(ref PbfBlockReader pbf)
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
                    pbf.ReadUIntCollection(wireType, result.Keys);
                    break;
                case 3:
                    result.Values ??= [];
                    pbf.ReadUIntCollection(wireType, result.Values);
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
        pbf.WriteSignedLong(ID);

        pbf.WriteFieldHeader(8, PbfLite.WireType.VarInt);
        pbf.WriteSignedLong(Latitude);

        pbf.WriteFieldHeader(9, PbfLite.WireType.VarInt);
        pbf.WriteSignedLong(Longitude);

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
    }
}
