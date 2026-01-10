using PbfLite;
using ProtoBuf;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents data transfer object used by PBF serializer for Entity metadata.
/// </summary>
[ProtoContract(Name = "Info")]
internal class PbfMetadata
{

    /// <summary>
    /// Gets or sets changeset ID.
    /// </summary>
    [ProtoMember(3, Name = "changeset", IsRequired = false)]
    public long? Changeset { get; set; }

    /// <summary>
    /// Gets or sets Timestamp.
    /// </summary>
    [ProtoMember(2, Name = "timestamp", IsRequired = false)]
    public long? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets UserID.
    /// </summary>
    [ProtoMember(4, Name = "uid", IsRequired = false)]
    public int? UserID { get; set; }

    /// <summary>
    /// Gets or sets index of the username in string table.
    /// </summary>
    [ProtoMember(5, Name = "user_sid", IsRequired = false)]
    public int? UserNameIndex { get; set; }

    /// <summary>
    /// Gets or sets version of the entity.
    /// </summary>
    [ProtoMember(1, Name = "version", IsRequired = false)]
    public int? Version { get; set; }

    public static PbfMetadata Deserialize(PbfBlockReader pbf)
    {
        var result = new PbfMetadata();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.Version = pbf.ReadInt();
                    break;
                case 2:
                    result.Timestamp = pbf.ReadLong();
                    break;
                case 3:
                    result.Changeset = pbf.ReadLong();
                    break;
                case 4:
                    result.UserID = pbf.ReadInt();
                    break;
                case 5:
                    result.UserNameIndex = pbf.ReadInt();
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
