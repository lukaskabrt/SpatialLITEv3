using PbfLite;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Represents header message of the PBF file.
/// </summary>
internal class OsmHeader
{
    /// <summary>
    /// Gets or sets bounding box of the data in the file
    /// </summary>
    public HeaderBBox? BBox { get; set; }

    /// <summary>
    /// Gets or sets collection of optional features that parser could take advantage of
    /// </summary>
    public List<string> OptionalFeatures { get; set; } = [];

    /// <summary>
    /// Gets or sets collection of required features that parser must support to process the file
    /// </summary>
    public List<string> RequiredFeatures { get; set; } = [];

    /// <summary>
    /// Gets or sets source of the data
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets identification of writing program
    /// </summary>
    public string? WritingProgram { get; set; }

    /// <summary>
    /// Serializes the OSM header to a PBF block writer.
    /// </summary>
    /// <param name="pbf">The PBF block writer to serialize to.</param>
    public void Serialize(ref PbfBlockWriter pbf)
    {
        if (BBox != null)
        {
            pbf.WriteFieldHeader(1, WireType.String);

            var bboxBlock = pbf.StartLengthPrefixedBlock(32);
            BBox.Serialize(ref pbf);
            pbf.FinalizeLengthPrefixedBlock(bboxBlock);
        }

        foreach (var feature in RequiredFeatures)
        {
            pbf.WriteFieldHeader(4, WireType.String);
            pbf.WriteString(feature);
        }

        foreach (var feature in OptionalFeatures)
        {
            pbf.WriteFieldHeader(5, WireType.String);
            pbf.WriteString(feature);
        }

        if (Source != null)
        {
            pbf.WriteFieldHeader(16, WireType.String);
            pbf.WriteString(Source);
        }

        if (WritingProgram != null)
        {
            pbf.WriteFieldHeader(17, WireType.String);
            pbf.WriteString(WritingProgram);
        }
    }

    /// <summary>
    /// Deserializes an OSM header from a PBF block reader.
    /// </summary>
    /// <param name="pbf">The PBF block reader to deserialize from.</param>
    /// <returns>A new OsmHeader instance containing the deserialized header data.</returns>
    public static OsmHeader Deserialize(ref PbfBlockReader pbf)
    {
        var result = new OsmHeader();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    var headerPbf = PbfBlockReader.Create(pbf.ReadLengthPrefixedBytes());
                    result.BBox = HeaderBBox.Deserialize(ref headerPbf);
                    break;
                case 4:
                    result.RequiredFeatures.Add(pbf.ReadString());
                    break;
                case 5:
                    result.OptionalFeatures.Add(pbf.ReadString());
                    break;
                case 16:
                    result.Source = pbf.ReadString();
                    break;
                case 17:
                    result.WritingProgram = pbf.ReadString();
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
