using BenchmarkDotNet.Attributes;
using SpatialLite.Osm;
using SpatialLite.Osm.IO;
using SpatialLITE.Osm.IO.Xml;

namespace SpatialLITE.Benchmarks.Osm.IO;

[MemoryDiagnoser]
public class OsmXmlWriterBenchmarks
{
    private List<Node> _multipleNodes = null!;
    private List<Node> _multipleNodesWithTags = null!;
    private List<Node> _multipleNodesWithMetadata = null!;
    private List<Way> _multipleWays = null!;
    private List<Way> _multipleWaysWithTags = null!;
    private List<Relation> _multipleRelations = null!;
    private List<Relation> _multipleRelationsWithTags = null!;
    private List<IOsmEntity> _complexMixedEntities = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Create multiple simple nodes
        _multipleNodes = new List<Node>();
        for (int i = 1; i <= 10; i++)
        {
            _multipleNodes.Add(new Node
            {
                Id = i,
                Latitude = 50.086758 + (i * 0.001),
                Longitude = 14.4092038 + (i * 0.001),
                Tags = new TagsCollection()
            });
        }

        // Create multiple nodes with tags
        _multipleNodesWithTags = new List<Node>();
        var tagTypes = new[] { "highway", "amenity", "shop", "leisure", "tourism", "barrier", "natural", "public_transport", "man_made", "building" };
        var tagValues = new[] { "traffic_signals", "restaurant", "supermarket", "park", "hotel", "bollard", "tree", "stop_position", "tower", "house" };
        for (int i = 1; i <= 10; i++)
        {
            _multipleNodesWithTags.Add(new Node
            {
                Id = i,
                Latitude = 50.086758 + (i * 0.001),
                Longitude = 14.4092038 + (i * 0.001),
                Tags = new TagsCollection
                {
                    { tagTypes[i - 1], tagValues[i - 1] },
                    { "name", $"Node {i}" },
                    { "source", "test_data" }
                }
            });
        }

        // Create multiple nodes with metadata
        _multipleNodesWithMetadata = new List<Node>();
        for (int i = 1; i <= 10; i++)
        {
            _multipleNodesWithMetadata.Add(new Node
            {
                Id = i,
                Latitude = 50.086758 + (i * 0.001),
                Longitude = 14.4092038 + (i * 0.001),
                Tags = new TagsCollection { { "highway", "traffic_signals" } },
                Metadata = new EntityMetadata
                {
                    Timestamp = new DateTime(2008, 9, 25, 14, 4, 49, DateTimeKind.Utc).AddMinutes(i),
                    Uid = 17615 + i,
                    User = $"TestUser{i}",
                    Visible = true,
                    Version = i,
                    Changeset = 695161 + i
                }
            });
        }

        // Create multiple simple ways
        _multipleWays = new List<Way>();
        for (int i = 1; i <= 10; i++)
        {
            var nodes = new List<long>();
            for (int j = 1; j <= 5; j++)
            {
                nodes.Add((i - 1) * 5 + j);
            }

            _multipleWays.Add(new Way
            {
                Id = i,
                Nodes = nodes,
                Tags = new TagsCollection()
            });
        }

        // Create multiple ways with tags
        _multipleWaysWithTags = new List<Way>();
        var wayTypes = new[] { "highway", "waterway", "railway", "boundary", "landuse", "natural", "aeroway", "power", "barrier", "leisure" };
        var wayValues = new[] { "primary", "river", "rail", "administrative", "residential", "coastline", "runway", "line", "fence", "park" };
        for (int i = 1; i <= 10; i++)
        {
            var nodes = new List<long>();
            for (int j = 1; j <= 8; j++)
            {
                nodes.Add((i - 1) * 8 + j);
            }

            _multipleWaysWithTags.Add(new Way
            {
                Id = i,
                Nodes = nodes,
                Tags = new TagsCollection
                {
                    { wayTypes[i - 1], wayValues[i - 1] },
                    { "name", $"Way {i}" },
                    { "maxspeed", (50 + i * 10).ToString(System.Globalization.CultureInfo.InvariantCulture) },
                    { "surface", "asphalt" }
                }
            });
        }

