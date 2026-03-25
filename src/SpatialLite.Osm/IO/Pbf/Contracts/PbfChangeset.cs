using PbfLite;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents data transfer object used by PBF serializer for changesets.
/// </summary>
public class PbfChangeset
{

    /// <summary>
    /// Gets or sets id of the changeset.
    /// </summary>
    public long ID { get; set; }

    /// <summary>
    /// Serializes the changeset to a PBF block writer.
    /// </summary>
    /// <param name="pbf">The PBF block writer to serialize to.</param>
    public void Serialize(ref PbfBlockWriter pbf)
    {
        pbf.WriteFieldHeader(1, WireType.VarInt);
        pbf.WriteLong(ID);
    }

    /// <summary>
    /// Deserializes a changeset from a PBF block reader.
    /// </summary>
    /// <param name="pbf">The PBF block reader to deserialize from.</param>
    /// <returns>A new PbfChangeset instance containing the deserialized changeset data.</returns>
    public static PbfChangeset Deserialize(ref PbfBlockReader pbf)
    {
        var result = new PbfChangeset();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.ID = pbf.ReadLong();
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
