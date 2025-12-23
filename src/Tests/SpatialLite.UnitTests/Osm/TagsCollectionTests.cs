using SpatialLite.Osm;

namespace SpatialLite.UnitTests.Osm;

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
        Assert.Contains(_tags[2], target);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Constructor_IEnumerable_ThrowsArgumentException_KeyIsNullOrEmpty(string? key)
    {
        var exception = Record.Exception(() => new TagsCollection([new(key!, "value")]));

        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void Constructor_IEnumerable_ThrowsArgumentException_ValueIsNull()
    {
        var exception = Record.Exception(() => new TagsCollection([new("key", null!)]));

        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void Add_KeyValuePair_AddsTag()
    {
        var target = new TagsCollection
        {
            _tags[0]
        };

        Assert.Contains(_tags[0], target);
    }

    [Fact]
    public void Add_StringString_AddsTag()
    {
        var target = new TagsCollection
        {
            { _tags[0].Key, _tags[0].Value }
        };

        Assert.Contains(_tags[0], target);
    }

    [Fact]
    public void Indexer_Set_AddsTag()
    {
        var target = new TagsCollection();
        target[_tags[0].Key] = _tags[0].Value;

        Assert.Contains(_tags[0], target);
    }

    [Fact]
    public void Indexer_Set_UpdatesTag()
    {
        var target = new TagsCollection
        {
            { "key", "value" }
        };

        var expectedValue = "new value";
        target["key"] = expectedValue;

        Assert.Equal(expectedValue, target["key"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Add_StringString_ThrowsArgumentException_KeyIsNullOrEmpty(string? key)
    {
        var target = new TagsCollection();

        var exception = Record.Exception(() => target.Add(key!, "value"));

        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void Add_StringString_ThrowsArgumentNullException_ValueIsNull()
    {
        var target = new TagsCollection();

        var exception = Record.Exception(() => target.Add("key", null!));

        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Add_KeyValuePair_ThrowsArgumentException_KeyIsNullOrEmpty(string? key)
    {
        var target = new TagsCollection();

        var exception = Record.Exception(() => target.Add(new KeyValuePair<string, string>(key!, "value")));

        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void Add_KeyValuePair_ThrowsArgumentNullException_ValueIsNull()
    {
        var target = new TagsCollection();

        var exception = Record.Exception(() => target.Add(new KeyValuePair<string, string>("key", null!)));

        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Indexer_Set_ThrowsArgumentException_KeyIsNullOrEmpty(string? key)
    {
        var target = new TagsCollection();

        var exception = Record.Exception(() => target[key!] = "value");

        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void Indexer_Set_ThrowsArgumentNullException_ValueIsNull()
    {
        var target = new TagsCollection();

        var exception = Record.Exception(() => target["key"] = null!);

        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void Keys_AreCaseSensitive_WithOrdinalComparison()
    {
        var target = new TagsCollection
        {
            { "Key", "value1" },
            { "key", "value2" }
        };

        // Should have both keys since they are different with case-sensitive comparison
        Assert.Equal(2, target.Count);
        Assert.Equal("value1", target["Key"]);
        Assert.Equal("value2", target["key"]);
        Assert.True(target.ContainsKey("Key"));
        Assert.True(target.ContainsKey("key"));
        Assert.False(target.ContainsKey("KEY"));
    }
}
