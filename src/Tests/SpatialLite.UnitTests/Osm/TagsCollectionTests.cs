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
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Constructor_IEnumerable_ThrowsArgumentException_KeyIsNullOrEmpty(string? key)
    {
        Assert.Throws<ArgumentException>(() => new TagsCollection([new(key!, "value")]));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Constructor_IEnumerable_ThrowsArgumentException_ValueIsNullOrEmpty(string? value)
    {
        Assert.Throws<ArgumentException>(() => new TagsCollection([new("key", value!)]));
    }

    [Fact]
    public void Add_KeyValuePair_AddsTag()
    {
        var target = new TagsCollection();
        target.Add(_tags[0]);

        Assert.Contains(_tags[0], target);
    }

    [Fact]
    public void Add_StringString_AddsTag()
    {
        var target = new TagsCollection();
        target.Add(_tags[0].Key, _tags[0].Value);

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
        var target = new TagsCollection();
        target.Add("key", "value");

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
        Assert.Throws<ArgumentException>(() => target.Add(key!, "value"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Add_StringString_ThrowsArgumentException_ValueIsNullOrEmpty(string? value)
    {
        var target = new TagsCollection();
        Assert.Throws<ArgumentException>(() => target.Add("key", value!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Add_KeyValuePair_ThrowsArgumentException_KeyIsNullOrEmpty(string? key)
    {
        var target = new TagsCollection();
        Assert.Throws<ArgumentException>(() => target.Add(new KeyValuePair<string, string>(key!, "value")));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Add_KeyValuePair_ThrowsArgumentException_ValueIsNullOrEmpty(string? value)
    {
        var target = new TagsCollection();
        Assert.Throws<ArgumentException>(() => target.Add(new KeyValuePair<string, string>("key", value!)));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Indexer_Set_ThrowsArgumentException_KeyIsNullOrEmpty(string? key)
    {
        var target = new TagsCollection();

        Assert.Throws<ArgumentException>(() => target[key!] = "value");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Indexer_Set_ThrowsArgumentException_ValueIsNullOrEmpty(string? value)
    {
        var target = new TagsCollection();
        Assert.Throws<ArgumentException>(() => target["key"] = value!);
    }

    [Fact]
    public void Constructor_LazyInitialization_DoesNotCreateUnderlyingCollectionUntilFirstAdd()
    {
        var target = new TagsCollection();

        // Should be empty but not have initialized internal storage yet
        Assert.Empty(target);

        // After adding first tag, should have content
        target.Add("key", "value");
        Assert.Single(target);
        Assert.NotEmpty(target);
    }

    [Fact]
    public void Keys_AreCaseSensitive_WithOrdinalComparison()
    {
        var target = new TagsCollection();
        target.Add("Key", "value1");
        target.Add("key", "value2");

        // Should have both keys since they are different with case-sensitive comparison
        Assert.Equal(2, target.Count);
        Assert.Equal("value1", target["Key"]);
        Assert.Equal("value2", target["key"]);
        Assert.True(target.ContainsKey("Key"));
        Assert.True(target.ContainsKey("key"));
        Assert.False(target.ContainsKey("KEY"));
    }
}
