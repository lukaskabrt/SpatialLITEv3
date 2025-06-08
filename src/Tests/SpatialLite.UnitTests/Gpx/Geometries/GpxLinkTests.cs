using SpatialLite.Gpx.Geometries;

namespace SpatialLite.UnitTests.Gpx.Geometries;

public class GpxLinkTests
{

    [Fact]
    public void Constructor_Url_SetsUrl()
    {
        Uri url = new("http://spatial.litesolutions.net");

        var target = new GpxLink(url);

        Assert.Same(url, target.Url);
    }
}
