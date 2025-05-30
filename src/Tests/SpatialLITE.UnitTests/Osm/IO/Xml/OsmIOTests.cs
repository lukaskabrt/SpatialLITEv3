using SpatialLite.Osm;

namespace SpatialLITE.UnitTests.Osm.IO.Xml;

/// <summary>
/// Base class for OSM IO tests with shared functionality for entity comparison.
/// </summary>
public abstract class OsmIOTests
{
    /// <summary>
    /// Compares two Node entities for equality.
    /// </summary>
    /// <param name="expected">Expected Node</param>
    /// <param name="actual">Actual Node</param>
    protected static void AssertNodesEqual(Node expected, Node? actual)
    {
        Assert.NotNull(actual);

        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Longitude, actual.Longitude);
        Assert.Equal(expected.Latitude, actual.Latitude);

        AssertTagsEqual(expected.Tags, actual.Tags);
        AssertMetadataEquals(expected.Metadata, actual.Metadata);
    }

    /// <summary>
    /// Compares two Way entities for equality.
    /// </summary>
    /// <param name="expected">Expected Way</param>
    /// <param name="actual">Actual Way</param>
    protected static void AssertWaysEqual(Way expected, Way? actual)
    {
        Assert.NotNull(actual);

        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Nodes.Count, actual.Nodes.Count);
        for (var i = 0; i < expected.Nodes.Count; i++)
        {
            Assert.Equal(expected.Nodes[i], actual.Nodes[i]);
        }

        AssertTagsEqual(expected.Tags, actual.Tags);
        AssertMetadataEquals(expected.Metadata, actual.Metadata);
    }

    /// <summary>
    /// Compares two Relation entities for equality.
    /// </summary>
    /// <param name="expected">Expected Relation</param>
    /// <param name="actual">Actual Relation</param>
    protected static void AssertRelationsEqual(Relation expected, Relation? actual)
    {
        Assert.NotNull(actual);

        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Members.Count, actual.Members.Count);
        for (var i = 0; i < expected.Members.Count; i++)
        {
            Assert.Equal(expected.Members[i], actual.Members[i]);
        }

        AssertTagsEqual(expected.Tags, actual.Tags);
        AssertMetadataEquals(expected.Metadata, actual.Metadata);
    }

    /// <summary>
    /// Compares two TagsCollection objects for equality.
    /// </summary>
    /// <param name="expected">Expected TagsCollection</param>
    /// <param name="actual">Actual TagsCollection</param>
    protected static void AssertTagsEqual(TagsCollection? expected, TagsCollection? actual)
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

    /// <summary>
    /// Compares two EntityMetadata objects for equality.
    /// </summary>
    /// <param name="expected">Expected EntityMetadata</param>
    /// <param name="actual">Actual EntityMetadata</param>
    protected static void AssertMetadataEquals(EntityMetadata? expected, EntityMetadata? actual)
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