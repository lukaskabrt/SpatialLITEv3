﻿using SpatialLite.Osm;
using SpatialLite.Osm.IO;
using SpatialLite.Osm.IO.Pbf;

namespace SpatialLite.UnitTests.Osm.IO.Pbf;

public class PbfWriterTests : OsmIOTests
{
    private readonly EntityMetadata _metadata;

    public PbfWriterTests()
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
    public void Constructor_FilenameSettings_SetsSettings()
    {
        var filename = Path.GetTempFileName();

        try
        {
            var settings = new PbfWriterSettings();
            using PbfWriter target = new(filename, settings);

            Assert.Same(settings, target.Settings);
        }
        finally
        {
            File.Delete(filename);
        }
    }

    [Fact]
    public void Constructor_FilenameSettings_CreatesOutputFile()
    {
        var filename = Path.GetTempFileName();

        try
        {
            using var target = new PbfWriter(filename, new PbfWriterSettings());
            Assert.True(File.Exists(filename));
        }
        finally
        {
            File.Delete(filename);
        }
    }

    [Fact]
    public void Constructor_StreamSettings_SetsSettings()
    {
        var settings = new PbfWriterSettings();

        using var target = new PbfWriter(new MemoryStream(), settings);

        Assert.Same(settings, target.Settings);
    }

    [Fact]
    public void Write_ThrowsArgumentNullExceptionIfWriteMetadataIsTrueButEntityDoesNotHaveMetadata()
    {
        var node = new Node { Id = 1, Position = new(16.2, 50.4), Tags = [] };
        using var target = new PbfWriter(new MemoryStream(), new() { UseDenseFormat = true, Compression = CompressionMode.None, WriteMetadata = true });

        Assert.Throws<ArgumentNullException>(() => target.Write(node));
    }

    [Fact]
    public void Write_ThrowsArgumentNullExceptionIfMetadataContainsNullInsteadOfUsername()
    {
        var nodeProperties = new Node { Id = 1, Position = new(16.2, 50.4), Tags = [], Metadata = _metadata };
        if (nodeProperties.Metadata != null)
        {
            nodeProperties.Metadata.User = null;
        }

        using var target = new PbfWriter(new MemoryStream(), new() { UseDenseFormat = true, Compression = CompressionMode.None, WriteMetadata = true });
        Assert.Throws<ArgumentNullException>(() => target.Write(nodeProperties));
    }

    [Fact]
    public void Write_IOsmEntity_WritesNode()
    {
        var node = new Node { Id = 1, Position = new(16.2, 50.4), Tags = [] };

        MemoryStream stream = new();
        using (var target = new PbfWriter(stream, new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false }))
        {
            target.Write(node);
        }

