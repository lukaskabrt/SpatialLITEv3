using System.Reflection;

namespace SpatialLITE.UnitTests.Data;

public class TestDataReader
{
    private readonly string _resourcePrefix;
    private readonly Assembly _assembly;

    public TestDataReader(string resourcePrefix)
    {
        _resourcePrefix = resourcePrefix;
        _assembly = typeof(TestDataReader).GetTypeInfo().Assembly;
    }

    public Stream Open(string name)
    {
        return _assembly.GetManifestResourceStream(_resourcePrefix + name)
            ?? throw new Exception($"Resource {_resourcePrefix + name} not found.");
    }

    public byte[] Read(string name)
    {
        using var stream = new MemoryStream();
        using var resourceStream = _assembly.GetManifestResourceStream(_resourcePrefix + name)
            ?? throw new Exception($"Resource {_resourcePrefix + name} not found.");

        resourceStream.CopyTo(stream);
        return stream.ToArray();
    }

    public static readonly TestDataReader CoreIO = new("SpatialLITE.UnitTests.Data.Core.IO.");

    public static readonly TestDataReader OsmXml = new("SpatialLITE.UnitTests.Data.Osm.Xml.");
}
