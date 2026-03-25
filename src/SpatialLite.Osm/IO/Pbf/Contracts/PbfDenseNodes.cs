using PbfLite;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents data transfer object used by PBF serializer for nodes saved in dense format.
/// </summary>
internal class PbfDenseNodes
{
    /// <summary>
    /// Initializes a new instance of the DenseNodes class with internal fields initialized to default capacity.
    /// </summary>
    public PbfDenseNodes()
    {
        Id = new List<long>();
        Latitude = new List<long>();
        Longitude = new List<long>();
        KeysVals = new List<uint>();
    }

    /// <summary>
    /// Initializes a new instance of the DenseInfo class with internal fields initialized to specified capacity.
    /// </summary>
    /// <param name="capacity">The desired capacity of internal fields.</param>
    public PbfDenseNodes(int capacity)
    {
        Id = new List<long>(capacity);
        Latitude = new List<long>(capacity);
        Longitude = new List<long>(capacity);
        KeysVals = new List<uint>(capacity);
    }

    /// <summary>
    /// Gets or sets ids of the nodes. This property is delta encoded.
    /// </summary>
    public List<long> Id { get; set; }

    /// <summary>
    /// Gets or sets latitudes of the nodes as number of granularity steps from the LatOffset. This property is delta encoded.
    /// </summary>
    /// <example>
    /// double nodeLat = 1E-09 * (block.LatOffset + (block.Granularity * Latitude));
    /// </example>
    public List<long> Latitude { get; set; }

    /// <summary>
    /// Gets or sets longitude of the nodes as number of granularity steps from the LonOffset. This property is delta encoded.
    /// </summary>
    /// <example>
    /// double nodeLon = 1E-09 * (block.LonOffset + (block.Granularity * Longitude));
    /// </example>
    public List<long> Longitude { get; set; }

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
    public List<uint> KeysVals { get; set; }

    /// <summary>
    /// Serializes the dense nodes to a PBF block writer.
    /// </summary>
    /// <param name="pbf">The PBF block writer to serialize to.</param>
    public void Serialize(ref PbfBlockWriter pbf)
    {
        if (Id.Count > 0)
        {
            pbf.WriteFieldHeader(1, WireType.String);
            pbf.WriteSignedLongCollection(Id);
        }

        if (Latitude.Count > 0)
        {
            pbf.WriteFieldHeader(8, WireType.String);
            pbf.WriteSignedLongCollection(Latitude);
        }

        if (Longitude.Count > 0)
        {
            pbf.WriteFieldHeader(9, WireType.String);
            pbf.WriteSignedLongCollection(Longitude);
        }

        if (DenseInfo != null)
        {
            pbf.WriteFieldHeader(5, WireType.String);
            var denseInfoBlock = pbf.StartLengthPrefixedBlock(512);
            DenseInfo.Serialize(ref pbf);
            pbf.FinalizeLengthPrefixedBlock(denseInfoBlock);
        }

        if (KeysVals.Count > 0)
        {
            pbf.WriteFieldHeader(10, WireType.String);
            pbf.WriteUIntCollection(KeysVals);
        }
    }

    /// <summary>
    /// Deserializes dense nodes from a PBF block reader.
    /// </summary>
    /// <param name="pbf">The PBF block reader to deserialize from.</param>
    /// <returns>A new PbfDenseNodes instance containing the deserialized nodes.</returns>
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
}
