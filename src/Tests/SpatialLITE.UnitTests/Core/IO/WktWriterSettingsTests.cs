using SpatialLite.Core.IO;
using Xunit;

namespace SpatialLITE.UnitTests.Core.IO;

public class WktWriterSettingsTests
{

    [Fact]
    public void Constructor__SetsDefaultValues()
    {
        var target = new WktWriterSettings();

        Assert.False(target.IsReadOnly);
    }
}
