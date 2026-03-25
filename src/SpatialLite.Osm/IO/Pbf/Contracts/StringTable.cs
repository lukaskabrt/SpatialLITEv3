using PbfLite;
using System.Text;

namespace SpatialLite.Osm.IO.Pbf.Contracts;

/// <summary>
/// Stores all strings for Primitive block.
/// </summary>
public class StringTable
{

    private List<byte[]> _s = new List<byte[]>();
    private List<string>? _stringList = null;

    /// <summary>
    /// Gets or sets collection of strings serialized as byte array.
    /// </summary>
    public List<byte[]> Storage
    {
        get { return _s; }
        set { _s = value; }
    }

    /// <summary>
    /// Gets or sets string at specified position.
    /// </summary>
    /// <param name="index">The index of the string.</param>
    /// <returns>string at specified position.</returns>
    public string this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Storage.Count);

            if (_stringList == null)
            {
                _stringList = new List<string>(Storage.Count);
                foreach (var item in Storage)
                {
                    _stringList.Add(Encoding.UTF8.GetString(item, 0, item.Length));
                }
            }

            return _stringList[index];
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Storage.Count);

            Storage[index] = Encoding.UTF8.GetBytes(value);
        }
    }

    /// <summary>
    /// Gets or sets string at specified position.
    /// </summary>
    /// <param name="index">The index of the string.</param>
    /// <returns>string at specified position.</returns>
    public string this[uint index]
    {
        get
        {
            return this[(int)index];
        }
        set
        {
            this[(int)index] = value;
        }
    }

    /// <summary>
    /// Serializes the string table to a PBF block writer.
    /// </summary>
    /// <param name="pbf">The PBF block writer to serialize to.</param>
    public void Serialize(ref PbfBlockWriter pbf)
    {
        foreach (var bytes in Storage)
        {
            pbf.WriteFieldHeader(1, WireType.String);
            pbf.WriteLengthPrefixedBytes(bytes);
        }
    }

    /// <summary>
    /// Deserializes a string table from a PBF block reader.
    /// </summary>
    /// <param name="pbf">The PBF block reader to deserialize from.</param>
    /// <returns>A new StringTable instance containing the deserialized strings.</returns>
    public static StringTable Deserialize(ref PbfBlockReader pbf)
    {
        var result = new StringTable();
        var (fieldNumber, wireType) = pbf.ReadFieldHeader();
        while (fieldNumber != 0)
        {
            switch (fieldNumber)
            {
                case 1:
                    result.Storage.Add(pbf.ReadLengthPrefixedBytes().ToArray());
                    break;
                default:
                    pbf.SkipField(wireType);
                    break;
            }

            (fieldNumber, wireType) = pbf.ReadFieldHeader();

        }

        return result;
    }
}