        TestPbfOutput(stream, node);
    }

    [Fact]
    public void Write_IOsmEntity_WritesNodeWithTags()
    {
        var nodeTags = new Node { Id = 1, Position = new(16.2, 50.4), Tags = [new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2")] };

        MemoryStream stream = new();
        using (var target = new PbfWriter(stream, new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false }))
        {
            target.Write(nodeTags);
        }

        TestPbfOutput(stream, nodeTags);
    }

    [Fact]
    public void Write_IOsmEntity_WritesNodeWithMetadata()
    {
        var nodeProperties = new Node { Id = 1, Position = new(16.2, 50.4), Tags = [], Metadata = _metadata };

        MemoryStream stream = new();
        using (var target = new PbfWriter(stream, new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = true }))
        {
            target.Write(nodeProperties);
        }

        TestPbfOutput(stream, nodeProperties);
    }

    [Fact]
    public void Write_IOsmEntity_DoesNotWriteNodeMetadataIfWriteMetadataSettingsIsFalse()
    {
        var nodeWithProperties = new Node { Id = 1, Position = new(16.2, 50.4), Tags = [], Metadata = _metadata };
        var node = new Node { Id = 1, Position = new(16.2, 50.4), Tags = [] };

        MemoryStream stream = new();
        using (var target = new PbfWriter(stream, new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false }))
        {
            target.Write(nodeWithProperties);
        }

        TestPbfOutput(stream, node);
    }

    [Fact]
    public void Write_IOsmEntity_WritesNode_Dense()
    {
        var node = new Node { Id = 1, Position = new(16.2, 50.4), Tags = [] };

        MemoryStream stream = new();
        using (var target = new PbfWriter(stream, new() { UseDenseFormat = true, Compression = CompressionMode.None, WriteMetadata = false }))
        {
            target.Write(node);
        }

        TestPbfOutput(stream, node);
    }

    [Fact]
    public void Write_IOsmEntity_WritesNodeWithTags_Dense()
    {
        var nodeTags = new Node
        {
            Id = 1,
            Position = new(16.2, 50.4),
            Tags = [new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2")]
        };

        MemoryStream stream = new();
        using (var target = new PbfWriter(stream, new() { UseDenseFormat = true, Compression = CompressionMode.None, WriteMetadata = false }))
        {
            target.Write(nodeTags);
        }

        TestPbfOutput(stream, nodeTags);
    }

    [Fact]
    public void Write_IOsmEntity_WritesNodeWithMetadata_Dense()
    {
        var nodeProperties = new Node { Id = 1, Position = new(16.2, 50.4), Tags = [], Metadata = _metadata };

        MemoryStream stream = new();
        using (PbfWriter target = new(stream, new() { UseDenseFormat = true, Compression = CompressionMode.None, WriteMetadata = true }))
        {
            target.Write(nodeProperties);
        }

        TestPbfOutput(stream, nodeProperties);
    }

    [Fact]
    public void Write_IOsmEntity_DoesNotWriteNodeMetadataIfWriteMetadataSettingsIsFalse_Dense()
    {
        var nodeProperties = new Node { Id = 1, Position = new(16.2, 50.4), Tags = [], Metadata = _metadata };
        var node = new Node { Id = 1, Position = new(16.2, 50.4), Tags = [] };

        MemoryStream stream = new();
        using (PbfWriter target = new(stream, new() { UseDenseFormat = true, Compression = CompressionMode.None, WriteMetadata = false }))
        {
            target.Write(nodeProperties);
        }

        TestPbfOutput(stream, node);
    }

    [Fact]
    public void Write_IOsmEntity_WritesWay()
    {
        var way = new Way { Id = 1, Tags = [], Nodes = [10, 11, 12] };
        PbfWriterSettings settings = new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false };
        MemoryStream stream = new();

        using (var target = new PbfWriter(stream, settings))
        {
            target.Write(way);
        }

        TestPbfOutput(stream, way);
    }

    [Fact]
    public void Write_IOsmEntity_WritesWayWithTags()
    {
        var wayTags = new Way { Id = 1, Tags = [new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2")], Nodes = [10, 11, 12] };

        MemoryStream stream = new();
        using (var target = new PbfWriter(stream, new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false }))
        {
            target.Write(wayTags);
        }

        TestPbfOutput(stream, wayTags);
    }

    [Fact]
    public void Write_IOsmEntity_WritesWayWithMetadata()
    {
        var wayProperties = new Way { Id = 1, Tags = [], Nodes = [10, 11, 12], Metadata = _metadata };

        MemoryStream stream = new();
        using (var target = new PbfWriter(stream, new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = true }))
        {
            target.Write(wayProperties);
        }

        TestPbfOutput(stream, wayProperties);
    }

    [Fact]
    public void Write_IOsmEntity_DoesNotWriteWayMetadataIfWriteMetadataSettingsIsFalse()
    {
        var wayProperties = new Way { Id = 1, Tags = [], Nodes = [10, 11, 12], Metadata = _metadata };
        var way = new Way { Id = 1, Tags = [], Nodes = [10, 11, 12] };

        MemoryStream stream = new();
        using (var target = new PbfWriter(stream, new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false }))
        {
            target.Write(wayProperties);
        }

        TestPbfOutput(stream, way);
    }

    [Fact]
    public void Write_IOsmEntity_WritesRelationWithNode()
    {
        var relationNode = new Relation { Id = 1, Tags = [], Members = [new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" }] };

        MemoryStream stream = new();
        using (var target = new PbfWriter(stream, new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false }))
        {
            target.Write(relationNode);
        }

        TestPbfOutput(stream, relationNode);
    }

    [Fact]
    public void Write_IOsmEntity_WritesRelationWithWay()
    {
        var relationWay = new Relation { Id = 1, Tags = [], Members = [new RelationMember { MemberType = EntityType.Way, MemberId = 10, Role = "test" }] };

        MemoryStream stream = new();
        using (var target = new PbfWriter(stream, new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false }))
        {
            target.Write(relationWay);
        }

        TestPbfOutput(stream, relationWay);
    }

    [Fact]
    public void Write_IOsmEntity_WritesRelationWithRelation()
    {
        var relationRelation = new Relation { Id = 1, Tags = [], Members = [new RelationMember { MemberType = EntityType.Relation, MemberId = 10, Role = "test" }] };

        MemoryStream stream = new();
        using (var target = new PbfWriter(stream, new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false }))
        {
            target.Write(relationRelation);
        }

        TestPbfOutput(stream, relationRelation);
    }

    [Fact]
    public void Write_IOsmEntity_WritesRelationWithTags()
    {
        var relationTags = new Relation { Id = 1, Tags = [new KeyValuePair<string, string>("name", "test"), new KeyValuePair<string, string>("name-2", "test-2")], Members = [new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" }] };

        MemoryStream stream = new();
        using (var target = new PbfWriter(stream, new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false }))
        {
            target.Write(relationTags);
        }

        TestPbfOutput(stream, relationTags);
    }

    [Fact]
    public void Write_IOsmEntity_WritesRelationWithMetadata()
    {
        var relationNodeProperties = new Relation { Id = 1, Tags = [], Members = [new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" }], Metadata = _metadata };

        MemoryStream stream = new();
        using (var target = new PbfWriter(stream, new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = true }))
        {
            target.Write(relationNodeProperties);
        }

        TestPbfOutput(stream, relationNodeProperties);
    }

    [Fact]
    public void Write_IOsmEntity_DoesNotWriteRelationMetadataIfWriteMetadataSettingsIsFalse()
    {
        var relationNodeProperties = new Relation { Id = 1, Tags = [], Members = [new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" }], Metadata = _metadata };
        var relationNode = new Relation { Id = 1, Tags = [], Members = [new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" }] };

        MemoryStream stream = new();
        using (var target = new PbfWriter(stream, new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false }))
        {
            target.Write(relationNodeProperties);
        }

        TestPbfOutput(stream, relationNode);
    }

    [Fact]
    public void Write_IOsmEntity_WritesRelation()
    {
        Relation relation = new() { Id = 100, Tags = [], Members = [new RelationMember { MemberType = EntityType.Node, MemberId = 1, Role = "test-role" }] };

        MemoryStream stream = new();
        using (var target = new PbfWriter(stream, new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false }))
        {
            target.Write(relation);
        }

        TestPbfOutput(stream, relation);
    }

    [Fact]
    public void Write_IOsmEntity_ThrowsExceptionIfEntityIsNull()
    {
        MemoryStream stream = new();
        using var target = new PbfWriter(stream, new() { UseDenseFormat = false, Compression = CompressionMode.None, WriteMetadata = false });

        IOsmEntity? entity = null;
        Assert.Throws<ArgumentNullException>(() => target.Write(entity!));
    }

    [Fact]
    public void Flush_ForcesWriterToWriteDataToUnderlyingStorage()
    {
        MemoryStream stream = new();

        var target = new PbfWriter(stream, new()
        {
            UseDenseFormat = false,
            Compression = CompressionMode.None,
            WriteMetadata = false
        });

        //1000 nodes should fit into tokens
        for (var i = 0; i < 1000; i++)
        {
            Node node = new() { Id = i, Position = new(-126.5, 45.87), Tags = [] };
            target.Write(node);
        }

        var minimalExpectedLengthIncrease = 1000 * 8;

        var originalStreamLength = stream.Length;
        target.Flush();

        Assert.True(stream.Length > originalStreamLength + minimalExpectedLengthIncrease);
    }

    private static void TestPbfOutput(MemoryStream pbfStream, IOsmEntity expected)
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
                AssertNodesEqual((Node)expected, (Node?)read);
                break;
            case EntityType.Way:
                AssertWaysEqual((Way)expected, (Way?)read);
                break;
            case EntityType.Relation:
                AssertRelationsEqual((Relation)expected, (Relation?)read);
                break;
        }
    }
}
