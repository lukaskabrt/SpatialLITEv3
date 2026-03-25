using PbfLite;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents content of the file block.
/// </summary>
internal class Blob
{
    /// <summary>
    /// Gets or sets blob content if no compression is used.
    /// </summary>
    public byte[]? Raw { get; set; }

    /// <summary>
    /// Gets or sets uncompressed size of the blob content if ZLIB compression is used.
    /// </summary>
    public int? RawSize { get; set; }

    /// <summary>
    /// Gets or sets blob content if ZLIB compression is used.
    /// </summary>
    public byte[]? ZlibData { get; set; }

    /// <summary>
    /// Deserializes a blob from a PBF block reader.
    /// </summary>
    /// <param name="pbf">The PBF block reader to deserialize from.</param>
    /// <returns>A new Blob instance containing the deserialized blob data.</returns>
    public static Blob Deserialize(ref PbfBlockReader pbf)
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
}
