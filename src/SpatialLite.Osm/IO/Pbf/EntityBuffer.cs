﻿using SpatialLite.Osm.IO.Pbf.Contracts;

namespace SpatialLite.Osm.IO.Pbf;

/// <summary>
/// Stores entities to be written to output PBF.
/// </summary>
/// <typeparam name="T">The type of entities to store.</typeparam>
internal class EntityBuffer<T> : IStringTableBuilder, IEnumerable<T> where T : class, IOsmEntity
{

    private readonly List<T> _storage;
    private readonly Dictionary<string, uint> _stringTable;

    private int _stringTableSize = 0;
    private int _estimatedEntityDataSize = 0;
    private readonly bool _writeMetadata;

    /// <summary>
    /// Initializes a new instance of the EntityBuffer class.
    /// </summary>
    /// <param name="writeMetadata">A value indicating whether writer is writing entity metadata (necessary for size estimation).</param>
    public EntityBuffer(bool writeMetadata)
    {
        _storage = new List<T>(10000);
        _stringTable = new Dictionary<string, uint>();

        _stringTableSize = 0;
        _estimatedEntityDataSize = 0;

        _writeMetadata = writeMetadata;

        //add dummy value to the string table to get rid of 0 index
        GetStringIndex(string.Empty);
    }

    /// <summary>
    /// Get maximal estimated size of data in buffer.
    /// </summary>
    /// <remarks>
    /// Actual size of the data should be smaller because proto buffer library compresses data.
    /// </remarks>
    public int EstimatedMaxSize
    {
        get
        {
            return _estimatedEntityDataSize + _stringTableSize;
        }
    }

    /// <summary>
    /// Gets number of entities in the buffer.
    /// </summary>
    public int Count
    {
        get
        {
            return _storage.Count;
        }
    }

    /// <summary>
    /// Adds specific entity to buffer
    /// </summary>
    /// <param name="item">The entity to add</param>
    public void Add(T item)
    {
        _storage.Add(item);

        foreach (var tag in item.Tags)
        {
            GetStringIndex(tag.Key);
            GetStringIndex(tag.Value);

            _estimatedEntityDataSize += 8;
        }

        if (item.EntityType == EntityType.Node)
        {
            _estimatedEntityDataSize += 3 * 8;
        }
        else if (item.EntityType == EntityType.Way)
        {
            var wayItem = item as Way;
            _estimatedEntityDataSize += 8 + wayItem!.Nodes.Count * 8;
        }
        else if (item.EntityType == EntityType.Relation)
        {
            var relationItem = item as Relation;
            _estimatedEntityDataSize += 8 + relationItem!.Members.Count * 12;
        }

        if (_writeMetadata)
        {
            _estimatedEntityDataSize += 28;
        }
    }

    /// <summary>
    /// Remove all data from buffer.
    /// </summary>
    public void Clear()
    {
        _storage.Clear();
        _stringTable.Clear();

        _estimatedEntityDataSize = 0;
        _stringTableSize = 0;

        //add dummy value to the string table to get rid of 0 index
        GetStringIndex(string.Empty);
    }

    /// <summary>
    /// Gets index of the string in StringTable being constructed.
    /// </summary>
    /// <param name="str">The string to locate in StringTable.</param>
    /// <returns>Index of the string in the StringTable.</returns>
    public uint GetStringIndex(string str)
    {
        if (_stringTable.TryGetValue(str, out uint value))
        {
            return value;
        }
        else
        {
            uint index = (uint)_stringTable.Count;
            _stringTable.Add(str, index);
            _stringTableSize += str.Length;

            return index;
        }
    }

    /// <summary>
    /// Creates a StringTable object with data from StringTable object.
    /// </summary>
    /// <returns>The StringTable object with data from the buffer.</returns>
    public StringTable BuildStringTable()
    {
        StringTable result = new StringTable();
        result.Storage = new List<byte[]>(_stringTable.Count);
        for (int i = 0; i < _stringTable.Count; i++)
        {
            result.Storage.Add(new byte[0]);
        }

        foreach (var item in _stringTable)
        {
            result[item.Value] = item.Key;
        }

        return result;
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>A IEnumerator&lt;T&gt; that can be used to iterate through the collection.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        return _storage.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>A IEnumerator that can be used to iterate through the collection.</returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return _storage.GetEnumerator();
    }
}
