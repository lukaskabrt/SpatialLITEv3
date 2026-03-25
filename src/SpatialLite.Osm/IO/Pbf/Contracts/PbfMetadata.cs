using PbfLite;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents data transfer object used by PBF serializer for Entity metadata.
/// </summary>
internal class PbfMetadata
{
    /// <summary>
    /// Gets or sets changeset ID.
    /// </summary>
    public long? Changeset { get; set; }

    /// <summary>
    /// Gets or sets Timestamp.
    /// </summary>
    public long? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets UserID.
    /// </summary>
    public int? UserID { get; set; }

    /// <summary>
    /// Gets or sets index of the username in string table.
    /// </summary>
    public int? UserNameIndex { get; set; }

    /// <summary>
    /// Gets or sets version of the entity.
    /// </summary>
    public int? Version { get; set; }

    /// <summary>
    /// Serializes the metadata to a PBF block writer.
    /// </summary>
    /// <param name="pbf">The PBF block writer to serialize to.</param>
    public void Serialize(ref PbfBlockWriter pbf)
    {
        if (Version.HasValue)
        {
            pbf.WriteFieldHeader(1, WireType.VarInt);
            pbf.WriteInt(Version.Value);
        }

        if (Timestamp.HasValue)
        {
            pbf.WriteFieldHeader(2, WireType.VarInt);
            pbf.WriteLong(Timestamp.Value);
        }

        if (Changeset.HasValue)
        {
            pbf.WriteFieldHeader(3, WireType.VarInt);
            pbf.WriteLong(Changeset.Value);
        }

        if (UserID.HasValue)
        {
            pbf.WriteFieldHeader(4, WireType.VarInt);
            pbf.WriteInt(UserID.Value);
        }

        if (UserNameIndex.HasValue)
        {
            pbf.WriteFieldHeader(5, WireType.VarInt);
            pbf.WriteInt(UserNameIndex.Value);
        }
    }

    /// <summary>
    /// Deserializes metadata from a PBF block reader.
    /// </summary>
    /// <param name="pbf">The PBF block reader to deserialize from.</param>
    /// <returns>A new PbfMetadata instance containing the deserialized metadata.</returns>
    public static PbfMetadata Deserialize(ref PbfBlockReader pbf)
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
