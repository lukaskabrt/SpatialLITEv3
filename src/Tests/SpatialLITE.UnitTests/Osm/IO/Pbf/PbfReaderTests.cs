using SpatialLite.Osm;
using SpatialLite.Osm.IO;
using SpatialLITE.Osm.IO.Pbf;
using SpatialLITE.UnitTests.Data;

namespace SpatialLITE.UnitTests.Osm.IO.Pbf;

public class PbfReaderTests
{
    //resolution for default granularity
    private const double Resolution = 1E-07;

    private readonly EntityMetadata _details;
    private readonly Node _node, _nodeTags, _nodeProperties;
    private readonly Way _way, _wayTags, _wayProperties, _wayWithoutNodes;
    private readonly Relation _relationNode, _relationWay, _relationRelation, _relationProperties, _relationTags;

    public PbfReaderTests()
    {
        _details = new EntityMetadata()
        {
            Timestamp = new DateTime(2010, 11, 19, 22, 5, 56, DateTimeKind.Utc),
            Uid = 127998,
            User = "Luk@s",
            Visible = true,
            Version = 2,
            Changeset = 6410629
        };

        _node = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = new TagsCollection() };
        _nodeTags = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = new TagsCollection(new[] { new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2") }) };
        _nodeProperties = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = new TagsCollection(), Metadata = _details };

        _way = new Way { Id = 1, Tags = new TagsCollection(), Nodes = new List<long> { 10, 11, 12 } };
        _wayTags = new Way { Id = 1, Tags = new TagsCollection(new[] { new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2") }), Nodes = new List<long> { 10, 11, 12 } };
        _wayProperties = new Way { Id = 1, Tags = new TagsCollection(), Nodes = new List<long> { 10, 11, 12 }, Metadata = _details };
        _wayWithoutNodes = new Way { Id = 1, Tags = new TagsCollection(), Nodes = new List<long>() };

        _relationNode = new Relation { Id = 1, Tags = new TagsCollection(), Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" } } };
        _relationWay = new Relation { Id = 1, Tags = new TagsCollection(), Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Way, MemberId = 10, Role = "test" } } };
        _relationRelation = new Relation { Id = 1, Tags = new TagsCollection(), Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Relation, MemberId = 10, Role = "test" } } };
        _relationTags = new Relation { Id = 1, Tags = new TagsCollection(new[] { new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2") }), Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" } } };
        _relationProperties = new Relation { Id = 1, Tags = new TagsCollection(), Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" } }, Metadata = _details };
    }

    [Fact]
    public void Constructor_StreamSettings_ThrowsExceptionIfStreamDoesNotContainOSMHeaderBeforeOSMData()
    {
        var dataStream = TestDataReader.OsmPbf.Open("pbf-without-osm-header.pbf");
        Assert.Throws<InvalidDataException>(() => new PbfReader(dataStream, new OsmReaderSettings() { ReadMetadata = false }));
    }

    [Fact]
    public void Constructor_StreamSettings_ThrowsExceptionIfOSMHeaderDefinedUnsupportedRequiredFeature()
    {
        var dataStream = TestDataReader.OsmPbf.Open("pbf-unsupported-required-feature.pbf");
        Assert.Throws<InvalidDataException>(() => new PbfReader(dataStream, new OsmReaderSettings() { ReadMetadata = false }));
    }

    [Fact]
    public void Constructor_StreamSettings_SetsSettings()
    {
        var dataStream = TestDataReader.OsmPbf.Open("pbf-n-node.pbf");
        OsmReaderSettings settings = new();

        using (PbfReader target = new(dataStream, settings))
        {
            Assert.Same(settings, target.Settings);
        }
    }

    [Fact]
    public void Constructor_StringSettings_ThrowsExceptionIfFileDoesNotExist()
    {
        Assert.Throws<FileNotFoundException>(() =>
        {
            new PbfReader("non-existing-file.pbf", new OsmReaderSettings() { ReadMetadata = false });
        });
    }

    [Fact]
    public void Constructor_StringSettings_ThrowsExceptionIfFileDoesNotContainOSMHeaderBeforeOSMData()
    {
        var filename = Path.Combine("..", "..", "..", "Data", "Pbf", "pbf-without-osm-header.pbf");
        Assert.Throws<InvalidDataException>(() => new PbfReader(filename, new OsmReaderSettings() { ReadMetadata = false }));
    }

    [Fact]
    public void Constructor_StringSettings_ThrowsExceptionIfOSMHeaderDefinedUnsupportedRequiredFeature()
    {
        var filename = Path.Combine("..", "..", "..", "Data", "Pbf", "pbf-unsupported-required-feature.pbf");
        Assert.Throws<InvalidDataException>(() => new PbfReader(filename, new OsmReaderSettings() { ReadMetadata = false }));
    }

    [Fact]
    public void Constructor_StringSettings_SetsSettings()
    {
        var filename = Path.Combine("..", "..", "..", "Data", "Pbf", "pbf-n-node.pbf");
        OsmReaderSettings settings = new();

        using (PbfReader target = new(filename, settings))
        {
            Assert.Same(settings, target.Settings);
        }
    }

    [Fact]
    public void Read_ReturnsNullIfAllEntitiesHaveBeenRead()
    {
        var target = new PbfReader(TestDataReader.OsmPbf.Open("pbf-n-node.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        //read only entity
        var _ = target.Read();
        // should return null
        Assert.Null(target.Read());
    }

    [Fact]
    public void Read_ThrowInvalidDataExceptionIfHeaderBlockSizeExceedsAllowedValue()
    {
        Assert.Throws<InvalidDataException>(() => new PbfReader(TestDataReader.OsmPbf.Open("pbf-too-large-header-block.pbf"), new OsmReaderSettings() { ReadMetadata = false }));
    }

    [Fact]
    public void Read_ThrowInvalidDataExceptionIfOsmDataBlockSizeExceedsAllowedValue()
    {
        Assert.Throws<InvalidDataException>(() => new PbfReader(TestDataReader.OsmPbf.Open("pbf-too-large-data-block.pbf"), new OsmReaderSettings() { ReadMetadata = false }));
    }

    [Fact]
    public void Read_ReadsNode_DenseNoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-nd-node.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var readNode = target.Read() as Node;

        CompareNodes(_node, readNode);
    }

    [Fact]
    public void Read_ReadsNodeWithTags_DenseNoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-nd-node-tags.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var readNode = target.Read() as Node;

        CompareNodes(_nodeTags, readNode);
    }

    [Fact]
    public void Read_ReadsNodeWithMetadata_DenseNoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-nd-node-all-properties.pbf"), new OsmReaderSettings() { ReadMetadata = true });
        var readNode = target.Read() as Node;

        CompareNodes(_nodeProperties, readNode);
    }

