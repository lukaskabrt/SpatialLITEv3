using SpatialLite.Osm;
using SpatialLite.Osm.IO;
using SpatialLITE.Osm.IO.Pbf;

namespace SpatialLITE.UnitTests.Osm.IO.Pbf;

public class PbfWriterTests
{
    //resolution for default granularity
    private const double Resolution = 1E-07;

    private readonly EntityMetadata _details;
    private readonly Node _node, _nodeTags, _nodeProperties;
    private readonly Way _way, _wayTags, _wayProperties, _wayWithoutNodes;
    private readonly Relation _relationNode, _relationWay, _relationRelation, _relationNodeProperties, _relationTags;

    public PbfWriterTests()
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
        _relationNodeProperties = new Relation { Id = 1, Tags = new TagsCollection(), Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" } }, Metadata = _details };
    }

    [Fact]
    public void Constructor_FilenameSettings_SetsSettingsAndMakesThemReadOnly()
    {
        string filename = Path.GetTempFileName();

        PbfWriterSettings settings = new();
        using (PbfWriter target = new(filename, settings))
        {
            Assert.Same(settings, target.Settings);
        }
    }

    [Fact]
    public void Constructor_FilenameSettings_CreatesOutputFile()
    {
        string filename = Path.GetTempFileName();

        PbfWriterSettings settings = new();
        using (PbfWriter target = new(filename, settings))
        {
            ;
        }

        Assert.True(File.Exists(filename));
    }

    [Fact]
    public void Constructor_FilenameSettings_WritesOsmHeader()
    {
        string filename = Path.GetTempFileName();

        PbfWriterSettings settings = new();
        using (PbfWriter target = new(filename, settings))
        {
            ;
        }

        FileInfo fi = new(filename);
        Assert.True(fi.Length > 0);
    }

    [Fact]
    public void Constructor_StreamSettings_SetsSettingsAndMakeThemReadOnly()
    {
        PbfWriterSettings settings = new();
        using (PbfWriter target = new(new MemoryStream(), settings))
        {
            Assert.Same(settings, target.Settings);
        }
    }

    [Fact]
    public void Constructor_StreamSettings_WritesOsmHeader()
    {
        MemoryStream stream = new();
        PbfWriterSettings settings = new();
        using (PbfWriter target = new(stream, settings))
        {
            ;
        }

        Assert.True(stream.ToArray().Length > 0);
    }

    [Fact]
    public void Dispose_ClosesOutputStreamIfWritingToFiles()
    {
        string filename = Path.GetTempFileName();

        PbfWriterSettings settings = new();
        PbfWriter target = new(filename, settings);
        target.Dispose();

        new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
    }

    [Fact]
    public void Write_ThrowsArgumentNullExceptionIfWriteMetadataIsTrueButEntityDoesNotHaveMetadata()
    {
        using (PbfWriter target = new(new MemoryStream(), new PbfWriterSettings() { UseDenseFormat = true, Compression = CompressionMode.None, WriteMetadata = true }))
        {
            Assert.Throws<ArgumentNullException>(() => target.Write(_node));
        }
    }

    [Fact]
    public void Write_ThrowsArgumentNullExceptionIfMetadataContainsNullInsteadOfUsername()
    {
        if (_nodeProperties.Metadata != null)
        {
            _nodeProperties.Metadata.User = null;
        }

        using (PbfWriter target = new(new MemoryStream(), new PbfWriterSettings() { UseDenseFormat = true, Compression = CompressionMode.None, WriteMetadata = true }))
        {
            Assert.Throws<ArgumentNullException>(() => target.Write(_nodeProperties));
        }
    }

