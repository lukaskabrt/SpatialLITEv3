using SpatialLITE.Osm.IO.Pbf;

namespace SpatialLITE.UnitTests.Osm.IO.Pbf;

public class PbfWriterSettingsTests
{
    [Fact]
    public void Constructor__SetsDefaultValues()
    {
        PbfWriterSettings target = new();

        Assert.True(target.UseDenseFormat);
        Assert.Equal(CompressionMode.ZlibDeflate, target.Compression);
        Assert.False(target.WriteMetadata);
    }
}
