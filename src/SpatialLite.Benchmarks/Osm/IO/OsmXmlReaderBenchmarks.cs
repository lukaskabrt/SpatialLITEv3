using BenchmarkDotNet.Attributes;
using SpatialLite.Benchmarks.Data;
using SpatialLite.Osm;
using SpatialLite.Osm.IO.Xml;
using System.IO.Compression;

namespace SpatialLite.Benchmarks.Osm.IO;

[MemoryDiagnoser]
public class OsmXmlReaderBenchmarks
{
    private readonly MemoryStream _xml = new();
    private readonly MemoryStream _xmlWithMetadata = new();

    [GlobalSetup(Target = nameof(ReadFileWithoutMetadata))]
    public void SetupWithoutMetadata()
    {
        LoadXmlData(_xml, "andorra.osm.gz");
    }

    [GlobalSetup(Target = nameof(ReadFileWithMetadata))]
    public void SetupWithMetadata()
    {
        LoadXmlData(_xmlWithMetadata, "andorra-metadata.osm.gz");
    }

    private static void LoadXmlData(MemoryStream stream, string fileName)
    {
        using var source = BenchmarkDataReader.OsmXml.Open(fileName);
        using var gzipStream = new GZipStream(source, CompressionMode.Decompress);
        gzipStream.CopyTo(stream);
    }

    [Benchmark]
    public int ReadFileWithMetadata()
    {
        _xmlWithMetadata.Position = 0;
        using var reader = new OsmXmlReader(_xmlWithMetadata, new OsmXmlReaderSettings { ReadMetadata = true });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }

    [Benchmark]
    public int ReadFileWithoutMetadata()
    {
        _xml.Position = 0;
        using var reader = new OsmXmlReader(_xml, new OsmXmlReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }
}
