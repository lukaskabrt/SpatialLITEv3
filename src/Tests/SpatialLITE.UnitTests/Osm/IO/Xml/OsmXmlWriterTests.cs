using SpatialLite.Osm;
using SpatialLite.Osm.IO;
using SpatialLITE.Osm.IO.Xml;
using System.Xml.Linq;

namespace SpatialLITE.UnitTests.Osm.IO.Xml;

public class OsmXmlWriterTests
{
    //resolution for default granularity
    private const double _resolution = 1E-07;

    private readonly EntityMetadata _metadata;
    private readonly NodeInfo _node, _nodeTags, _nodeProperties;
    private readonly WayInfo _way, _wayTags, _wayProperties, _wayWithoutNodes;
    private readonly RelationInfo _relationNode, _relationWay, _relationRelation, _relationNodeProperties, _relationTags;

    public OsmXmlWriterTests()
    {
        _metadata = new EntityMetadata()
        {
            Timestamp = new DateTime(2010, 11, 19, 22, 5, 56, DateTimeKind.Utc),
            Uid = 127998,
            User = "Luk@s",
            Visible = true,
            Version = 2,
            Changeset = 6410629
        };

        _node = new NodeInfo(1, 50.4, 16.2, new TagsCollection());
        _nodeTags = new NodeInfo(1, 50.4, 16.2, new TagsCollection(new Tag[] { new("name", "test"), new("name-2", "test-2") }));
        _nodeProperties = new NodeInfo(1, 50.4, 16.2, new TagsCollection(), _metadata);

        _way = new WayInfo(1, new TagsCollection(), new long[] { 10, 11, 12 });
        _wayTags = new WayInfo(1, new TagsCollection(new Tag[] { new("name", "test"), new("name-2", "test-2") }), new long[] { 10, 11, 12 });
        _wayProperties = new WayInfo(1, new TagsCollection(), new long[] { 10, 11, 12 }, _metadata);
        _wayWithoutNodes = new WayInfo(1, new TagsCollection(), new long[] { });

        _relationNode = new RelationInfo(1, new TagsCollection(), new RelationMemberInfo[] { new() { MemberType = EntityType.Node, Reference = 10, Role = "test" } });
        _relationWay = new RelationInfo(1, new TagsCollection(), new RelationMemberInfo[] { new() { MemberType = EntityType.Way, Reference = 10, Role = "test" } });
        _relationRelation = new RelationInfo(1, new TagsCollection(), new RelationMemberInfo[] { new() { MemberType = EntityType.Relation, Reference = 10, Role = "test" } });
        _relationTags = new RelationInfo(
            1,
            new TagsCollection(new Tag[] { new("name", "test"), new("name-2", "test-2") }),
            new RelationMemberInfo[] { new() { MemberType = EntityType.Node, Reference = 10, Role = "test" } });
        _relationNodeProperties = new RelationInfo(1, new TagsCollection(), new RelationMemberInfo[] { new() { MemberType = EntityType.Node, Reference = 10, Role = "test" } }, _metadata);
    }

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
        string path = Path.GetTempPath();

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
        var node = new Node { ID = 1, Latitude = 50.4, Longitude = 16.2, Tags = [] };

        MemoryStream stream = new();

