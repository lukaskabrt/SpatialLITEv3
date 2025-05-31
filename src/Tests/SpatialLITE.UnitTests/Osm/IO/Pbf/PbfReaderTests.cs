using SpatialLite.Osm;
using SpatialLite.Osm.IO;
using SpatialLITE.Osm.IO.Pbf;
using SpatialLITE.UnitTests.Data;

namespace SpatialLITE.UnitTests.Osm.IO.Pbf;

public class PbfReaderTests : OsmIOTests
{
    private readonly EntityMetadata _metadata;

    public PbfReaderTests()
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
        var filename = TestDataReader.OsmPbf.GetPath("pbf-without-osm-header.pbf");
        Assert.Throws<InvalidDataException>(() => new PbfReader(filename, new OsmReaderSettings() { ReadMetadata = false }));
    }

    [Fact]
    public void Constructor_StringSettings_ThrowsExceptionIfOSMHeaderDefinedUnsupportedRequiredFeature()
    {
        var filename = TestDataReader.OsmPbf.GetPath("pbf-unsupported-required-feature.pbf");
        Assert.Throws<InvalidDataException>(() => new PbfReader(filename, new OsmReaderSettings() { ReadMetadata = false }));
    }

    [Fact]
    public void Constructor_StringSettings_SetsSettings()
    {
        var filename = TestDataReader.OsmPbf.GetPath("pbf-n-node.pbf");
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
        var _node = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = new TagsCollection() };
        var readNode = target.Read() as Node;

        AssertNodesEqual(_node, readNode);
    }

    [Fact]
    public void Read_ReadsNodeWithTags_DenseNoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-nd-node-tags.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var _nodeTags = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = new TagsCollection(new[] { new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2") }) };
        var readNode = target.Read() as Node;

        AssertNodesEqual(_nodeTags, readNode);
    }

    [Fact]
    public void Read_ReadsNodeWithMetadata_DenseNoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-nd-node-all-properties.pbf"), new OsmReaderSettings() { ReadMetadata = true });
        var _nodeProperties = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = new TagsCollection(), Metadata = _metadata };
        var readNode = target.Read() as Node;

        AssertNodesEqual(_nodeProperties, readNode);
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
        var _node = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = new TagsCollection() };
        var readNode = target.Read() as Node;

        AssertNodesEqual(_node, readNode);
    }

    [Fact]
    public void Read_ReadsNodeWithTags_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-node-tags.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var _nodeTags = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = new TagsCollection(new[] { new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2") }) };
        var readNode = target.Read() as Node;

        AssertNodesEqual(_nodeTags, readNode);
    }

    [Fact]
    public void Read_ReadsNodeWithMetadata_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-node-all-properties.pbf"), new OsmReaderSettings() { ReadMetadata = true });
        var _nodeProperties = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = new TagsCollection(), Metadata = _metadata };
        var readNode = target.Read() as Node;

        AssertNodesEqual(_nodeProperties, readNode);
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
        var _way = new Way { Id = 1, Tags = new TagsCollection(), Nodes = new List<long> { 10, 11, 12 } };
        var readWay = target.Read() as Way;

        AssertWaysEqual(_way, readWay);
    }

    [Fact]
    public void Read_ReadsWayWithTags_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-way-tags.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var _wayTags = new Way { Id = 1, Tags = new TagsCollection(new[] { new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2") }), Nodes = new List<long> { 10, 11, 12 } };
        var readWay = target.Read() as Way;

        AssertWaysEqual(_wayTags, readWay);
    }

    [Fact]
    public void Read_ReadsWayWithMetadata_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-way-all-properties.pbf"), new OsmReaderSettings() { ReadMetadata = true });
        var _wayProperties = new Way { Id = 1, Tags = new TagsCollection(), Nodes = new List<long> { 10, 11, 12 }, Metadata = _metadata };
        var readWay = target.Read() as Way;

        AssertWaysEqual(_wayProperties, readWay);
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
        var _wayWithoutNodes = new Way { Id = 1, Tags = new TagsCollection(), Nodes = new List<long>() };
        var readWay = target.Read() as Way;

        AssertWaysEqual(_wayWithoutNodes, readWay);
    }

    [Fact]
    public void Read_ReadsRelationWithNode_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-relation-node.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var _relationNode = new Relation { Id = 1, Tags = new TagsCollection(), Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" } } };
        var readRelation = target.Read() as Relation;

        AssertRelationsEqual(_relationNode, readRelation);
    }

    [Fact]
    public void Read_ReadsRelationWithWay_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-relation-way.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var _relationWay = new Relation { Id = 1, Tags = new TagsCollection(), Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Way, MemberId = 10, Role = "test" } } };
        var readRelation = target.Read() as Relation;

        AssertRelationsEqual(_relationWay, readRelation);
    }

    [Fact]
    public void Read_ReadsRelationWithRelation_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-relation-relation.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var _relationRelation = new Relation { Id = 1, Tags = new TagsCollection(), Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Relation, MemberId = 10, Role = "test" } } };
        var readRelation = target.Read() as Relation;

        AssertRelationsEqual(_relationRelation, readRelation);
    }

    [Fact]
    public void Read_ReadsRelationWithTags_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-relation-tags.pbf"), new OsmReaderSettings() { ReadMetadata = false });
        var _relationTags = new Relation { Id = 1, Tags = new TagsCollection(new[] { new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2") }), Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" } } };
        var readRelation = target.Read() as Relation;

        AssertRelationsEqual(_relationTags, readRelation);
    }

    [Fact]
    public void Read_ReadsRelationWithAllProperties_NoCompression()
    {
        PbfReader target = new(TestDataReader.OsmPbf.Open("pbf-n-relation-all-properties.pbf"), new OsmReaderSettings() { ReadMetadata = true });
        var _relationProperties = new Relation { Id = 1, Tags = new TagsCollection(), Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" } }, Metadata = _metadata };
        var readRelation = target.Read() as Relation;

        AssertRelationsEqual(_relationProperties, readRelation);
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
}
