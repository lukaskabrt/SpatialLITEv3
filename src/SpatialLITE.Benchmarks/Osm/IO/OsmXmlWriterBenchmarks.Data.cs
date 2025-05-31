using BenchmarkDotNet.Attributes;
using SpatialLite.Osm;

namespace SpatialLITE.Benchmarks.Osm.IO;

public partial class OsmXmlWriterBenchmarks
{
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
        for (int i = 1; i <= 10; i++)
        {
            var tagCollection = new TagsCollection();
            tagCollection.Add("highway", i % 2 == 0 ? "traffic_signals" : "crossing");
            tagCollection.Add("name", $"Node {i}");
            if (i % 3 == 0)
            {
                tagCollection.Add("type", "special");
            }

            _multipleNodesWithTags.Add(new Node
            {
                Id = i,
                Latitude = 50.086758 + (i * 0.001),
                Longitude = 14.4092038 + (i * 0.001),
                Tags = tagCollection
            });
        }

        // Create multiple nodes with metadata
        _multipleNodesWithMetadata = new List<Node>();
        for (int i = 1; i <= 10; i++)
        {
            var tagCollection = new TagsCollection();
            tagCollection.Add("highway", "traffic_signals");
            if (i % 2 == 0)
            {
                tagCollection.Add("name", $"Intersection {i}");
            }

            _multipleNodesWithMetadata.Add(new Node
            {
                Id = i,
                Latitude = 50.086758 + (i * 0.001),
                Longitude = 14.4092038 + (i * 0.001),
                Tags = tagCollection,
                Metadata = new EntityMetadata
                {
                    Version = i % 3 + 1,
                    Timestamp = DateTime.Parse("2023-01-01T12:00:00Z", System.Globalization.CultureInfo.InvariantCulture).AddDays(i),
                    Changeset = 123 + i,
                    Uid = i,
                    User = $"testuser{i}"
                }
            });
        }

        // Create multiple simple ways
        _multipleWays = new List<Way>();
        for (int i = 101; i <= 110; i++)
        {
            var nodeRefs = new List<long>();
            for (int j = 1; j <= 3; j++)
            {
                nodeRefs.Add((i - 100) * 10 + j);
            }

            _multipleWays.Add(new Way
            {
                Id = i,
                Nodes = nodeRefs,
                Tags = new TagsCollection()
            });
        }

        // Create multiple ways with tags
        _multipleWaysWithTags = new List<Way>();
        var roadTypes = new[] { "residential", "primary", "secondary", "tertiary", "unclassified" };
        for (int i = 101; i <= 110; i++)
        {
            var nodeRefs = new List<long>();
            for (int j = 1; j <= 3; j++)
            {
                nodeRefs.Add((i - 100) * 10 + j);
            }

            var tagCollection = new TagsCollection();
            tagCollection.Add("highway", roadTypes[(i - 101) % roadTypes.Length]);
            tagCollection.Add("name", $"Street {i}");
            if (i % 2 == 0)
            {
                tagCollection.Add("maxspeed", "50");
            }

            _multipleWaysWithTags.Add(new Way
            {
                Id = i,
                Nodes = nodeRefs,
                Tags = tagCollection
            });
        }

        // Create multiple simple relations
        _multipleRelations = new List<Relation>();
        for (int i = 201; i <= 205; i++)
        {
            var members = new List<RelationMember>
            {
                new() { MemberType = EntityType.Way, MemberId = i - 100, Role = "outer" },
                new() { MemberType = EntityType.Way, MemberId = i - 99, Role = "inner" }
            };

            _multipleRelations.Add(new Relation
            {
                Id = i,
                Members = members,
                Tags = new TagsCollection()
            });
        }

        // Create multiple relations with tags
        _multipleRelationsWithTags = new List<Relation>();
        var relationTypes = new[] { "multipolygon", "route", "building", "restriction" };
        for (int i = 201; i <= 205; i++)
        {
            var members = new List<RelationMember>
            {
                new() { MemberType = EntityType.Way, MemberId = i - 100, Role = "outer" },
                new() { MemberType = EntityType.Node, MemberId = i - 200, Role = "entrance" }
            };

            var tagCollection = new TagsCollection();
            tagCollection.Add("type", relationTypes[(i - 201) % relationTypes.Length]);
            tagCollection.Add("name", $"Complex {i}");
            if (i % 2 == 1)
            {
                tagCollection.Add("building", "yes");
            }

            _multipleRelationsWithTags.Add(new Relation
            {
                Id = i,
                Members = members,
                Tags = tagCollection
            });
        }

        // Create complex mixed entities
        _complexMixedEntities = new List<IOsmEntity>();

        // Add nodes with various configurations
        for (int i = 1; i <= 10; i++)
        {
            var tagCollection = new TagsCollection();
            if (i % 3 == 0)
            {
                tagCollection.Add("amenity", i % 2 == 0 ? "restaurant" : "cafe");
                tagCollection.Add("name", $"Place {i}");
            }

            var node = new Node
            {
                Id = i,
                Latitude = 50.086758 + (i * 0.001),
                Longitude = 14.4092038 + (i * 0.001),
                Tags = tagCollection
            };

            if (i % 4 == 0)
            {
                node.Metadata = new EntityMetadata
                {
                    Version = 2,
                    Timestamp = DateTime.Parse("2023-01-01T12:00:00Z", System.Globalization.CultureInfo.InvariantCulture),
                    Changeset = 456,
                    Uid = 1,
                    User = "testuser"
                };
            }

            _complexMixedEntities.Add(node);
        }

        // Add ways with various configurations
        for (int i = 101; i <= 110; i++)
        {
            var nodeRefs = new List<long>();
            for (int j = 1; j <= 4; j++)
            {
                nodeRefs.Add(j);
            }

            var tagCollection = new TagsCollection();
            if (i % 2 == 0)
            {
                tagCollection.Add("highway", "residential");
                tagCollection.Add("name", $"Street {i}");
            }

            if (i % 3 == 0)
            {
                tagCollection.Add("surface", "asphalt");
            }

            var way = new Way
            {
                Id = i,
                Nodes = nodeRefs,
                Tags = tagCollection
            };

            _complexMixedEntities.Add(way);
        }

        // Add relations with various configurations
        for (int i = 201; i <= 205; i++)
        {
            var members = new List<RelationMember>
            {
                new() { MemberType = EntityType.Way, MemberId = 101 + (i - 201), Role = "outer" },
                new() { MemberType = EntityType.Node, MemberId = 1 + (i - 201), Role = "entrance" }
            };

            var tagCollection = new TagsCollection();
            tagCollection.Add("type", "multipolygon");
            tagCollection.Add("name", $"Building {i}");
            if (i % 2 == 1)
            {
                tagCollection.Add("building", "commercial");
            }

            var relation = new Relation
            {
                Id = i,
                Members = members,
                Tags = tagCollection
            };

            _complexMixedEntities.Add(relation);
        }
    }
}