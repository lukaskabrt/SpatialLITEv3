using BenchmarkDotNet.Attributes;
using SpatialLite.Benchmarks.Data;
using SpatialLite.Osm;
using SpatialLite.Osm.IO;
using SpatialLite.Osm.IO.Pbf;

namespace SpatialLite.Benchmarks.Osm.IO;

[MemoryDiagnoser]
public partial class PbfWriterBenchmarks
{
    private readonly MemoryStream _pbf = new(10 * 1024 * 1024); // 10 MB buffer for the PBF data
    private readonly List<IOsmEntity> _entities = new();
    private readonly List<IOsmEntity> _entitiesWithMetadata = new();

    [GlobalSetup(Target = nameof(WriteFileWithoutMetadata))]
    public void SetupWithoutMetadata()
    {
        LoadPbfData(_entities, "andorra.osm.pbf", readMetadata: false);
    }

    [GlobalSetup(Target = nameof(WriteFileWithMetadata))]
    public void SetupWithMetadata()
    {
        LoadPbfData(_entitiesWithMetadata, "andorra-metadata.osm.pbf", readMetadata: true);
    }

    private static void LoadPbfData(List<IOsmEntity> target, string fileName, bool readMetadata)
    {
        using var source = BenchmarkDataReader.OsmPbf.Open(fileName);

        IOsmEntity? entity;
        using var reader = new PbfReader(source, new OsmReaderSettings { ReadMetadata = readMetadata });
        while ((entity = reader.Read()) != null)
        {
            target.Add(entity);
        }
    }

    [Benchmark]
    public long WriteFileWithMetadata()
    {
        _pbf.Position = 0;
        using var writer = new PbfWriter(_pbf, new PbfWriterSettings { WriteMetadata = true, UseDenseFormat = true, Compression = CompressionMode.ZlibDeflate });

        foreach (var entity in _entitiesWithMetadata)
        {
            writer.Write(entity);
        }

        writer.Flush();

        return _pbf.Position;
    }

    [Benchmark]
    public long WriteFileWithoutMetadata()
    {
        _pbf.Position = 0;
        using var writer = new PbfWriter(_pbf, new PbfWriterSettings { WriteMetadata = false, UseDenseFormat = true, Compression = CompressionMode.ZlibDeflate });

        foreach (var entity in _entities)
        {
            writer.Write(entity);
        }

        writer.Flush();

        return _pbf.Position;
    }
}