    [Fact]
    public void Read_SkipsNodeMetadataIfProcessMetadataIsFalse_DenseNoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-nd-node-all-properties.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var readNode = target.Read() as Node;

        if (readNode is not null)
        {
            Assert.Null(readNode.Metadata);
        }
    }

    [Fact]
    public void Read_ReadsNode_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-node.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var readNode = target.Read() as Node;

        CompareNodes(_node, readNode);
    }

    [Fact]
    public void Read_ReadsNodeWithTags_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-node-tags.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var readNode = target.Read() as Node;

        CompareNodes(_nodeTags, readNode);
    }

    [Fact]
    public void Read_ReadsNodeWithMetadata_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-node-all-properties.pbf"), new OsmReaderSettings() { ReadMetadata = true });
        var readNode = target.Read() as Node;

        CompareNodes(_nodeProperties, readNode);
    }

    [Fact]
    public void Read_SkipsNodeMetadataIfProcessMetadataIsFalse_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-node-all-properties.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var readNode = target.Read() as Node;

        if (readNode is not null)
        {
            Assert.Null(readNode.Metadata);
        }
    }

    [Fact]
    public void Read_ReadsWay_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-way.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var readWay = target.Read() as Way;

        CompareWays(_way, readWay);
    }

    [Fact]
    public void Read_ReadsWayWithTags_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-way-tags.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var readWay = target.Read() as Way;

        CompareWays(_wayTags, readWay);
    }

    [Fact]
    public void Read_ReadsWayWithMetadata_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-way-all-properties.pbf"), new OsmReaderSettings() { ReadMetadata = true });
        var readWay = target.Read() as Way;

        CompareWays(_wayProperties, readWay);
    }

    [Fact]
    public void Read_SkipsWayMetadataIfProcessMetadataIsFalse_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-way-all-properties.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var readWay = target.Read() as Way;

        if (readWay is not null)
        {
            Assert.Null(readWay.Metadata);
        }
    }

    [Fact]
    public void Read_ReadsWayWithoutNodes_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-way-without-nodes.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var readWay = target.Read() as Way;

        CompareWays(_wayWithoutNodes, readWay);
    }

    [Fact]
    public void Read_ReadsRelationWithNode_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-relation-node.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var readRelation = target.Read() as Relation;

        CompareRelation(_relationNode, readRelation);
    }

    [Fact]
    public void Read_ReadsRelationWithWay_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-relation-way.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var readRelation = target.Read() as Relation;

        CompareRelation(_relationWay, readRelation);
    }

    [Fact]
    public void Read_ReadsRelationWithRelation_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-relation-relation.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var readRelation = target.Read() as Relation;

        CompareRelation(_relationRelation, readRelation);
    }

    [Fact]
    public void Read_ReadsRelationWithTags_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-relation-tags.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var readRelation = target.Read() as Relation;

        CompareRelation(_relationTags, readRelation);
    }

    [Fact]
    public void Read_ReadsRelationWithAllProperties_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-relation-all-properties.pbf"), new OsmReaderSettings() { ReadMetadata = true });
        var readRelation = target.Read() as Relation;

        CompareRelation(_relationProperties, readRelation);
    }

    [Fact]
    public void Read_SkipsRelationMetadataIfProcessMetadataIsFalse_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-relation-all-properties.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var readRelation = target.Read() as Relation;

        if (readRelation is not null)
        {
            Assert.Null(readRelation.Metadata);
        }
    }

    [Fact]
    public void Dispose_ClosesOutputStreamIfWritingToFiles()
    {
        var filename = Path.Combine("..", "..", "..", "Data", "Pbf", "pbf-n-node.pbf");
        OsmReaderSettings settings = new() { ReadMetadata = true };

        PbfReader target = new(filename, settings);
        target.Dispose();

        using var testStream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
    }

    [Fact]
    public void Dispose_ClosesOutputStreamIfWritingToStream()
    {
        var stream = TestDataReader.OsmPbf.Open("pbf-n-node.pbf");
        OsmReaderSettings settings = new() { ReadMetadata = true };

        PbfReader target = new(stream, settings);
        target.Dispose();

        Assert.False(stream.CanRead);
    }

    private void CompareNodes(Node expected, Node? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.InRange(actual.Longitude, expected.Longitude - Resolution, expected.Longitude + Resolution);
        Assert.InRange(actual.Latitude, expected.Latitude - Resolution, expected.Latitude + Resolution);

        CompareTags(expected.Tags, actual.Tags);
        CompareEntityDetails(expected.Metadata, actual.Metadata);
    }

    private void CompareWays(Way expected, Way? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
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
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Members.Count, actual.Members.Count);
        for (var i = 0; i < expected.Members.Count; i++)
        {
            Assert.Equal(expected.Members[i].MemberId, actual.Members[i].MemberId);
            Assert.Equal(expected.Members[i].MemberType, actual.Members[i].MemberType);
            Assert.Equal(expected.Members[i].Role, actual.Members[i].Role);
        }

        CompareTags(expected.Tags, actual.Tags);
        CompareEntityDetails(expected.Metadata, actual.Metadata);
    }

    private void CompareTags(TagsCollection? expected, TagsCollection? actual)
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
}