    [Fact]
    public void Write_IEntityInfo_WritesNode()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_node);
        }

        TestPbfOutput(stream, _node);
    }

    [Fact]
    public void Write_IEntityInfo_WritesNodeWithTags()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_nodeTags);
        }

        TestPbfOutput(stream, _nodeTags);
    }

    [Fact]
    public void Write_IEntityInfo_WritesNodeWithMetadata()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = true };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_nodeProperties);
        }

        TestPbfOutput(stream, _nodeProperties);
    }

    [Fact]
    public void Write_IEntityInfo_DoesNotWriteNodeMetadataIfWriteMetadataSettingsIsFalse()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_nodeProperties);
        }

        TestPbfOutput(stream, _node);
    }

    [Fact]
    public void Write_IEntityInfo_WritesNode_Dense()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = true, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_node);
        }

        TestPbfOutput(stream, _node);
    }

    [Fact]
    public void Write_IEntityInfo_WritesNodeWithTags_Dense()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = true, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_nodeTags);
        }

        TestPbfOutput(stream, _nodeTags);
    }

    [Fact]
    public void Write_IEntityInfo_WritesNodeWithMetadata_Dense()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = true, Compression = CompressionMode.None, WriteMetadata = true };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_nodeProperties);
        }

        TestPbfOutput(stream, _nodeProperties);
    }

    [Fact]
    public void Write_IEntityInfo_DoesNotWriteNodeMetadataIfWriteMetadataSettingsIsFalse_Dense()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = true, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_nodeProperties);
        }

        TestPbfOutput(stream, _node);
    }

    [Fact]
    public void Write_IEntityInfo_WritesWay()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_way);
        }

        TestPbfOutput(stream, _way);
    }

    [Fact]
    public void Write_IEntityInfo_WritesWayWithTags()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_wayTags);
        }

        TestPbfOutput(stream, _wayTags);
    }

    [Fact]
    public void Write_IEntityInfo_WritesWayWithMetadata()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = true };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_wayProperties);
        }

        TestPbfOutput(stream, _wayProperties);
    }

    [Fact]
    public void Write_IEntityInfo_DoesNotWriteWayMetadataIfWriteMetadataSettingsIsFalse()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_wayProperties);
        }

        TestPbfOutput(stream, _way);
    }

    [Fact]
    public void Write_IEntityInfo_WritesRelationWithNode()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_relationNode);
        }

        TestPbfOutput(stream, _relationNode);
    }

    [Fact]
    public void Write_IEntityInfo_WritesRelationWithWay()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_relationWay);
        }

        TestPbfOutput(stream, _relationWay);
    }

    [Fact]
    public void Write_IEntityInfo_WritesRelationWithRelation()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_relationRelation);
        }

        TestPbfOutput(stream, _relationRelation);
    }

    [Fact]
    public void Write_IEntityInfo_WritesRelationWithTags()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_relationTags);
        }

        TestPbfOutput(stream, _relationTags);
    }

    [Fact]
    public void Write_IEntityInfo_WritesRelationWithMetadata()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = true };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_relationNodeProperties);
        }

        TestPbfOutput(stream, _relationNodeProperties);
    }

    [Fact]
    public void Write_IEntityInfo_DoesNotWriteRelationMetadataIfWriteMetadataSettingsIsFalse()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(_relationNodeProperties);
        }

        TestPbfOutput(stream, _relationNode);
    }

    [Fact]
    public void Write_IOsmEntity_WritesNode()
    {
        Node node = new() { Id = 1, Latitude = 11.1, Longitude = 12.1, Tags = new TagsCollection() };
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(node);
        }

        TestPbfOutput(stream, node);
    }

    [Fact]
    public void Write_IOsmEntity_WritesWay()
    {
        Way way = new() { Id = 10, Tags = new TagsCollection(), Nodes = new List<long> { 1, 2, 3 } };
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(way);
        }

        TestPbfOutput(stream, way);
    }

    [Fact]
    public void Write_IOsmEntity_WritesRelation()
    {
        Relation relation = new() { Id = 100, Tags = new TagsCollection(), Members = new List<RelationMember> { new RelationMember { MemberType = EntityType.Node, MemberId = 1, Role = "test-role" } } };

        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            target.Write(relation);
        }

        TestPbfOutput(stream, relation);
    }

    [Fact]
    public void Write_IOsmEntity_ThrowsExceptionIfEntityIsNull()
    {
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (PbfWriter target = new(stream, settings))
        {
            IOsmEntity? entity = null;
            Assert.Throws<ArgumentNullException>(() => target.Write(entity!));
        }
    }

    [Fact]
    public void Flush_ForcesWriterToWriteDataToUnderlyingStorage()
    {
        MemoryStream stream = new();

        PbfWriter target = new(stream, new PbfWriterSettings() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false });

        //1000 nodes should fit into tokens
        for (var i = 0; i < 1000; i++)
        {
            Node node = new() { Id = i, Latitude = 45.87, Longitude = -126.5, Tags = [] };
            target.Write(node);
        }

        var minimalExpectedLengthIncrease = 1000 * 8;

        var originalStreamLength = stream.Length;
        target.Flush();

        Assert.True(stream.Length > originalStreamLength + minimalExpectedLengthIncrease);
    }

    private void TestPbfOutput(MemoryStream pbfStream, IOsmEntity expected)
    {
        if (pbfStream.CanSeek)
        {
            pbfStream.Seek(0, SeekOrigin.Begin);
        }
        else
        {
            pbfStream = new MemoryStream(pbfStream.ToArray());
        }

        var reader = new PbfReader(pbfStream, new OsmReaderSettings() { ReadMetadata = true });
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
}
