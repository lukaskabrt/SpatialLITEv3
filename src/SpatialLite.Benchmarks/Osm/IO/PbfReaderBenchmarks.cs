using BenchmarkDotNet.Attributes;
using SpatialLite.Osm;
using SpatialLite.Osm.IO;
using SpatialLite.Benchmarks.Data;
using SpatialLite.Osm.IO.Pbf;

namespace SpatialLite.Benchmarks.Osm.IO;

[MemoryDiagnoser]
public class PbfReaderBenchmarks
{
    private readonly MemoryStream _pbf = new();
    private readonly MemoryStream _pbfWithMetadata = new();

    [GlobalSetup(Target = nameof(ReadFileWithoutMetadata))]
    public void SetupWithoutMetadata()
    {
        BenchmarkDataReader.OsmPbf.Open("andorra.osm.pbf").CopyTo(_pbf);
    }

    [GlobalSetup(Target = nameof(ReadFileWithMetadata))]
    public void SetupWithMetadata()
    {
        BenchmarkDataReader.OsmPbf.Open("andorra-metadata.osm.pbf").CopyTo(_pbfWithMetadata);
    }

    [Benchmark]
    public int ReadFileWithMetadata()
    {
        _pbfWithMetadata.Position = 0;
        using var reader = new PbfReader(_pbfWithMetadata, new OsmReaderSettings { ReadMetadata = true });

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
        _pbf.Position = 0;
        using var reader = new PbfReader(_pbf, new OsmReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }
}
