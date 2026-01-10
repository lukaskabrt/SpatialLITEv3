using PbfLite;
using ProtoBuf;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents content of the file block.
/// </summary>
[ProtoContract]
internal class Blob
{

    /// <summary>
    /// Gets or sets blob content if no compression is used.
    /// </summary>
    [ProtoMember(1, IsRequired = false, Name = "raw")]
    public byte[]? Raw { get; set; }

    /// <summary>
    /// Gets or sets uncompressed size of the blob content if ZLIB compression is used.
    /// </summary>
    [ProtoMember(2, IsRequired = false, Name = "raw_size")]
    public int? RawSize { get; set; }

    /// <summary>
    /// Gets or sets blob content if ZLIB compression is used.
    /// </summary>
    [ProtoMember(3, IsRequired = false, Name = "zlib_data")]
    public byte[]? ZlibData { get; set; }

    public static Blob Deserialize(PbfBlockReader pbf)
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
