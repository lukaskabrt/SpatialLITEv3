using PbfLite;
using ProtoBuf;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents header of the fileblock.
/// </summary>
[ProtoContract]
internal class BlobHeader
{

    /// <summary>
    /// Gets or sets type of the fileblock.
    /// </summary>
    /// <remarks>
    /// Supported values are 'OSMHeader' and 'OSMData'.
    /// </remarks>
    [ProtoMember(1, IsRequired = true, Name = "type")]
    public required string Type { get; set; }

    /// <summary>
    /// Gets or sets an arbitrary blob that may include metadata about the following blob. For future use.
    /// </summary>
    [ProtoMember(2, IsRequired = false, Name = "indexdata")]
    public byte[]? IndexData { get; set; }

    /// <summary>
    /// Gets or sets size of the subsequent Blob message.
    /// </summary>
    [ProtoMember(3, IsRequired = true, Name = "datasize")]
    public int DataSize { get; set; }

    public static BlobHeader Deserialize(PbfBlockReader pbf)
    {
        string? type = null;
        int? dataSize = null;

        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    type = pbf.ReadString();
                    break;
                case 3:
                    dataSize = pbf.ReadInt();
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();
        }

        if (type == null)
        {
            throw new InvalidDataException("Invalid BlobHeader - missing 'type' field.");
        }

        return new BlobHeader { Type = type, DataSize = dataSize ?? 0 };
    }
}
