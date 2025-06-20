﻿using SpatialLite.Core.IO;

namespace SpatialLite.UnitTests.Core.IO;

public class WkbWriterSettingsTests
{

    [Fact]
    public void Constructor__SetsDefaultValues()
    {
        var target = new WkbWriterSettings();

        Assert.Equal(BinaryEncoding.LittleEndian, target.Encoding);
    }
}
