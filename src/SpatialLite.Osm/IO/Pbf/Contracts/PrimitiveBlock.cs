using PbfLite;
using ProtoBuf;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents content of the fileblock's data stream.
/// </summary>
[ProtoContract(Name = "PrimitiveBlock")]
internal class PrimitiveBlock
{

    private int _granularity = 100;
    private int _date_granularity = 1000;

    /// <summary>
    /// Gets or sets StringTable with all strings used in the block.
    /// </summary>
    [ProtoMember(1, IsRequired = true, Name = "stringtable")]
    public StringTable StringTable { get; set; } = new StringTable();

    /// <summary>
    /// Gets or sets PrimitiveGroup object with OSM entities.
    /// </summary>
    [ProtoMember(2, Name = "primitivegroup")]
    public List<PrimitiveGroup> PrimitiveGroup { get; set; } = new List<PrimitiveGroup>();

    /// <summary>
    /// Gets or sets granularity of the position data. Default value is 100.
    /// </summary>
    [ProtoMember(16, IsRequired = false, Name = "granularity")]
    public int Granularity
    {
        get { return _granularity; }
        set { _granularity = value; }
    }

    /// <summary>
    /// Gets or sets latitude offset.
    /// </summary>
    [ProtoMember(19, IsRequired = false, Name = "lat_offset")]
    public long LatOffset { get; set; }

    /// <summary>
    /// Gets or sets longitude offset.
    /// </summary>
    [ProtoMember(20, IsRequired = false, Name = "lon_offset")]
    public long LonOffset { get; set; }

    /// <summary>
    /// Gets or sets granularity of the DateTime data. Default value is 1000.
    /// </summary>
    [ProtoMember(18, IsRequired = false, Name = "date_granularity")]
    public int DateGranularity
    {
        get { return _date_granularity; }
        set { _date_granularity = value; }
    }

    public static PrimitiveBlock Deserialize(PbfBlockReader pbf)
    {
        var result = new PrimitiveBlock();

        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.StringTable = StringTable.Deserialize(PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes()));
                    break;
                case 2:
                {
                    result.PrimitiveGroup.Add(Contracts.PrimitiveGroup.Deserialize(PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes())));
                }

                break;
                case 16:
                    result.Granularity = pbf.ReadInt();
                    break;
                case 18:
                    result.DateGranularity = pbf.ReadInt();
                    break;
                case 19:
                    result.LatOffset = pbf.ReadLong();
                    break;
                case 20:
                    result.LonOffset = pbf.ReadLong();
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
