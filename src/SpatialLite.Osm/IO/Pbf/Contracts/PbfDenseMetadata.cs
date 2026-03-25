using PbfLite;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents data transfer object used by PBF serializer for metadata in dense format.
/// </summary>
internal class PbfDenseMetadata
{
    /// <summary>
    /// Initializes a new instance of the DenseInfo class with internal fields initialized to default capacity.
    /// </summary>
    public PbfDenseMetadata()
    {
        Changeset = [];
        Timestamp = [];
        UserId = [];
        UserNameIndex = [];
        Version = [];
        Visible = [];
    }

    /// <summary>
    /// Initializes a new instance of the DenseInfo class with internal fields initialized to specified capacity.
    /// </summary>
    /// <param name="capacity">The desired capacity of internal fields.</param>
    public PbfDenseMetadata(int capacity)
    {
        Changeset = new List<long>(capacity);
        Timestamp = new List<long>(capacity);
        UserId = new List<int>(capacity);
        UserNameIndex = new List<int>(capacity);
        Version = new List<int>(capacity);
        Visible = new List<bool>(capacity);
    }

    /// <summary>
    /// Gets or sets changeset id for corresponding node in DenseNodes. Property is delta encoded.
    /// </summary>
    public List<long> Changeset { get; set; }

    /// <summary>
    /// Gets or sets timestamp as number of DateGranularity from UNIX 1970 epoch for the corresponding node in DenseNodes. Property is delta encoded.
    /// </summary>
    /// <example>
    /// DateTime LastChange = _unixEpoch.AddMilliseconds(timestamp * block.DateGranularity).
    /// </example>
    public List<long> Timestamp { get; set; }

    /// <summary>
    /// Gets or sets UserId for corresponding node in DenseNodes. Property is delta encoded.
    /// </summary>
    public List<int> UserId { get; set; }

    /// <summary>
    /// Gets or sets index of the UserName in StringTable for corresponding node in DenseNodes. Property is delta encoded.
    /// </summary>
    public List<int> UserNameIndex { get; set; }

    /// <summary>
    /// Gets or sets version of the corresponding node in DenseNodes.
    /// </summary>
    public List<int> Version { get; set; }

    /// <summary>
    /// Gets or sets visible attribute for corresponding node in DenseNodes.
    /// </summary>
    public List<bool> Visible { get; set; }

    /// <summary>
    /// Deserializes dense metadata from a PBF block reader.
    /// </summary>
    /// <param name="pbf">The PBF block reader to deserialize from.</param>
    /// <returns>A new PbfDenseMetadata instance containing the deserialized metadata.</returns>
    public static PbfDenseMetadata Deserialize(ref PbfBlockReader pbf)
    {
        var result = new PbfDenseMetadata();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1: // version - not ZigZag encoded
                    pbf.ReadIntCollection(wireType, result.Version);
                    break;
                case 2: // timestamp - ZigZag encoded
                    pbf.ReadSignedLongCollection(wireType, result.Timestamp);
                    break;
                case 3: // changeset - ZigZag encoded
                    pbf.ReadSignedLongCollection(wireType, result.Changeset);
                    break;
                case 4: // uid - ZigZag encoded
                    pbf.ReadSignedIntCollection(wireType, result.UserId);
                    break;
                case 5: // user_sid - ZigZag encoded
                    pbf.ReadSignedIntCollection(wireType, result.UserNameIndex);
                    break;
                case 6: // visible - not ZigZag encoded
                    pbf.ReadBooleanCollection(wireType, result.Visible);
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();
        }

        return result;
    }

    /// <summary>
    /// Serializes the dense metadata to a PBF block writer.
    /// </summary>
    /// <param name="pbf">The PBF block writer to serialize to.</param>
    public void Serialize(ref PbfBlockWriter pbf)
    {
        if (Version.Count > 0)
        {
            pbf.WriteFieldHeader(1, WireType.String);
            pbf.WriteIntCollection(Version);
        }

        if (Timestamp.Count > 0)
        {
            pbf.WriteFieldHeader(2, WireType.String);
            pbf.WriteSignedLongCollection(Timestamp);
        }

        if (Changeset.Count > 0)
        {
            pbf.WriteFieldHeader(3, WireType.String);
            pbf.WriteSignedLongCollection(Changeset);
        }

        if (UserId.Count > 0)
        {
            pbf.WriteFieldHeader(4, WireType.String);
            pbf.WriteSignedIntCollection(UserId);
        }

        if (UserNameIndex.Count > 0)
        {
            pbf.WriteFieldHeader(5, WireType.String);
            pbf.WriteSignedIntCollection(UserNameIndex);
        }

        if (Visible.Count > 0)
        {
            pbf.WriteFieldHeader(6, WireType.String);
            pbf.WriteBooleanCollection(Visible);
        }
    }
}
