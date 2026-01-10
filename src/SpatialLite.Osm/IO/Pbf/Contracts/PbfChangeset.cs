using PbfLite;
using ProtoBuf;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents data transfer object used by PBF serializer for changesets.
/// </summary>
[ProtoContract(Name = "ChangeSet")]
public class PbfChangeset
{

    /// <summary>
    /// Gets or sets id of the changeset.
    /// </summary>
    [ProtoMember(1, IsRequired = true, Name = "id")]
    public long ID { get; set; }

    public static PbfChangeset Deserialize(PbfBlockReader pbf)
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
