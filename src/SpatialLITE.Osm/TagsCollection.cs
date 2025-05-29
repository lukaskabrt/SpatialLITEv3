using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace SpatialLite.Osm;

/// <summary>
/// Represents a collection of tags that are accessible by their key.
/// </summary>
public class TagsCollection : IDictionary<string, string>, IReadOnlyDictionary<string, string>
{
    private readonly Dictionary<string, string> _tags;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagsCollection"/> class that is empty and uses case-insensitive key comparison.
    /// </summary>
    public TagsCollection()
    {
        _tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TagsCollection"/> class with a specified capacity.
    /// </summary>
    /// <param name="capacity"> The initial number of elements that the <see cref="TagsCollection"/> can contain.</param>
    public TagsCollection(int capacity)
    {
        _tags = new Dictionary<string, string>(capacity, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TagsCollection"/> class that contains elements copied from the specified collection.
    /// and uses case-insensitive key comparison.
    /// </summary>
    /// <param name="tags">The collection whose elements are copied to the new <see cref="TagsCollection"/>.</param>
    public TagsCollection(IEnumerable<KeyValuePair<string, string>> tags)
    {
        _tags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var tag in tags)
        {
            ValidateTag(tag.Key, tag.Value);
            _tags.Add(tag.Key, tag.Value);
        }
    }

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>The value associated with the specified key.</returns>
    public string this[string key]
    {
        get => _tags[key];
        set
        {
            ValidateTag(key, value);
            _tags[key] = value;
        }
    }

    /// <summary>
    /// Gets a collection containing the keys in the <see cref="TagsCollection"/>.
    /// </summary>
    public ICollection<string> Keys => _tags.Keys;

    /// <summary>
    /// Gets an enumerable collection containing the keys in the <see cref="TagsCollection"/>.
    /// </summary>
    IEnumerable<string> IReadOnlyDictionary<string, string>.Keys => _tags.Keys;

    /// <summary>
    /// Gets a collection containing the values in the <see cref="TagsCollection"/>.
    /// </summary>
    public ICollection<string> Values => _tags.Values;

    /// <summary>
    /// Gets an enumerable collection containing the values in the <see cref="TagsCollection"/>.
    /// </summary>
    IEnumerable<string> IReadOnlyDictionary<string, string>.Values => _tags.Values;

    /// <summary>
    /// Gets the number of tag elements contained in the <see cref="TagsCollection"/>.
    /// </summary>
    public int Count => _tags.Count;

    /// <summary>
    /// Gets a value indicating whether the <see cref="TagsCollection"/> is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Adds a tag with the specified key and value to the collection.
    /// </summary>
    /// <param name="key">The key of the tag to add.</param>
    /// <param name="value">The value of the tag to add.</param>
    /// <exception cref="ArgumentException">Thrown when the key or value is null or empty.</exception>
    public void Add(string key, string value)
    {
        ValidateTag(key, value);

        _tags.Add(key, value);
    }

    private static void ValidateTag(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        }

        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        }
    }

    /// <summary>
    /// Determines whether the <see cref="TagsCollection"/> contains a tag with the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the collection.</param>
    /// <returns><c>true</c> if the collection contains a tag with the specified key; otherwise, <c>false</c>.</returns>
    public bool ContainsKey(string key) => _tags.ContainsKey(key);

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
    /// <returns><c>true</c> if the collection contains a tag with the specified key; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value) => _tags.TryGetValue(key, out value);

    /// <summary>
    /// Removes the tag with the specified key from the collection.
    /// </summary>
    /// <param name="key">The key of the tag to remove.</param>
    /// <returns><c>true</c> if the tag is successfully removed; otherwise, <c>false</c>.</returns>
    public bool Remove(string key) => _tags.Remove(key);

    /// <summary>
    /// Removes all tags from the collection.
    /// </summary>
    public void Clear() => _tags.Clear();

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator for the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => _tags.GetEnumerator();

    /// <summary>
    /// Adds the specified tag to the collection.
    /// </summary>
    /// <param name="item">The tag to add.</param>
    public void Add(KeyValuePair<string, string> item) => Add(item.Key, item.Value);

    /// <summary>
    /// Determines whether the collection contains a specific tag.
    /// </summary>
    /// <param name="item">The tag to locate in the collection.</param>
    /// <returns><c>true</c> if the tag is found; otherwise, <c>false</c>.</returns>
    public bool Contains(KeyValuePair<string, string> item) => _tags.Contains(item);

    /// <summary>
    /// Removes the specified tag from the collection.
    /// </summary>
    /// <param name="item">The tag to remove.</param>
    /// <returns><c>true</c> if the tag was successfully removed; otherwise, <c>false</c>.</returns>
    public bool Remove(KeyValuePair<string, string> item) => ((ICollection<KeyValuePair<string, string>>)_tags).Remove(item);

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator for the collection.</returns>
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _tags.GetEnumerator();

    /// <summary>
    /// Copies the elements of the collection to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, string>>)_tags).CopyTo(array, arrayIndex);
}
