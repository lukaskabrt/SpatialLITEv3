using SpatialLite.Osm;
using SpatialLite.Osm.IO;
using SpatialLITE.Osm.IO.Xml;
using System.Xml.Linq;

namespace SpatialLITE.UnitTests.Osm.IO.Xml;

public class OsmXmlWriterTests : OsmIOTests
{
    //resolution for default granularity
    private const double Resolution = 1E-07;

    [Fact]
    public void Constructor_StreamSettings_SetsSettings()
    {
        OsmWriterSettings settings = new();

        using var stream = new MemoryStream();
        using var target = new OsmXmlWriter(stream, settings);
        Assert.Same(settings, target.Settings);
    }

    [Fact]
    public void Constructor_PathSettings_SetsSettings()
    {
        string path = Path.GetTempFileName();

        try
        {
            OsmWriterSettings settings = new();
            using var target = new OsmXmlWriter(path, settings);

            Assert.Same(settings, target.Settings);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void Constructor_PathSettings_CreatesOutputFile()
    {
        string filename = Path.GetTempFileName();

        try
        {
            OsmWriterSettings settings = new();
            using (OsmXmlWriter target = new(filename, settings))
            {
                ;
            }

            Assert.True(File.Exists(filename));
        }
        finally
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }
    }

    [Fact]
    public void Write_ThrowsArgumentExceptionIfWriteMetadataIsTrueButEntityDoesNotHaveMetadata()
    {
        var node = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = new TagsCollection() };

        MemoryStream stream = new();

        using OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = true });

        Assert.Throws<ArgumentException>(() => target.Write(node));
    }

    [Fact]
    public void Write_DoesNotThrowsExceptionIfMetadataContainsNullInsteadUsername()
    {
        var metadata = new EntityMetadata()
        {
            Timestamp = new DateTime(2010, 11, 19, 22, 5, 56, DateTimeKind.Utc),
            Uid = 127998,
            User = null,
            Visible = true,
            Version = 2,
            Changeset = 6410629
        };
        var node = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = new TagsCollection(), Metadata = metadata };

        MemoryStream stream = new();
        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = true }))
        {
            target.Write(node);
        }
    }

    [Fact]
    public void Write_IEntityInfo_WritesNode()
    {
        var node = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = new TagsCollection() };
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(node);
        }

        TestXmlOutput(stream, node, false);
    }

    [Fact]
    public void Write_IEntityInfo_WritesNodeWithTags()
    {
        var tags = new TagsCollection(new[] { new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2") });
        var nodeTags = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = tags };
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(nodeTags);
        }

        TestXmlOutput(stream, nodeTags, false);
    }

    [Fact]
    public void Write_IEntityInfo_WritesNodeWithMetadata()
    {
        var metadata = new EntityMetadata()
        {
            Timestamp = new DateTime(2010, 11, 19, 22, 5, 56, DateTimeKind.Utc),
            Uid = 127998,
            User = "Luk@s",
            Visible = true,
            Version = 2,
            Changeset = 6410629
        };
        var nodeProperties = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = new TagsCollection(), Metadata = metadata };
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = true }))
        {
            target.Write(nodeProperties);
        }

        TestXmlOutput(stream, nodeProperties, true);
    }

    [Fact]
    public void Write_IEntityInfo_DoesntWriteNodeMetadataIfWriteMedataIsFalse()
    {
        var metadata = new EntityMetadata()
        {
            Timestamp = new DateTime(2010, 11, 19, 22, 5, 56, DateTimeKind.Utc),
            Uid = 127998,
            User = "Luk@s",
            Visible = true,
            Version = 2,
            Changeset = 6410629
        };
        var nodeProperties = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = new TagsCollection(), Metadata = metadata };
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(nodeProperties);
        }

        stream = new MemoryStream(stream.ToArray());

        using (TextReader reader = new StreamReader(stream))
        {
            string? line = null;
            while ((line = reader.ReadLine()) != null)
            {
                Assert.DoesNotContain("timestamp", line);
            }
        }
    }

    [Fact]
    public void Write_IEntityInfo_WritesWay()
    {
        var way = new Way { Id = 1, Tags = new TagsCollection(), Nodes = new List<long> { 10, 11, 12 } };
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(way);
        }

        TestXmlOutput(stream, way, false);
    }

    [Fact]
    public void Write_IEntityInfo_WritesWayWithTags()
    {
        var tags = new TagsCollection(new[] { new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2") });
        var wayTags = new Way { Id = 1, Tags = tags, Nodes = new List<long> { 10, 11, 12 } };
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(wayTags);
        }

        TestXmlOutput(stream, wayTags, false);
    }

    [Fact]
    public void Write_IEntityInfo_WritesWayWithMetadata()
    {
        var metadata = new EntityMetadata()
        {
            Timestamp = new DateTime(2010, 11, 19, 22, 5, 56, DateTimeKind.Utc),
            Uid = 127998,
            User = "Luk@s",
            Visible = true,
            Version = 2,
            Changeset = 6410629
        };
        var wayProperties = new Way { Id = 1, Tags = new TagsCollection(), Nodes = new List<long> { 10, 11, 12 }, Metadata = metadata };
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = true }))
        {
            target.Write(wayProperties);
        }

        TestXmlOutput(stream, wayProperties, true);
    }

    [Fact]
    public void Write_IEntityInfo_DoesntWriteWayMetadataIfWriteMedataIsFalse()
    {
        var metadata = new EntityMetadata()
        {
            Timestamp = new DateTime(2010, 11, 19, 22, 5, 56, DateTimeKind.Utc),
            Uid = 127998,
            User = "Luk@s",
            Visible = true,
            Version = 2,
            Changeset = 6410629
        };
        var wayProperties = new Way { Id = 1, Tags = new TagsCollection(), Nodes = new List<long> { 10, 11, 12 }, Metadata = metadata };
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(wayProperties);
        }

        stream = new MemoryStream(stream.ToArray());

        using (TextReader reader = new StreamReader(stream))
        {
            string? line = null;
            while ((line = reader.ReadLine()) != null)
            {
                Assert.DoesNotContain("timestamp", line);
            }
        }
    }

    [Fact]
    public void Write_IEntityInfo_WritesRelationWithNode()
    {
        var relationNode = new Relation
        {
            Id = 1,
            Tags = new TagsCollection(),
            Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" } }
        };
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(relationNode);
        }

        TestXmlOutput(stream, relationNode, false);
    }

    [Fact]
    public void Write_IEntityInfo_WritesRelationWithWay()
    {
        var relationWay = new Relation
        {
            Id = 1,
            Tags = new TagsCollection(),
            Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Way, MemberId = 10, Role = "test" } }
        };
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(relationWay);
        }

        TestXmlOutput(stream, relationWay, false);
    }

    [Fact]
    public void Write_IEntityInfo_WritesRelationWithRelation()
    {
        var relationRelation = new Relation
        {
            Id = 1,
            Tags = new TagsCollection(),
            Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Relation, MemberId = 10, Role = "test" } }
        };
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(relationRelation);
        }

        TestXmlOutput(stream, relationRelation, false);
    }

    [Fact]
    public void Write_IEntityInfo_WritesRelationWithTags()
    {
        var tags = new TagsCollection(new[] { new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2") });
        var relationTags = new Relation
        {
            Id = 1,
            Tags = tags,
            Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" } }
        };
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(relationTags);
        }

        TestXmlOutput(stream, relationTags, false);
    }

    [Fact]
    public void Write_IEntityInfo_WritesRelationWithMetadata()
    {
        var metadata = new EntityMetadata()
        {
            Timestamp = new DateTime(2010, 11, 19, 22, 5, 56, DateTimeKind.Utc),
            Uid = 127998,
            User = "Luk@s",
            Visible = true,
            Version = 2,
            Changeset = 6410629
        };
        var relationNodeProperties = new Relation
        {
            Id = 1,
            Tags = new TagsCollection(),
            Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" } },
            Metadata = metadata
        };
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = true }))
        {
            target.Write(relationNodeProperties);
        }

        TestXmlOutput(stream, relationNodeProperties, true);
    }

    [Fact]
    public void Write_IEntityInfo_DoesntWriteRelationMetadataIfWriteMedataIsFalse()
    {
        var metadata = new EntityMetadata()
        {
            Timestamp = new DateTime(2010, 11, 19, 22, 5, 56, DateTimeKind.Utc),
            Uid = 127998,
            User = "Luk@s",
            Visible = true,
            Version = 2,
            Changeset = 6410629
        };
        var relationNodeProperties = new Relation
        {
            Id = 1,
            Tags = new TagsCollection(),
            Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" } },
            Metadata = metadata
        };
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(relationNodeProperties);
        }

        stream = new MemoryStream(stream.ToArray());

        using (TextReader reader = new StreamReader(stream))
        {
            string? line = null;
            while ((line = reader.ReadLine()) != null)
            {
                Assert.DoesNotContain("timestamp", line);
            }
        }
    }
    
    private void TestXmlOutput(MemoryStream xmlStream, IOsmEntity expected, bool readMetadata)
    {
        if (xmlStream.CanSeek)
        {
            xmlStream.Seek(0, SeekOrigin.Begin);
        }
        else
        {
            xmlStream = new MemoryStream(xmlStream.ToArray());
        }

        OsmXmlReader reader = new(xmlStream, new OsmXmlReaderSettings() { ReadMetadata = readMetadata });
        var read = reader.Read();

        switch (expected.EntityType)
        {
            case EntityType.Node:
                CompareNodes((Node)expected, (Node?)read);
                break;
            case EntityType.Way:
                CompareWays((Way)expected, (Way?)read);
                break;
            case EntityType.Relation:
                CompareRelations((Relation)expected, (Relation?)read);
                break;
        }
    }

    private new void CompareNodes(Node expected, Node? actual)
    {
        Assert.NotNull(actual);

        Assert.Equal(expected.Id, actual.Id);
        Assert.InRange(actual.Longitude, expected.Longitude - Resolution, expected.Longitude + Resolution);
        Assert.InRange(actual.Latitude, expected.Latitude - Resolution, expected.Latitude + Resolution);

        AssertTagsEqual(expected.Tags, actual.Tags);
        AssertMetadataEquals(expected.Metadata, actual.Metadata);
    }

    private void CheckNode(XElement element)
    {
        Assert.Equal("15", element.Attribute("id")?.Value);
        Assert.Equal("46.8", element.Attribute("lon")?.Value);
        Assert.Equal("-15.6", element.Attribute("lat")?.Value);

        var tagE = element.Elements("tag").Single();
        Assert.Equal("source", tagE.Attribute("k")?.Value);
        Assert.Equal("survey", tagE.Attribute("v")?.Value);
    }

    private void CheckWay(XElement element)
    {
        Assert.Equal("25", element.Attribute("id")?.Value);

        var nodesElement = element.Elements("nd");
        Assert.Equal(3, nodesElement.Count());
        Assert.Equal("11", nodesElement.ToList()[0].Attribute("ref")?.Value);
        Assert.Equal("12", nodesElement.ToList()[1].Attribute("ref")?.Value);
        Assert.Equal("13", nodesElement.ToList()[2].Attribute("ref")?.Value);

        var tagE = element.Elements("tag").Single();
        Assert.Equal("source", tagE.Attribute("k")?.Value);
        Assert.Equal("survey", tagE.Attribute("v")?.Value);
    }

    private void CheckRelation(XElement element)
    {
        Assert.Equal("25", element.Attribute("id")?.Value);

        var tagE = element.Elements("tag").Single();
        Assert.Equal("source", tagE.Attribute("k")?.Value);
        Assert.Equal("survey", tagE.Attribute("v")?.Value);
    }

    private void CheckOsmDetails(EntityMetadata details, XElement element)
    {
        if (details == null)
        {
            Assert.Null(element.Attribute("version"));
            Assert.Null(element.Attribute("changeset"));
            Assert.Null(element.Attribute("uid"));
            Assert.Null(element.Attribute("user"));
            Assert.Null(element.Attribute("visible"));
            Assert.Null(element.Attribute("timestamp"));
        }
        else
        {
            Assert.Equal("2", element.Attribute("version")?.Value);
            Assert.Equal("123", element.Attribute("changeset")?.Value);
            Assert.Equal("4587", element.Attribute("uid")?.Value);
            Assert.Equal("username", element.Attribute("user")?.Value);
            Assert.Equal("true", element.Attribute("visible")?.Value);
            Assert.Equal("2011-01-20T14:00:04Z", element.Attribute("timestamp")?.Value);
        }
    }
}