        // Create multiple simple relations
        _multipleRelations = new List<Relation>();
        for (int i = 1; i <= 10; i++)
        {
            _multipleRelations.Add(new Relation
            {
                Id = i,
                Members = new List<RelationMember>
                {
                    new() { MemberType = EntityType.Way, MemberId = i, Role = "outer" },
                    new() { MemberType = EntityType.Node, MemberId = i, Role = "" }
                },
                Tags = new TagsCollection()
            });
        }

        // Create multiple relations with tags
        _multipleRelationsWithTags = new List<Relation>();
        var relationTypes = new[] { "multipolygon", "boundary", "route", "restriction", "site", "collection", "waterway", "public_transport", "enforcement", "associatedStreet" };
        for (int i = 1; i <= 10; i++)
        {
            _multipleRelationsWithTags.Add(new Relation
            {
                Id = i,
                Members = new List<RelationMember>
                {
                    new() { MemberType = EntityType.Way, MemberId = i, Role = "outer" },
                    new() { MemberType = EntityType.Way, MemberId = i + 10, Role = "inner" },
                    new() { MemberType = EntityType.Node, MemberId = i, Role = "entrance" },
                    new() { MemberType = EntityType.Relation, MemberId = (i % 5) + 1, Role = "subarea" }
                },
                Tags = new TagsCollection
                {
                    { "type", relationTypes[i - 1] },
                    { "name", $"Relation {i}" },
                    { "admin_level", "8" }
                }
            });
        }

        // Create complex mixed entities
        _complexMixedEntities = new List<IOsmEntity>();

        // Add 10 nodes with various properties
        for (int i = 1; i <= 10; i++)
        {
            var node = new Node
            {
                Id = i,
                Latitude = 50.086758 + (i * 0.001),
                Longitude = 14.4092038 + (i * 0.001),
                Tags = new TagsCollection()
            };

            if (i % 3 == 0)
            {
                node.Tags.Add("highway", "traffic_signals");
                node.Tags.Add("name", $"Junction {i}");
            }
            else if (i % 2 == 0)
            {
                node.Tags.Add("amenity", "restaurant");
                node.Tags.Add("name", $"Restaurant {i}");
            }

            _complexMixedEntities.Add(node);
        }

        // Add 10 ways with various properties
        for (int i = 1; i <= 10; i++)
        {
            var nodes = new List<long>();
            for (int j = 1; j <= 6; j++)
            {
                nodes.Add((i - 1) * 6 + j);
            }

            var way = new Way
            {
                Id = 100 + i,
                Nodes = nodes,
                Tags = new TagsCollection()
            };

            if (i % 2 == 0)
            {
                way.Tags.Add("highway", "primary");
                way.Tags.Add("name", $"Highway {i}");
                way.Tags.Add("maxspeed", "80");
            }
            else
            {
                way.Tags.Add("waterway", "river");
                way.Tags.Add("name", $"River {i}");
            }

            _complexMixedEntities.Add(way);
        }

        // Add 5 relations with various properties
        for (int i = 1; i <= 5; i++)
        {
            var relation = new Relation
            {
                Id = 200 + i,
                Members = new List<RelationMember>
                {
                    new() { MemberType = EntityType.Way, MemberId = 100 + i, Role = "outer" },
                    new() { MemberType = EntityType.Way, MemberId = 100 + i + 5, Role = "inner" },
                    new() { MemberType = EntityType.Node, MemberId = i, Role = "entrance" }
                },
                Tags = new TagsCollection
                {
                    { "type", "multipolygon" },
                    { "landuse", "commercial" },
                    { "name", $"Commercial Area {i}" }
                }
            };

            _complexMixedEntities.Add(relation);
        }
    }

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
