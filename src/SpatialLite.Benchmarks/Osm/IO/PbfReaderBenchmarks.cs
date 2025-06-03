using BenchmarkDotNet.Attributes;
using SpatialLite.Benchmarks.Data;
using SpatialLite.Osm;
using SpatialLite.Osm.IO;
using SpatialLite.Osm.IO.Pbf;

namespace SpatialLite.Benchmarks.Osm.IO;

[MemoryDiagnoser]
public class PbfReaderBenchmarks
{
    private byte[] _pbf = [];
    private byte[] _pbfWithMetadata = [];

    [GlobalSetup(Target = nameof(ReadFileWithoutMetadata))]
    public void SetupWithoutMetadata()
    {
        _pbf = BenchmarkDataReader.OsmPbf.Read("andorra.osm.pbf");
    }

    [GlobalSetup(Target = nameof(ReadFileWithMetadata))]
    public void SetupWithMetadata()
    {
        _pbfWithMetadata = BenchmarkDataReader.OsmPbf.Read("andorra-metadata.osm.pbf");
    }

    [Benchmark]
    public int ReadFileWithMetadata()
    {
        using var stream = new MemoryStream(_pbfWithMetadata);
        using var reader = new PbfReader(stream, new OsmReaderSettings { ReadMetadata = true });

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
        using var stream = new MemoryStream(_pbf);
        using var reader = new PbfReader(stream, new OsmReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }
}
