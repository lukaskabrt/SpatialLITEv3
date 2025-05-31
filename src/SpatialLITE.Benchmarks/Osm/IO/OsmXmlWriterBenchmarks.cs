using BenchmarkDotNet.Attributes;
using SpatialLite.Osm;
using SpatialLite.Osm.IO;
using SpatialLITE.Osm.IO.Xml;

namespace SpatialLITE.Benchmarks.Osm.IO;

[MemoryDiagnoser]
public partial class OsmXmlWriterBenchmarks
{
    private List<Node> _multipleNodes = null!;
    private List<Node> _multipleNodesWithTags = null!;
    private List<Node> _multipleNodesWithMetadata = null!;
    private List<Way> _multipleWays = null!;
    private List<Way> _multipleWaysWithTags = null!;
    private List<Relation> _multipleRelations = null!;
    private List<Relation> _multipleRelationsWithTags = null!;
    private List<IOsmEntity> _complexMixedEntities = null!;

    [GlobalCleanup]
    public void Cleanup()
    {
        _multipleNodes?.Clear();
        _multipleNodesWithTags?.Clear();
        _multipleNodesWithMetadata?.Clear();
        _multipleWays?.Clear();
        _multipleWaysWithTags?.Clear();
        _multipleRelations?.Clear();
        _multipleRelationsWithTags?.Clear();
        _complexMixedEntities?.Clear();
    }

    [Benchmark]
    public int WriteMultipleNodes()
    {
        using var stream = new MemoryStream();
        using var writer = new OsmXmlWriter(stream, new OsmWriterSettings { WriteMetadata = false });

        int count = 0;
        foreach (var node in _multipleNodes)
        {
            writer.Write(node);
            count++;
        }

        return count;
    }

    [Benchmark]
    public int WriteMultipleNodesWithTags()
    {
        using var stream = new MemoryStream();
        using var writer = new OsmXmlWriter(stream, new OsmWriterSettings { WriteMetadata = false });

        int count = 0;
        foreach (var node in _multipleNodesWithTags)
        {
            writer.Write(node);
            count++;
        }

        return count;
    }

    [Benchmark]
    public int WriteMultipleNodesWithMetadata()
    {
        using var stream = new MemoryStream();
        using var writer = new OsmXmlWriter(stream, new OsmWriterSettings { WriteMetadata = true });

        int count = 0;
        foreach (var node in _multipleNodesWithMetadata)
        {
            writer.Write(node);
            count++;
        }

        return count;
    }

    [Benchmark]
    public int WriteMultipleWays()
    {
        using var stream = new MemoryStream();
        using var writer = new OsmXmlWriter(stream, new OsmWriterSettings { WriteMetadata = false });

        int count = 0;
        foreach (var way in _multipleWays)
        {
            writer.Write(way);
            count++;
        }

        return count;
    }

    [Benchmark]
    public int WriteMultipleWaysWithTags()
    {
        using var stream = new MemoryStream();
        using var writer = new OsmXmlWriter(stream, new OsmWriterSettings { WriteMetadata = false });

        int count = 0;
        foreach (var way in _multipleWaysWithTags)
        {
            writer.Write(way);
            count++;
        }

        return count;
    }

    [Benchmark]
    public int WriteMultipleRelations()
    {
        using var stream = new MemoryStream();
        using var writer = new OsmXmlWriter(stream, new OsmWriterSettings { WriteMetadata = false });

        int count = 0;
        foreach (var relation in _multipleRelations)
        {
            writer.Write(relation);
            count++;
        }

        return count;
    }

    [Benchmark]
    public int WriteMultipleRelationsWithTags()
    {
        using var stream = new MemoryStream();
        using var writer = new OsmXmlWriter(stream, new OsmWriterSettings { WriteMetadata = false });

        int count = 0;
        foreach (var relation in _multipleRelationsWithTags)
        {
            writer.Write(relation);
            count++;
        }

        return count;
    }

    [Benchmark]
    public int WriteComplexMixedEntities()
    {
        using var stream = new MemoryStream();
        using var writer = new OsmXmlWriter(stream, new OsmWriterSettings { WriteMetadata = false });

        int count = 0;
        foreach (var entity in _complexMixedEntities)
        {
            writer.Write(entity);
            count++;
        }

        return count;
    }
}