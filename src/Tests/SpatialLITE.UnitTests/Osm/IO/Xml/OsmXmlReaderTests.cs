using SpatialLite.Osm;
using SpatialLITE.Osm.IO.Xml;
using SpatialLITE.UnitTests.Data;
using System.Xml;

namespace SpatialLITE.UnitTests.Osm.IO.Xml;

public class OsmXmlReaderTests : OsmIOTests
{
    private readonly EntityMetadata _metadata = new()
    {
        Timestamp = new DateTime(2010, 11, 19, 22, 5, 56, DateTimeKind.Utc),
        Uid = 127998,
        User = "Luk@s",
        Visible = true,
        Version = 2,
        Changeset = 6410629
    };

    [Fact]
    public void Constructor_StringSettings_ThrowsExceptionIfFileDoesNotExist()
    {
        Assert.Throws<FileNotFoundException>(delegate
        { new OsmXmlReader("non-existing-file.osm", new OsmXmlReaderSettings() { ReadMetadata = false }); });
    }

    [Fact]
    public void Constructor_StreamSettings_SetsSettings()
    {
        OsmXmlReaderSettings settings = new() { ReadMetadata = false };
        using (OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-simple-node.osm"), settings))
        {
            Assert.Same(settings, target.Settings);
        }
    }

    [Fact]
    public void Read_SkipsUnknownElements()
    {
        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-unknown-inner-element.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });
        var result = target.Read();

