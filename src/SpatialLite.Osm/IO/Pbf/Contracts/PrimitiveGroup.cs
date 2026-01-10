using PbfLite;
using ProtoBuf;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents container for PBF data transfer objects for OSM entities.
/// </summary>
[ProtoContract(Name = "PrimitiveGroup")]
internal class PrimitiveGroup
{

    /// <summary>
    /// Gets or sets collection of nodes.
    /// </summary>
    [ProtoMember(1, Name = "nodes")]
    public List<PbfNode>? Nodes { get; set; }

    /// <summary>
    /// Gets or sets collection of nodes serialized in dense format.
    /// </summary>
    [ProtoMember(2, IsRequired = false, Name = "dense")]
    public PbfDenseNodes? DenseNodes { get; set; }

    /// <summary>
    /// Gets or sets collection of way.
    /// </summary>
    [ProtoMember(3, Name = "ways")]
    public List<PbfWay>? Ways { get; set; }

    /// <summary>
    /// Gets or sets collection of relations.
    /// </summary>
    [ProtoMember(4, Name = "relations")]
    public List<PbfRelation>? Relations { get; set; }

    /// <summary>
    /// Gets or sets collection of changesets.
    /// </summary>
    [ProtoMember(5, Name = "changesets")]
    public List<PbfChangeset>? Changesets { get; set; }

    public static PrimitiveGroup Deserialize(PbfBlockReader pbf)
    {
        var result = new PrimitiveGroup();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.Nodes ??= [];
                    result.Nodes.Add(PbfNode.Deserialize(PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes())));
                    break;
                case 2:
                    result.DenseNodes = PbfDenseNodes.Deserialize(PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes()));
                    break;
                case 3:
                    result.Ways ??= [];
                    result.Ways.Add(PbfWay.Deserialize(PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes())));
                    break;
                case 4:
                    result.Relations ??= [];
                    result.Relations.Add(PbfRelation.Deserialize(PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes())));
                    break;
                case 5:
                    result.Changesets ??= [];

                    result.Changesets.Add(PbfChangeset.Deserialize(PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes())));
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