        using OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = true });

        Assert.Throws<ArgumentException>(() => target.Write(node));
    }

    [Fact]
    public void Write_DoesNotThrowsExceptionIfMetadataContainsNullInsteadUsername()
    {
        var node = new Node { ID = 1, Latitude = 50.4, Longitude = 16.2, Tags = [], Metadata = _metadata };
        node.Metadata.User = null;

        MemoryStream stream = new();
        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = true }))
        {
            target.Write(_nodeProperties);
        }
    }

    [Fact]
    public void Write_IEntityInfo_WritesNode()
    {
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_node);
        }

        TestXmlOutput(stream, _node, false);
    }

    [Fact]
    public void Write_IEntityInfo_WritesNodeWithTags()
    {
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_nodeTags);
        }

        TestXmlOutput(stream, _nodeTags, false);
    }

    [Fact]
    public void Write_IEntityInfo_WritesNodeWithMetadata()
    {
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = true }))
        {
            target.Write(_nodeProperties);
        }

        TestXmlOutput(stream, _nodeProperties, true);
    }

    [Fact]
    public void Write_IEntityInfo_DoesntWriteNodeMetadataIfWriteMedataIsFalse()
    {
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_nodeProperties);
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
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_way);
        }

        TestXmlOutput(stream, _way, false);
    }

    [Fact]
    public void Write_IEntityInfo_WritesWayWithTags()
    {
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_wayTags);
        }

        TestXmlOutput(stream, _wayTags, false);
    }

    [Fact]
    public void Write_IEntityInfo_WritesWayWithMetadata()
    {
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = true }))
        {
            target.Write(_wayProperties);
        }

        TestXmlOutput(stream, _wayProperties, true);
    }

    [Fact]
    public void Write_IEntityInfo_DoesntWriteWayMetadataIfWriteMedataIsFalse()
    {
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_wayProperties);
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
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_relationNode);
        }

        TestXmlOutput(stream, _relationNode, false);
    }

    [Fact]
    public void Write_IEntityInfo_WritesRelationWithWay()
    {
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_relationWay);
        }

        TestXmlOutput(stream, _relationWay, false);
    }

    [Fact]
    public void Write_IEntityInfo_WritesRelationWithRelation()
    {
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_relationRelation);
        }

        TestXmlOutput(stream, _relationRelation, false);
    }

    [Fact]
    public void Write_IEntityInfo_WritesRelationWithTags()
    {
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_relationTags);
        }

        TestXmlOutput(stream, _relationTags, false);
    }

    [Fact]
    public void Write_IEntityInfo_WritesRelationWithMetadata()
    {
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = true }))
        {
            target.Write(_relationNodeProperties);
        }

        TestXmlOutput(stream, _relationNodeProperties, true);
    }

    [Fact]
    public void Write_IEntityInfo_DoesntWriteRelationMetadataIfWriteMedataIsFalse()
    {
        MemoryStream stream = new();

        using (OsmXmlWriter target = new(stream, new OsmWriterSettings() { WriteMetadata = false }))
        {
            target.Write(_relationNodeProperties);
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
                CompareRelation((Relation)expected, (Relation?)read);
                break;
        }
    }

    private void CompareNodes(Node expected, Node? actual)
    {
        Assert.NotNull(actual);

        Assert.Equal(expected.ID, actual.ID);
        Assert.InRange(actual.Longitude, expected.Longitude - _resolution, expected.Longitude + _resolution);
        Assert.InRange(actual.Latitude, expected.Latitude - _resolution, expected.Latitude + _resolution);

        CompareTags(expected.Tags, actual.Tags);
        CompareEntityDetails(expected.Metadata, actual.Metadata);
    }

    private void CompareWays(Way expected, Way? actual)
    {
        Assert.NotNull(actual);

        Assert.Equal(expected.ID, actual.ID);
        Assert.Equal(expected.Nodes.Count, actual.Nodes.Count);
        for (var i = 0; i < expected.Nodes.Count; i++)
        {
            Assert.Equal(expected.Nodes[i], actual.Nodes[i]);
        }

        CompareTags(expected.Tags, actual.Tags);
        CompareEntityDetails(expected.Metadata, actual.Metadata);
    }

    private void CompareRelation(Relation expected, Relation? actual)
    {
        Assert.NotNull(actual);

        Assert.Equal(expected.ID, actual.ID);
        Assert.Equal(expected.Members.Count, actual.Members.Count);
        for (var i = 0; i < expected.Members.Count; i++)
        {
            Assert.Equal(expected.Members[i], actual.Members[i]);
        }

        CompareTags(expected.Tags, actual.Tags);
        CompareEntityDetails(expected.Metadata, actual.Metadata);
    }

    private void CompareTags(TagsCollection expected, TagsCollection actual)
    {
        if (expected == null && actual == null)
        {
            return;
        }

        Assert.NotNull(expected);
        Assert.NotNull(actual);

        Assert.Equal(expected.Count, actual.Count);
        Assert.True(expected.All(actual.Contains));
    }

    private void CompareEntityDetails(EntityMetadata? expected, EntityMetadata? actual)
    {
        if (expected == null && actual == null)
        {
            return;
        }

        Assert.NotNull(expected);
        Assert.NotNull(actual);

        Assert.Equal(expected.Timestamp, actual.Timestamp);
        Assert.Equal(expected.Uid, actual.Uid);
        Assert.Equal(expected.User, actual.User);
        Assert.Equal(expected.Visible, actual.Visible);
        Assert.Equal(expected.Version, actual.Version);
        Assert.Equal(expected.Changeset, actual.Changeset);
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
