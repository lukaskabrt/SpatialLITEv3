using PbfLite;
using ProtoBuf;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents data transfer object used by PBF serializer for metadata in dense format.
/// </summary>
[ProtoContract(Name = "DenseInfo")]
internal class PbfDenseMetadata
{
    private List<long> _changeset = new List<long>();
    private List<long> _timestamp = new List<long>();
    private List<int> _userId = new List<int>();
    private List<int> _userNameIndex = new List<int>();
    private List<int> _version = new List<int>();
    private List<bool> _visible = new List<bool>();

    /// <summary>
    /// Initializes a new instance of the DenseInfo class with internal fields initialized to default capacity.
    /// </summary>
    public PbfDenseMetadata()
    {
        _changeset = new List<long>();
        _timestamp = new List<long>();
        _userId = new List<int>();
        _userNameIndex = new List<int>();
        _version = new List<int>();
        _visible = new List<bool>();
    }

    /// <summary>
    /// Initializes a new instance of the DenseInfo class with internal fields initialized to specified capacity.
    /// </summary>
    /// <param name="capacity">The desired capacity of internal fields.</param>
    public PbfDenseMetadata(int capacity)
    {
        _changeset = new List<long>(capacity);
        _timestamp = new List<long>(capacity);
        _userId = new List<int>(capacity);
        _userNameIndex = new List<int>(capacity);
        _version = new List<int>(capacity);
        _visible = new List<bool>(capacity);
    }

    /// <summary>
    /// Gets or sets changeset id for corresponding node in DenseNodes. Property is delta encoded.
    /// </summary>
    [ProtoMember(3, Name = "changeset", Options = MemberSerializationOptions.Packed, DataFormat = DataFormat.ZigZag)]
    public List<long> Changeset
    {
        get { return _changeset; }
        set { _changeset = value; }
    }

    /// <summary>
    /// Gets or sets timestamp as number of DateGranularity from UNIX 1970 epoch for the corresponding node in DenseNodes. Property is delta encoded.
    /// </summary>
    /// <example>
    /// DateTime LastChange = _unixEpoch.AddMilliseconds(timestamp * block.DateGranularity).
    /// </example>
    [ProtoMember(2, Name = "timestamp", Options = MemberSerializationOptions.Packed, DataFormat = DataFormat.ZigZag)]
    public List<long> Timestamp
    {
        get { return _timestamp; }
        set { _timestamp = value; }
    }

    /// <summary>
    /// Gets or sets UserId for corresponding node in DenseNodes. Property is delta encoded.
    /// </summary>
    [ProtoMember(4, Name = "uid", Options = MemberSerializationOptions.Packed, DataFormat = DataFormat.ZigZag)]
    public List<int> UserId
    {
        get { return _userId; }
        set { _userId = value; }
    }

    /// <summary>
    /// Gets or sets index of the UserName in StringTable for corresponding node in DenseNodes. Property is delta encoded.
    /// </summary>
    [ProtoMember(5, Name = "user_sid", Options = MemberSerializationOptions.Packed, DataFormat = DataFormat.ZigZag)]
    public List<int> UserNameIndex
    {
        get { return _userNameIndex; }
        set { _userNameIndex = value; }
    }

    /// <summary>
    /// Gets or sets version of the corresponding node in DenseNodes.
    /// </summary>
    [ProtoMember(1, Name = "version", Options = MemberSerializationOptions.Packed)]
    public List<int> Version
    {
        get { return _version; }
        set { _version = value; }
    }

    /// <summary>
    /// Gets or sets visible attribute for corresponding node in DenseNodes.
    /// </summary>
    [ProtoMember(6, Name = "visible", Options = MemberSerializationOptions.Packed)]
    public List<bool> Visible
    {
        get { return _visible; }
        set { _visible = value; }
    }

    public static PbfDenseMetadata Deserialize(PbfBlockReader pbf)
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
}
