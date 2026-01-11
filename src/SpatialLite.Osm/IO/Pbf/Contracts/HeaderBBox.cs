using PbfLite;
using ProtoBuf;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents rectangular envelope of data.
/// </summary>
[ProtoContract(Name = "HeaderBBox")]
internal class HeaderBBox
{

    /// <summary>
    /// Gets or sets Bottom boundary of the BBox.
    /// </summary>
    [ProtoMember(4, Name = "bottom", IsRequired = true, DataFormat = DataFormat.ZigZag)]
    public long Bottom { get; set; }

    /// <summary>
    /// Gets or sets Left boundary of the BBox.
    /// </summary>
    [ProtoMember(1, Name = "left", IsRequired = true, DataFormat = DataFormat.ZigZag)]
    public long Left { get; set; }

    /// <summary>
    /// Gets or sets Right boundary of the BBox.
    /// </summary>
    [ProtoMember(2, Name = "right", IsRequired = true, DataFormat = DataFormat.ZigZag)]
    public long Right { get; set; }

    /// <summary>
    /// Gets or sets Top boundary of the BBox.
    /// </summary>
    [ProtoMember(3, Name = "top", IsRequired = true, DataFormat = DataFormat.ZigZag)]
    public long Top { get; set; }

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
