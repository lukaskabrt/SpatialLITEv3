using PbfLite;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents content of the fileblock's data stream.
/// </summary>
internal class PrimitiveBlock
{

    private int _granularity = 100;
    private int _date_granularity = 1000;

    /// <summary>
    /// Gets or sets StringTable with all strings used in the block.
    /// </summary>
    public StringTable StringTable { get; set; } = new StringTable();

    /// <summary>
    /// Gets or sets PrimitiveGroup object with OSM entities.
    /// </summary>
    public List<PrimitiveGroup> PrimitiveGroup { get; set; } = new List<PrimitiveGroup>();

    /// <summary>
    /// Gets or sets granularity of the position data. Default value is 100.
    /// </summary>
    public int Granularity
    {
        get { return _granularity; }
        set { _granularity = value; }
    }

    /// <summary>
    /// Gets or sets latitude offset.
    /// </summary>
    public long LatOffset { get; set; }

    /// <summary>
    /// Gets or sets longitude offset.
    /// </summary>
    public long LonOffset { get; set; }

    /// <summary>
    /// Gets or sets granularity of the DateTime data. Default value is 1000.
    /// </summary>
    public int DateGranularity
    {
        get { return _date_granularity; }
        set { _date_granularity = value; }
    }

    public static PrimitiveBlock Deserialize(ref PbfBlockReader pbf)
    {
        var result = new PrimitiveBlock();

        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    var stringTablePbf = PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes());
                    result.StringTable = StringTable.Deserialize(ref stringTablePbf);
                    break;
                case 2:
                    var primitiveGroupPbf = PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes());
                    result.PrimitiveGroup.Add(Contracts.PrimitiveGroup.Deserialize(ref primitiveGroupPbf));
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

    public void Serialize(ref PbfBlockWriter pbf)
    {
        pbf.WriteFieldHeader(1, PbfLite.WireType.String);

        var stringTableBlock = pbf.StartLengthPrefixedBlock(1024);
        StringTable.Serialize(ref pbf);
        pbf.FinalizeLengthPrefixedBlock(stringTableBlock);

        foreach (var primitiveGroup in PrimitiveGroup)
        {
            pbf.WriteFieldHeader(2, PbfLite.WireType.String);

            var primitiveGroupBlock = pbf.StartLengthPrefixedBlock(1024);
            primitiveGroup.Serialize(ref pbf);
            pbf.FinalizeLengthPrefixedBlock(primitiveGroupBlock);
        }

        pbf.WriteFieldHeader(16, PbfLite.WireType.VarInt);
        pbf.WriteInt(Granularity);

        pbf.WriteFieldHeader(18, PbfLite.WireType.VarInt);
        pbf.WriteInt(DateGranularity);

        if (LatOffset != 0)
        {
            pbf.WriteFieldHeader(19, PbfLite.WireType.VarInt);
            pbf.WriteLong(LatOffset);
        }

        if (LonOffset != 0)
        {
            pbf.WriteFieldHeader(20, PbfLite.WireType.VarInt);
            pbf.WriteLong(LonOffset);
        }
    }
}
