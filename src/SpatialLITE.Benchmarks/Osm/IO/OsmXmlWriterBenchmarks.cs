using BenchmarkDotNet.Attributes;
using SpatialLite.Osm;
using SpatialLite.Osm.IO;
using SpatialLITE.Osm.IO.Xml;

namespace SpatialLITE.Benchmarks.Osm.IO;

[MemoryDiagnoser]
public class OsmXmlWriterBenchmarks
{
    private readonly Node _simpleNode;
    private readonly Node _nodeWithTags;
    private readonly Node _nodeWithMetadata;
    private readonly Way _simpleWay;
    private readonly Way _wayWithTags;
    private readonly Relation _simpleRelation;
    private readonly Relation _relationWithTags;
    private readonly List<IOsmEntity> _mixedEntities;

    public OsmXmlWriterBenchmarks()
    {
        // Create simple node
        _simpleNode = new Node
        {
            Id = 1,
            Latitude = 50.086758,
            Longitude = 14.4092038,
            Tags = new TagsCollection()
        };

        // Create node with tags
        _nodeWithTags = new Node
        {
            Id = 2,
            Latitude = 50.0860597,
            Longitude = 14.4143866,
            Tags = new TagsCollection
            {
                { "highway", "traffic_signals" },
                { "name", "Test Node" },
                { "crossing", "traffic_signals" }
            }
        };

        // Create node with metadata
        _nodeWithMetadata = new Node
        {
            Id = 3,
            Latitude = 50.0886819,
            Longitude = 14.415577,
            Tags = new TagsCollection { { "highway", "traffic_signals" } },
            Metadata = new EntityMetadata
            {
                Timestamp = new DateTime(2008, 9, 25, 14, 4, 49, DateTimeKind.Utc),
                Uid = 17615,
                User = "Test User",
                Visible = true,
                Version = 5,
                Changeset = 695161
            }
        };

        // Create simple way
        _simpleWay = new Way
        {
            Id = 100,
            Nodes = new List<long> { 1, 2, 3, 4, 5 },
            Tags = new TagsCollection()
        };

        // Create way with tags
        _wayWithTags = new Way
        {
            Id = 101,
            Nodes = new List<long> { 1, 2, 3, 4, 5, 6, 7, 8 },
            Tags = new TagsCollection
            {
                { "highway", "primary" },
                { "name", "Test Street" },
                { "maxspeed", "50" },
                { "surface", "asphalt" }
            }
        };

        // Create simple relation
        _simpleRelation = new Relation
        {
            Id = 200,
            Members = new List<RelationMember>
            {
                new() { MemberType = EntityType.Way, MemberId = 100, Role = "outer" },
                new() { MemberType = EntityType.Node, MemberId = 1, Role = "" }
            },
            Tags = new TagsCollection()
        };

        // Create relation with tags
        _relationWithTags = new Relation
        {
            Id = 201,
            Members = new List<RelationMember>
            {
                new() { MemberType = EntityType.Way, MemberId = 100, Role = "outer" },
                new() { MemberType = EntityType.Way, MemberId = 101, Role = "inner" },
                new() { MemberType = EntityType.Node, MemberId = 1, Role = "entrance" },
                new() { MemberType = EntityType.Relation, MemberId = 200, Role = "subarea" }
            },
            Tags = new TagsCollection
            {
                { "type", "multipolygon" },
                { "name", "Test Area" },
                { "area", "yes" }
            }
        };

        // Create mixed entities collection
        _mixedEntities = new List<IOsmEntity>
        {
            _simpleNode,
            _nodeWithTags,
            _simpleWay,
            _wayWithTags,
            _simpleRelation,
            _relationWithTags
        };
    }

    [Benchmark]
    public string WriteSimpleNode()
    {
        using var stream = new MemoryStream();
        using (var writer = new OsmXmlWriter(stream, new OsmWriterSettings { WriteMetadata = false }))
        {
            writer.Write(_simpleNode);
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    [Benchmark]
    public string WriteNodeWithTags()
    {
        using var stream = new MemoryStream();
        using (var writer = new OsmXmlWriter(stream, new OsmWriterSettings { WriteMetadata = false }))
        {
            writer.Write(_nodeWithTags);
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    [Benchmark]
    public string WriteNodeWithMetadata()
    {
        using var stream = new MemoryStream();
        using (var writer = new OsmXmlWriter(stream, new OsmWriterSettings { WriteMetadata = true }))
        {
            writer.Write(_nodeWithMetadata);
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    [Benchmark]
    public string WriteSimpleWay()
    {
        using var stream = new MemoryStream();
        using (var writer = new OsmXmlWriter(stream, new OsmWriterSettings { WriteMetadata = false }))
        {
            writer.Write(_simpleWay);
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    [Benchmark]
    public string WriteWayWithTags()
    {
        using var stream = new MemoryStream();
        using (var writer = new OsmXmlWriter(stream, new OsmWriterSettings { WriteMetadata = false }))
        {
            writer.Write(_wayWithTags);
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    [Benchmark]
    public string WriteSimpleRelation()
    {
        using var stream = new MemoryStream();
        using (var writer = new OsmXmlWriter(stream, new OsmWriterSettings { WriteMetadata = false }))
        {
            writer.Write(_simpleRelation);
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    [Benchmark]
    public string WriteRelationWithTags()
    {
        using var stream = new MemoryStream();
        using (var writer = new OsmXmlWriter(stream, new OsmWriterSettings { WriteMetadata = false }))
        {
            writer.Write(_relationWithTags);
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    [Benchmark]
    public string WriteMixedEntities()
    {
        using var stream = new MemoryStream();
        using (var writer = new OsmXmlWriter(stream, new OsmWriterSettings { WriteMetadata = false }))
        {
            foreach (var entity in _mixedEntities)
            {
                writer.Write(entity);
            }
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }
}
