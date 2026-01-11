using PbfLite;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents container for PBF data transfer objects for OSM entities.
/// </summary>
internal class PrimitiveGroup
{

    /// <summary>
    /// Gets or sets collection of nodes.
    /// </summary>
    public List<PbfNode>? Nodes { get; set; }

    /// <summary>
    /// Gets or sets collection of nodes serialized in dense format.
    /// </summary>
    public PbfDenseNodes? DenseNodes { get; set; }

    /// <summary>
    /// Gets or sets collection of way.
    /// </summary>
    public List<PbfWay>? Ways { get; set; }

    /// <summary>
    /// Gets or sets collection of relations.
    /// </summary>
    public List<PbfRelation>? Relations { get; set; }

    /// <summary>
    /// Gets or sets collection of changesets.
    /// </summary>
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

    public void Serialize(ref PbfBlockWriter pbf)
    {
        if (Nodes != null && Nodes.Count > 0)
        {
            foreach (var node in Nodes)
            {
                pbf.WriteFieldHeader(1, PbfLite.WireType.String);

                var nodeBlock = pbf.StartLengthPrefixedBlock(256);
                node.Serialize(ref pbf);
                pbf.FinalizeLengthPrefixedBlock(nodeBlock);
            }
        }

        if (DenseNodes != null)
        {
            pbf.WriteFieldHeader(2, PbfLite.WireType.String);

            var denseBlock = pbf.StartLengthPrefixedBlock(1024);
            DenseNodes.Serialize(ref pbf);
            pbf.FinalizeLengthPrefixedBlock(denseBlock);
        }

        if (Ways != null && Ways.Count > 0)
        {
            foreach (var way in Ways)
            {
                pbf.WriteFieldHeader(3, PbfLite.WireType.String);

                var wayBlock = pbf.StartLengthPrefixedBlock(256);
                way.Serialize(ref pbf);
                pbf.FinalizeLengthPrefixedBlock(wayBlock);
            }
        }

        if (Relations != null && Relations.Count > 0)
        {
            foreach (var relation in Relations)
            {
                pbf.WriteFieldHeader(4, PbfLite.WireType.String);

                var relationBlock = pbf.StartLengthPrefixedBlock(256);
                relation.Serialize(ref pbf);
                pbf.FinalizeLengthPrefixedBlock(relationBlock);
            }
        }

        if (Changesets != null && Changesets.Count > 0)
        {
            foreach (var changeset in Changesets)
            {
                pbf.WriteFieldHeader(5, PbfLite.WireType.String);

                var changesetBlock = pbf.StartLengthPrefixedBlock(256);
                changeset.Serialize(ref pbf);
                pbf.FinalizeLengthPrefixedBlock(changesetBlock);
            }
        }
    }
}