        Assert.NotNull(result as Node);
    }

    //Tested only on Nodes - code for parsing Tags is shared among functions parsing Node, Way and Relation
    [Fact]
    public void Read_ThrowsExceptionIfTagHasNotKey()
    {
        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-node-tag-without-key.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });

        Assert.Throws<XmlException>(target.Read);
    }

    //Tested only on Nodes - code for parsing Tags is shared among functions parsing Node, Way and Relation
    [Fact]
    public void Read_ThrowsExceptionIfTagHasNotValue()
    {
        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-node-tag-without-value.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });

        Assert.Throws<XmlException>(target.Read);
    }

    [Fact]
    public void Read_ThrowsExceptionIfPieceOfMetadataIsMissingAndStrictModeIsTrue()
    {
        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-node-missing-timestamp.osm"), new OsmXmlReaderSettings() { ReadMetadata = true, StrictMode = true });
        Assert.Throws<XmlException>(target.Read);
    }

    [Fact]
    public void Read_DoesNotThrowExceptionIfPieceOfMetadataIsMissingAndStrictModeIsFalse()
    {
        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-node-missing-timestamp.osm"), new OsmXmlReaderSettings() { ReadMetadata = true, StrictMode = false });
        target.Read();
    }

    [Fact]
    public void Read_ThrowsExceptionIfNodeHasNotID()
    {
        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-node-without-id.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });

        Assert.Throws<XmlException>(target.Read);
    }

    [Fact]
    public void Read_ThrowsExceptionIfNodeHasNotLat()
    {
        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-node-without-lat.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });

        Assert.Throws<XmlException>(target.Read);
    }

    [Fact]
    public void Read_ThrowsExceptionIfNodeHasNotLon()
    {
        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-node-without-lon.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });

        Assert.Throws<XmlException>(target.Read);
    }

    [Fact]
    public void Read_ReadsSimpleNode()
    {
        var expected = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = [] };

        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-simple-node.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });
        var readNode = target.Read() as Node;

        CompareNodes(expected, readNode);
    }

    [Fact]
    public void Read_ReadsNodeWithUnknownElement()
    {
        var expected = new Node { Id = 1, Latitude = 50.4, Longitude = 16.2, Tags = [] };

        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-node-with-tag-and-unknown-element.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });
        var readNode = target.Read() as Node;

        CompareNodes(expected, readNode);

        // nothing more left to read in the file
        Assert.Null(target.Read());
    }

    [Fact]
    public void Read_ReadsNodeWithTags()
    {
        var expected = new Node
        {
            Id = 2,
            Latitude = 50.4,
            Longitude = 16.2,
            Tags = [new("name", "test"), new("name-2", "test-2")]
        };

        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-node-with-tags.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });
        var readNode = target.Read() as Node;

        CompareNodes(expected, readNode);
    }

    [Fact]
    public void Read_ReadsNodeWithAllAttributes()
    {
        var expected = new Node
        {
            Id = 3,
            Latitude = 50.4,
            Longitude = 16.2,
            Tags = [],
            Metadata = _metadata
        };

        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-node-all-properties.osm"), new OsmXmlReaderSettings() { ReadMetadata = true });
        var readNode = target.Read() as Node;

        CompareNodes(expected, readNode);
    }

    [Fact]
    public void Read_ThrowsExceptionIfWayHasNotID()
    {
        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-way-nd-without-ref.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });

        Assert.Throws<XmlException>(target.Read);
    }

    [Fact]
    public void Read_ThrowsExceptionIfWayNDHasNotRef()
    {
        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-way-nd-without-ref.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });

        Assert.Throws<XmlException>(target.Read);
    }

    [Fact]
    public void Read_ReadsWayWithoutNodes()
    {
        var expected = new Way { Id = 1, Tags = [], Nodes = [] };

        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-way-without-nodes.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });
        var readWay = target.Read() as Way;

        CompareWays(expected, readWay);
    }

    [Fact]
    public void Read_ReadsSimpleWay()
    {
        var expected = new Way { Id = 1, Tags = [], Nodes = [10, 11, 12] };

        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-simple-way.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });
        var readWay = target.Read() as Way;

        CompareWays(expected, readWay);
    }

    [Fact]
    public void Read_ReadsWayWithTags()
    {
        var expected = new Way { Id = 2, Tags = [new("name", "test"), new("name-2", "test-2")], Nodes = [10, 11, 12] };

        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-way-with-tags.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });
        var readWay = target.Read() as Way;

        CompareWays(expected, readWay);
    }

    [Fact]
    public void Read_ReadsWayWithUnknownElement()
    {
        var expected = new Way { Id = 2, Tags = [new("name", "test"), new("name-2", "test-2")], Nodes = [10, 11, 12] };

        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-way-with-tags-and-unknown-element.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });
        var readWay = target.Read() as Way;

        CompareWays(expected, readWay);
    }

    [Fact]
    public void Read_ReadsWayWithAllAttributes()
    {
        var expected = new Way { Id = 1, Tags = [], Nodes = [10, 11, 12], Metadata = _metadata };

        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-way-all-properties.osm"), new OsmXmlReaderSettings() { ReadMetadata = true });
        var readWay = target.Read() as Way;

        CompareWays(expected, readWay);
    }

    [Fact]
    public void Read_ThrowsExceptionIfRelationHasNotID()
    {
        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-relation-without-id.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });

        Assert.Throws<XmlException>(target.Read);
    }

    [Fact]
    public void Read_ThrowsExceptionIfRelationMemberHasNotRef()
    {
        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-relation-member-without-ref.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });

        Assert.Throws<XmlException>(target.Read);
    }

    [Fact]
    public void Read_ThrowsExceptionIfRelationMemberHasNotType()
    {
        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-relation-member-without-type.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });

        Assert.Throws<XmlException>(target.Read);
    }

    [Fact]
    public void Read_ReadsRelationWithoutMembers()
    {
        var expected = new Relation { Id = 1, Tags = [], Members = [] };

        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-relation-without-members.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });
        var readRelation = target.Read() as Relation;

        CompareRelations(expected, readRelation);
    }

    [Fact]
    public void Read_ReadsRelationWithNodeMember()
    {
        var expected = new Relation { Id = 1, Tags = [], Members = [new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" }] };

        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-relation-node.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });
        var readRelation = target.Read() as Relation;

        CompareRelations(expected, readRelation);
    }

    [Fact]
    public void Read_ReadsRelationWithWayMember()
    {
        var expected = new Relation { Id = 1, Tags = [], Members = [new RelationMember { MemberType = EntityType.Way, MemberId = 10, Role = "test" }] };

        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-relation-way.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });
        var readRelation = target.Read() as Relation;

        CompareRelations(expected, readRelation);
    }

    [Fact]
    public void Read_ReadsRelationWithRelationMember()
    {
        var expected = new Relation { Id = 1, Tags = [], Members = [new RelationMember { MemberType = EntityType.Relation, MemberId = 10, Role = "test" }] };

        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-relation-relation.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });
        var readRelation = target.Read() as Relation;

        CompareRelations(expected, readRelation);
    }

    [Fact]
    public void Read_ReadsRelationWithTags()
    {
        var expected = new Relation
        {
            Id = 2,
            Tags = [new("name", "test"), new("name-2", "test-2")],
            Members = [new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" }]
        };

        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-relation-with-tags.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });
        var readRelation = target.Read() as Relation;

        CompareRelations(expected, readRelation);
    }

    [Fact]
    public void Read_ReadsRelationWithTagsAndUnknownElement()
    {
        var expected = new Relation
        {
            Id = 2,
            Tags = [new("name", "test"), new("name-2", "test-2")],
            Members = [new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" }]
        };

        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-relation-with-tags-and-unknown-element.osm"), new OsmXmlReaderSettings() { ReadMetadata = false });
        var readRelation = target.Read() as Relation;

        CompareRelations(expected, readRelation);
    }

    [Fact]
    public void Read_ReadsRelationWithAllProperties()
    {
        var expected = new Relation
        {
            Id = 1,
            Tags = [],
            Members = [new RelationMember { MemberType = EntityType.Node, MemberId = 10, Role = "test" }],
            Metadata = _metadata
        };

        OsmXmlReader target = new(TestDataReader.OsmXml.Open("osm-relation-all-properties.osm"), new OsmXmlReaderSettings() { ReadMetadata = true });
        var readRelation = target.Read() as Relation;

        CompareRelations(expected, readRelation);
    }
}
