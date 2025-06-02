using SpatialLite.Osm.IO;
using SpatialLite.Osm.IO.Xml;

namespace SpatialLite.IntegrationTests.Osm.Xml;

public class XmlWriterOsmiumTests : OsmiumTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]

    public void OsmiumReadsXmlFileCreatedByXmlWriter(bool writeMetadata)
    {
        var xmlFile = GetTempFileName(".osm.xml");
        var targetFile = GetTempFileName(".pbf");

        try
        {
            using (var writer = new OsmXmlWriter(xmlFile, new OsmWriterSettings { WriteMetadata = writeMetadata }))
            {
                GenerateOutputFile(writer);
            }

            AssertReadWithOsmium(xmlFile, targetFile);
        }
        finally
        {
            File.Delete(xmlFile);
            File.Delete(targetFile);

        }
    }
}
