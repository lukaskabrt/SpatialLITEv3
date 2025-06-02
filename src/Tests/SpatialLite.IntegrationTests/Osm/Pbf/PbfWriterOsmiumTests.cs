using SpatialLite.Osm.IO.Pbf;

namespace SpatialLite.IntegrationTests.Osm.Pbf;

public class PbfWriterOsmiumTests : OsmiumTests
{
    [Theory]
    [InlineData(false, false, CompressionMode.None)]
    [InlineData(false, false, CompressionMode.ZlibDeflate)]
    [InlineData(false, true, CompressionMode.None)]
    [InlineData(false, true, CompressionMode.ZlibDeflate)]
    [InlineData(true, false, CompressionMode.None)]
    [InlineData(true, false, CompressionMode.ZlibDeflate)]
    [InlineData(true, true, CompressionMode.None)]
    [InlineData(true, true, CompressionMode.ZlibDeflate)]
    public void OsmiumReadsPbfFileCreatedByPbfWriter(bool writeMetadata, bool useDenseFormat, CompressionMode compression)
    {
        var pbfFile = GetTempFileName(".pbf");
        var targetFile = GetTempFileName(".osm.xml");

        try
        {
            var settings = new PbfWriterSettings
            {
                Compression = compression,
                UseDenseFormat = useDenseFormat,
                WriteMetadata = writeMetadata
            };

            using (var writer = new PbfWriter(pbfFile, settings))
            {
                GenerateOutputFile(writer);
            }

            AssertReadWithOsmium(pbfFile, targetFile);
        }
        finally
        {
            File.Delete(pbfFile);
            File.Delete(targetFile);
        }
    }
}
