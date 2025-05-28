using System.Reflection;

namespace SpatialLITE.UnitTests.Data;

public static class TestDataReader
{
    public static Stream Open(string name)
    {
        var assembly = typeof(TestDataReader).GetTypeInfo().Assembly;
        return assembly.GetManifestResourceStream("SpatialLITE.UnitTests.Data.Core.IO." + name)
            ?? throw new Exception($"Resource {name} not found.");
    }

    public static byte[] Read(string name)
    {
        var assembly = typeof(TestDataReader).GetTypeInfo().Assembly;

        using var stream = new MemoryStream();
        using var resourceStream = assembly.GetManifestResourceStream("SpatialLITE.UnitTests.Data.Core.IO." + name)
            ?? throw new Exception($"Resource {name} not found.");

        resourceStream.CopyTo(stream);

        return stream.ToArray();
    }
}
