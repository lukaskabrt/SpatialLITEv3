using PbfLite;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents rectangular envelope of data.
/// </summary>
internal class HeaderBBox
{
    /// <summary>
    /// Gets or sets Bottom boundary of the BBox.
    /// </summary>
    public long Bottom { get; set; }

    /// <summary>
    /// Gets or sets Left boundary of the BBox.
    /// </summary>
    public long Left { get; set; }

    /// <summary>
    /// Gets or sets Right boundary of the BBox.
    /// </summary>
    public long Right { get; set; }

    /// <summary>
    /// Gets or sets Top boundary of the BBox.
    /// </summary>
    public long Top { get; set; }

    public void Serialize(ref PbfBlockWriter pbf)
    {
        pbf.WriteFieldHeader(1, WireType.VarInt);
        pbf.WriteSignedLong(Left);

        pbf.WriteFieldHeader(2, WireType.VarInt);
        pbf.WriteSignedLong(Right);

        pbf.WriteFieldHeader(3, WireType.VarInt);
        pbf.WriteSignedLong(Top);

        pbf.WriteFieldHeader(4, WireType.VarInt);
        pbf.WriteSignedLong(Bottom);
    }

    public static HeaderBBox Deserialize(ref PbfBlockReader pbf)
    {
        var result = new HeaderBBox();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.Left = pbf.ReadSignedLong();
                    break;
                case 2:
                    result.Right = pbf.ReadSignedLong();
                    break;
                case 3:
                    result.Top = pbf.ReadSignedLong();
                    break;
                case 4:
                    result.Bottom = pbf.ReadSignedLong();
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
