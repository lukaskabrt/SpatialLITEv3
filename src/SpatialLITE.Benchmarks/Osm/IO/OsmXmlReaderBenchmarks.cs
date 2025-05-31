using BenchmarkDotNet.Attributes;
using SpatialLite.Osm;
using SpatialLITE.Osm.IO.Xml;

namespace SpatialLITE.Benchmarks.Osm.IO;

[MemoryDiagnoser]
public partial class OsmXmlReaderBenchmarks
{
    private MemoryStream _multipleNodesStream = null!;
    private MemoryStream _multipleNodesWithTagsStream = null!;
    private MemoryStream _multipleNodesWithMetadataStream = null!;
    private MemoryStream _multipleWaysStream = null!;
    private MemoryStream _multipleWaysWithTagsStream = null!;
    private MemoryStream _multipleRelationsStream = null!;
    private MemoryStream _multipleRelationsWithTagsStream = null!;
    private MemoryStream _complexMixedStream = null!;

    [GlobalCleanup]
    public void Cleanup()
    {
        _multipleNodesStream?.Dispose();
        _multipleNodesWithTagsStream?.Dispose();
        _multipleNodesWithMetadataStream?.Dispose();
        _multipleWaysStream?.Dispose();
        _multipleWaysWithTagsStream?.Dispose();
        _multipleRelationsStream?.Dispose();
        _multipleRelationsWithTagsStream?.Dispose();
        _complexMixedStream?.Dispose();
    }

    [Benchmark]
    public int ReadMultipleNodes()
    {
        // Reset stream position and create new reader for this iteration
        _multipleNodesStream.Position = 0;
        using var reader = new OsmXmlReader(_multipleNodesStream, new OsmXmlReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }

    [Benchmark]
    public int ReadMultipleNodesWithTags()
    {
        // Reset stream position and create new reader for this iteration
        _multipleNodesWithTagsStream.Position = 0;
        using var reader = new OsmXmlReader(_multipleNodesWithTagsStream, new OsmXmlReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }

    [Benchmark]
    public int ReadMultipleNodesWithMetadata()
    {
        // Reset stream position and create new reader for this iteration
        _multipleNodesWithMetadataStream.Position = 0;
        using var reader = new OsmXmlReader(_multipleNodesWithMetadataStream, new OsmXmlReaderSettings { ReadMetadata = true });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }

    [Benchmark]
    public int ReadMultipleWays()
    {
        // Reset stream position and create new reader for this iteration
        _multipleWaysStream.Position = 0;
        using var reader = new OsmXmlReader(_multipleWaysStream, new OsmXmlReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }

    [Benchmark]
    public int ReadMultipleWaysWithTags()
    {
        // Reset stream position and create new reader for this iteration
        _multipleWaysWithTagsStream.Position = 0;
        using var reader = new OsmXmlReader(_multipleWaysWithTagsStream, new OsmXmlReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }

    [Benchmark]
    public int ReadMultipleRelations()
    {
        // Reset stream position and create new reader for this iteration
        _multipleRelationsStream.Position = 0;
        using var reader = new OsmXmlReader(_multipleRelationsStream, new OsmXmlReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }

    [Benchmark]
    public int ReadMultipleRelationsWithTags()
    {
        // Reset stream position and create new reader for this iteration
        _multipleRelationsWithTagsStream.Position = 0;
        using var reader = new OsmXmlReader(_multipleRelationsWithTagsStream, new OsmXmlReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }

    [Benchmark]
    public int ReadComplexMixedData()
    {
        // Reset stream position and create new reader for this iteration
        _complexMixedStream.Position = 0;
        using var reader = new OsmXmlReader(_complexMixedStream, new OsmXmlReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }
}
