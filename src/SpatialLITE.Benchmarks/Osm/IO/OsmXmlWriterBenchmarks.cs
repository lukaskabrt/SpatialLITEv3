using BenchmarkDotNet.Attributes;
using SpatialLite.Osm;
using SpatialLite.Osm.IO;
using SpatialLITE.Benchmarks.Data;
using SpatialLITE.Osm.IO.Xml;
using System.IO.Compression;

namespace SpatialLITE.Benchmarks.Osm.IO;

[MemoryDiagnoser]
public partial class OsmXmlWriterBenchmarks
{
    private readonly MemoryStream _xml = new(100 * 1024 * 1024); // 100 MB buffer for the XML data
    private readonly List<IOsmEntity> _entities = new();
    private readonly List<IOsmEntity> _entitiesWithMetadata = new();

    [GlobalSetup(Target = nameof(WriteFileWithoutMetadata))]
    public void SetupWithoutMetadata()
    {
        LoadXmlData(_entities, "andorra.osm.gz", readMetadata: false);
    }

    [GlobalSetup(Target = nameof(WriteFileWithMetadata))]
    public void SetupWithMetadata()
    {
        LoadXmlData(_entitiesWithMetadata, "andorra-metadata.osm.gz", readMetadata: true);
    }

    private static void LoadXmlData(List<IOsmEntity> target, string fileName, bool readMetadata)
    {
        using var source = BenchmarkDataReader.OsmXml.Open(fileName);
        using var gzipStream = new GZipStream(source, CompressionMode.Decompress);

        IOsmEntity? entity;
        using var reader = new OsmXmlReader(gzipStream, new OsmXmlReaderSettings { ReadMetadata = readMetadata });
        while ((entity = reader.Read()) != null)
        {
            target.Add(entity);
        }
    }

    [Benchmark]
    public long WriteFileWithMetadata()
    {
        _xml.Position = 0;
        using var writer = new OsmXmlWriter(_xml, new OsmWriterSettings { WriteMetadata = true });

        foreach (var entity in _entitiesWithMetadata)
        {
            writer.Write(entity);
        }

        writer.Flush();

        return _xml.Position;
    }

    [Benchmark]
    public long WriteFileWithoutMetadata()
    {
        _xml.Position = 0;
        using var writer = new OsmXmlWriter(_xml, new OsmWriterSettings { WriteMetadata = false });

        foreach (var entity in _entities)
        {
            writer.Write(entity);
        }

        writer.Flush();

        return _xml.Position;
    }
}
