using SpatialLite.Osm;

namespace SpatialLITE.UnitTests.Osm;

public class TagsCollectionTests
{
    private readonly KeyValuePair<string, string>[] _tags = [
        new("test-key-1", "test-value-1"),
        new("test-key-2", "test-value-2"),
        new("test-key-3", "test-value-3")
    ];

    [Fact]
    public void Constructor_CreatesEmptyTagsCollection()
    {
        var target = new TagsCollection();

        Assert.Empty(target);
    }

    [Fact]
    public void Constructor_IEnumerable_CreatesCollectionWithGivenTags()
    {
        var target = new TagsCollection(_tags);

        Assert.Equal(_tags.Length, target.Count);
        Assert.Contains(_tags[0], target);
        Assert.Contains(_tags[1], target);
    }

    [Fact]
    public void Add_AddsTag()
    {
        var target = new TagsCollection
        {
            _tags[0]
        };

        Assert.Contains(_tags[0], target);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Add_ThrowsArgumentException_KeyIsNullOrEmpty(string? key)
    {
        var target = new TagsCollection();
        Assert.Throws<ArgumentException>(() => target.Add(key!, "value"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Add_ThrowsArgumentException_ValueIsNullOrEmpty(string? value)
    {
        var target = new TagsCollection();
        Assert.Throws<ArgumentException>(() => target.Add("key", value!));
    }
}
