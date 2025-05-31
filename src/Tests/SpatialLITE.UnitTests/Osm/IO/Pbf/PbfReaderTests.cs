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
        var settings = new OsmReaderSettings();

        using PbfReader target = new(dataStream, settings);

        Assert.Same(settings, target.Settings);
    }

    [Fact]
    public void Constructor_StringSettings_ThrowsExceptionIfFileDoesNotExist()
    {
        Assert.Throws<FileNotFoundException>(() =>
        {
            new PbfReader("non-existing-file.pbf", new() { ReadMetadata = false });
        });
    }

    [Fact]
    public void Constructor_StringSettings_ThrowsExceptionIfFileDoesNotContainOSMHeaderBeforeOSMData()
    {
        var filename = TestDataReader.OsmPbf.GetPath("pbf-without-osm-header.pbf");

        Assert.Throws<InvalidDataException>(() => new PbfReader(filename, new() { ReadMetadata = false }));
    }

    [Fact]
    public void Constructor_StringSettings_ThrowsExceptionIfOSMHeaderDefinedUnsupportedRequiredFeature()
    {
        var filename = TestDataReader.OsmPbf.GetPath("pbf-unsupported-required-feature.pbf");

        Assert.Throws<InvalidDataException>(() => new PbfReader(filename, new() { ReadMetadata = false }));
    }

    [Fact]
    public void Constructor_StringSettings_SetsSettings()
    {
        var filename = TestDataReader.OsmPbf.GetPath("pbf-n-node.pbf");
        var settings = new OsmReaderSettings();

        using PbfReader target = new(filename, settings);
        Assert.Same(settings, target.Settings);
    }

    [Fact]
    public void Read_ReturnsNullIfAllEntitiesHaveBeenRead()
    {
        var target = new PbfReader(TestDataReader.OsmPbf.Open("pbf-n-node.pbf"), new() { ReadMetadata = false });
        //read only entity
        var _ = target.Read();

        Assert.Null(target.Read());
    }

    [Fact]
    public void Read_ThrowInvalidDataExceptionIfHeaderBlockSizeExceedsAllowedValue()
    {
        Assert.Throws<InvalidDataException>(() => new PbfReader(TestDataReader.OsmPbf.Open("pbf-too-large-header-block.pbf"), new() { ReadMetadata = false }));
    }

    [Fact]
    public void Read_ThrowInvalidDataExceptionIfOsmDataBlockSizeExceedsAllowedValue()
    {
        Assert.Throws<InvalidDataException>(() => new PbfReader(TestDataReader.OsmPbf.Open("pbf-too-large-data-block.pbf"), new() { ReadMetadata = false }));
    }

    [Fact]
    public void Read_ReadsNode_DenseNoCompression()
    {
        var expected = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = [] };

        var node = ReadEntity<Node>("pbf-nd-node.pbf", false);

        AssertNodesEqual(expected, node);
    }

    [Fact]
    public void Read_ReadsNodeWithTags_DenseNoCompression()
    {
        var expected = new Node
        {
            Id = 1,
            Latitude = 50.4,
            Longitude = 16.2,
            Tags = [new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2")]
        };

        var node = ReadEntity<Node>("pbf-nd-node-tags.pbf", false);

        AssertNodesEqual(expected, node);
    }

    [Fact]
    public void Read_ReadsNodeWithMetadata_DenseNoCompression()
    {
        var expected = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = [], Metadata = _metadata };

        var node = ReadEntity<Node>("pbf-nd-node-all-properties.pbf", true);

        AssertNodesEqual(expected, node);
    }

    [Fact]
    public void Read_SkipsNodeMetadataIfProcessMetadataIsFalse_DenseNoCompression()
    {
        var node = ReadEntity<Node>("pbf-nd-node-all-properties.pbf", false);

        Assert.NotNull(node);
        Assert.Null(node.Metadata);
    }

    [Fact]
    public void Read_ReadsNode_NoCompression()
    {
        var expected = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = [] };

        var node = ReadEntity<Node>("pbf-n-node.pbf", false);

        AssertNodesEqual(expected, node);
    }

    [Fact]
    public void Read_ReadsNodeWithTags_NoCompression()
    {
        var expected = new Node
        {
            Id = 1,
            Latitude = 50.4,
            Longitude = 16.2,
            Tags = [new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2")]
        };

        var node = ReadEntity<Node>("pbf-n-node-tags.pbf", false);

        AssertNodesEqual(expected, node);
    }

    [Fact]
    public void Read_ReadsNodeWithMetadata_NoCompression()
    {
        var expected = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = [], Metadata = _metadata };

        var node = ReadEntity<Node>("pbf-n-node-all-properties.pbf", true);

        AssertNodesEqual(expected, node);
    }

    [Fact]
    public void Read_SkipsNodeMetadataIfProcessMetadataIsFalse_NoCompression()
    {
        var node = ReadEntity<Node>("pbf-n-node-all-properties.pbf", false);

        Assert.NotNull(node);
        Assert.Null(node.Metadata);
    }

    [Fact]
    public void Read_ReadsWay_NoCompression()
    {
        var expected = new Way { Id = 1, Tags = [], Nodes = [10, 11, 12] };

        var way = ReadEntity<Way>("pbf-n-way.pbf", false);

        AssertWaysEqual(expected, way);
    }

    [Fact]
    public void Read_ReadsWayWithTags_NoCompression()
    {
        var expected = new Way
        {
            Id = 1,
            Tags = [new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2")],
            Nodes = [10, 11, 12]
        };

        var way = ReadEntity<Way>("pbf-n-way-tags.pbf", false);

        AssertWaysEqual(expected, way);
    }

    [Fact]
    public void Read_ReadsWayWithMetadata_NoCompression()
    {
        var expected = new Way { Id = 1, Tags = [], Nodes = [10, 11, 12], Metadata = _metadata };

        var way = ReadEntity<Way>("pbf-n-way-all-properties.pbf", true);

        AssertWaysEqual(expected, way);
    }

    [Fact]
    public void Read_SkipsWayMetadataIfProcessMetadataIsFalse_NoCompression()
    {
        var way = ReadEntity<Way>("pbf-n-way-all-properties.pbf", false);

        Assert.NotNull(way);
        Assert.Null(way.Metadata);
    }

    [Fact]
    public void Read_ReadsWayWithoutNodes_NoCompression()
    {
        var expected = new Way { Id = 1, Tags = [], Nodes = [] };

        var way = ReadEntity<Way>("pbf-n-way-without-nodes.pbf", false);

        AssertWaysEqual(expected, way);
    }

    [Fact]
    public void Read_ReadsRelationWithNode_NoCompression()
    {
        var expected = new Relation { Id = 1, Tags = [], Members = [new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" }] };

        var relation = ReadEntity<Relation>("pbf-n-relation-node.pbf", false);

        AssertRelationsEqual(expected, relation);
    }

    [Fact]
    public void Read_ReadsRelationWithWay_NoCompression()
    {
        var expected = new Relation { Id = 1, Tags = [], Members = [new RelationMember { MemberType = EntityType.Way, MemberId = 10, Role = "test" }] };

        var relation = ReadEntity<Relation>("pbf-n-relation-way.pbf", false);

        AssertRelationsEqual(expected, relation);
    }

    [Fact]
    public void Read_ReadsRelationWithRelation_NoCompression()
    {
        var expected = new Relation { Id = 1, Tags = [], Members = [new RelationMember { MemberType = EntityType.Relation, MemberId = 10, Role = "test" }] };

        var relation = ReadEntity<Relation>("pbf-n-relation-relation.pbf", false);

        AssertRelationsEqual(expected, relation);
    }

    [Fact]
    public void Read_ReadsRelationWithTags_NoCompression()
    {
        var expected = new Relation
        {
            Id = 1,
            Tags = [new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2")],
            Members = [new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" }]
        };

        var relation = ReadEntity<Relation>("pbf-n-relation-tags.pbf", false);

        AssertRelationsEqual(expected, relation);
    }

    [Fact]
    public void Read_ReadsRelationWithAllProperties_NoCompression()
    {
        var expected = new Relation
        {
            Id = 1,
            Tags = [],
            Members = [new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" }],
            Metadata = _metadata
        };

        var relation = ReadEntity<Relation>("pbf-n-relation-all-properties.pbf", true);

        AssertRelationsEqual(expected, relation);
    }

    [Fact]
    public void Read_SkipsRelationMetadataIfProcessMetadataIsFalse_NoCompression()
    {
        var relation = ReadEntity<Relation>("pbf-n-relation-all-properties.pbf", false);

        Assert.NotNull(relation);
        Assert.Null(relation.Metadata);
    }

    private static T ReadEntity<T>(string filename, bool readMetadata) where T : IOsmEntity
    {
        using var stream = TestDataReader.OsmPbf.Open(filename);
        using var reader = new PbfReader(stream, new OsmReaderSettings { ReadMetadata = readMetadata });

        var entity = reader.Read();

        Assert.NotNull(entity);
        Assert.IsType<T>(entity);

        return (T)entity;
    }
}
