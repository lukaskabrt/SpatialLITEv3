using PbfLite;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents header of the fileblock.
/// </summary>
internal class BlobHeader
{
    /// <summary>
    /// Gets or sets type of the fileblock.
    /// </summary>
    /// <remarks>
    /// Supported values are 'OSMHeader' and 'OSMData'.
    /// </remarks>
    public required string Type { get; set; }

    /// <summary>
    /// Gets or sets an arbitrary blob that may include metadata about the following blob. For future use.
    /// </summary>
    public byte[]? IndexData { get; set; }

    /// <summary>
    /// Gets or sets size of the subsequent Blob message.
    /// </summary>
    public int DataSize { get; set; }

    /// <summary>
    /// Deserializes a blob header from a PBF block reader.
    /// </summary>
    /// <param name="pbf">The PBF block reader to deserialize from.</param>
    /// <returns>A new BlobHeader instance containing the deserialized header data.</returns>
    public static BlobHeader Deserialize(ref PbfBlockReader pbf)
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
