using PbfLite;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents data transfer object used by PBF serializer for nodes saved in dense format.
/// </summary>
internal class PbfDenseNodes
{
    private List<long> _id;
    private List<long> _latitude;
    private List<long> _longitude;
    private List<uint> _keysVals;

    /// <summary>
    /// Initializes a new instance of the DenseNodes class with internal fields initialized to default capacity.
    /// </summary>
    public PbfDenseNodes()
    {
        _id = [];
        _latitude = [];
        _longitude = [];
        _keysVals = [];
    }

    /// <summary>
    /// Initializes a new instance of the DenseInfo class with internal fields initialized to specified capacity.
    /// </summary>
    /// <param name="capacity">The desired capacity of internal fields.</param>
    public PbfDenseNodes(int capacity)
    {
        _id = new List<long>(capacity);
        _latitude = new List<long>(capacity);
        _longitude = new List<long>(capacity);
        _keysVals = new List<uint>(capacity);
    }

    /// <summary>
    /// Gets or sets ids of the nodes. This property is delta encoded.
    /// </summary>
    public List<long> Id
    {
        get { return _id; }
        set { _id = value; }
    }

    /// <summary>
    /// Gets or sets latitudes of the nodes as number of granularity steps from the LatOffset. This property is delta encoded.
    /// </summary>
    /// <example>
    /// double nodeLat = 1E-09 * (block.LatOffset + (block.Granularity * Latitude));
    /// </example>
    public List<long> Latitude
    {
        get { return _latitude; }
        set { _latitude = value; }
    }

    /// <summary>
    /// Gets or sets longitude of the nodes as number of granularity steps from the LonOffset. This property is delta encoded.
    /// </summary>
    /// <example>
    /// double nodeLon = 1E-09 * (block.LonOffset + (block.Granularity * Longitude));
    /// </example>
    public List<long> Longitude
    {
        get { return _longitude; }
        set { _longitude = value; }
    }

    /// <summary>
    /// Gets or sets entities metadata encoded in the DenseInfo object
    /// </summary>
    public PbfDenseMetadata? DenseInfo { get; set; }

    /// <summary>
    /// Gets or sets tags for nodes.
    /// </summary>
    /// <remarks>
    /// Tags are saved as (KeyIndex, ValueIndex) pairs. Tags for consecutive nodes are separated by 0.
    /// </remarks>
    public List<uint> KeysVals
    {
        get { return _keysVals; }
        set { _keysVals = value; }
    }

    public static PbfDenseNodes Deserialize(ref PbfBlockReader pbf)
    {
        var result = new PbfDenseNodes();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    pbf.ReadSignedLongCollection(wireType, result.Id);
                    break;
                case 8:
                    pbf.ReadSignedLongCollection(wireType, result.Latitude);
                    break;
                case 9:
                    pbf.ReadSignedLongCollection(wireType, result.Longitude);
                    break;
                case 10:
                    pbf.ReadUIntCollection(wireType, result.KeysVals);
                    break;
                case 5:
                    var metadataPbf = PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes());
                    result.DenseInfo = PbfDenseMetadata.Deserialize(ref metadataPbf);
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
        if (Id.Count > 0)
        {
            pbf.WriteFieldHeader(1, PbfLite.WireType.String);
            pbf.WriteSignedLongCollection(Id.ToArray());
        }

        if (Latitude.Count > 0)
        {
            pbf.WriteFieldHeader(8, PbfLite.WireType.String);
            pbf.WriteSignedLongCollection(Latitude.ToArray());
        }

        if (Longitude.Count > 0)
        {
            pbf.WriteFieldHeader(9, PbfLite.WireType.String);
            pbf.WriteSignedLongCollection(Longitude.ToArray());
        }

        if (DenseInfo != null)
        {
            pbf.WriteFieldHeader(5, PbfLite.WireType.String);
            var denseInfoBlock = pbf.StartLengthPrefixedBlock(512);
            DenseInfo.Serialize(ref pbf);
            pbf.FinalizeLengthPrefixedBlock(denseInfoBlock);
        }

        if (KeysVals.Count > 0)
        {
            pbf.WriteFieldHeader(10, PbfLite.WireType.String);
            pbf.WriteUIntCollection(KeysVals.ToArray());
        }
    }
}
