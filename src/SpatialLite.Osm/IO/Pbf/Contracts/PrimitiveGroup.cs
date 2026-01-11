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

    public static PrimitiveGroup Deserialize(ref PbfBlockReader pbf)
    {
        var result = new PrimitiveGroup();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.Nodes ??= [];

                    var nodePbf = PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes());
                    result.Nodes.Add(PbfNode.Deserialize(ref nodePbf));
                    break;
                case 2:
                    var denseNodesPbf = PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes());
                    result.DenseNodes = PbfDenseNodes.Deserialize(ref denseNodesPbf);
                    break;
                case 3:
                    result.Ways ??= [];

                    var wayPbf = PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes());
                    result.Ways.Add(PbfWay.Deserialize(ref wayPbf));
                    break;
                case 4:
                    result.Relations ??= [];

                    var relationPbf = PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes());
                    result.Relations.Add(PbfRelation.Deserialize(ref relationPbf));
                    break;
                case 5:
                    result.Changesets ??= [];

                    var changesetsPbf = PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes());
                    result.Changesets.Add(PbfChangeset.Deserialize(ref changesetsPbf));
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
