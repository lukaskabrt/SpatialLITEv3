using SpatialLite.Contracts;
using SpatialLite.Core.Geometries;

namespace SpatialLite.Core.IO;

/// <summary>
/// Provides functions for reading and parsing geometries from WKB format.
/// </summary>
public class WkbReader : IDisposable
{

    private readonly BinaryReader _inputReader;
    private readonly FileStream? _inputFileStream;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the WkbReader class that reads data from specific stream.
    /// </summary>
    /// <param name="input">The stream to read data from.</param>
    public WkbReader(Stream input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input), "Input stream cannot be null");
        }

        _inputReader = new BinaryReader(input);
    }

    /// <summary>
    /// Initializes a new instance of the WkbReader class that reads data from specific file.
    /// </summary>
    /// <param name="path">Path to the file to read data from.</param>
    public WkbReader(string path)
    {
        _inputFileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        _inputReader = new BinaryReader(_inputFileStream);
    }

    /// <summary>
    /// Parses data from the binary array.
    /// </summary>
    /// <param name="wkb">The binary array with WKB serialized geometry.</param>
    /// <returns>Parsed geometry.</returns>
    /// <exception cref="WkbFormatException">Throws exception if wkb array does not contains valid WKB geometry.</exception>
    public static IGeometry? Parse(byte[] wkb)
    {
        if (wkb == null)
        {
            throw new ArgumentNullException(nameof(wkb));
        }

        using (MemoryStream ms = new(wkb))
        {
            using (BinaryReader reader = new(ms))
            {
                if (reader.PeekChar() == -1)
                {
                    return null;
                }

                try
                {
                    BinaryEncoding encoding = (BinaryEncoding)reader.ReadByte();
                    if (encoding == BinaryEncoding.BigEndian)
                    {
                        throw new NotSupportedException("Big endian encoding is not supported in the current version of WkbReader.");
                    }

                    var parsed = ReadGeometry(reader);

                    return parsed;
                }
                catch (EndOfStreamException)
                {
                    throw new WkbFormatException("End of stream reached before end of valid WKB geometry end.");
                }
            }
        }
    }

    /// <summary>
    /// Parses data from the binary array as given geometry type.
    /// </summary>
    /// <typeparam name="T">The Geometry type to be parsed.</typeparam>
    /// <param name="wkb">The binary array with WKB serialized geometry.</param>
    /// <returns>Parsed geometry.</returns>
    /// <exception cref="WkbFormatException">Throws exception if wkb array does not contains valid WKB geometry of specific type.</exception>
    public static T? Parse<T>(byte[] wkb) where T : IGeometry
    {
        var parsed = Parse(wkb);

        if (parsed != null)
        {
            if (parsed is not T result)
            {
                throw new WkbFormatException("Input doesn't contain valid WKB representation of the specified geometry type.");
            }

            return result;
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Releases all resources used by the ComponentLibrary.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Read geometry in WKB format from the input.
    /// </summary>
    /// <returns>Parsed geometry or null if no other geometry is available.</returns>
    public IGeometry? Read()
    {
        if (_inputReader.PeekChar() == -1)
        {
            return null;
        }

        try
        {
            BinaryEncoding encoding = (BinaryEncoding)_inputReader.ReadByte();
            if (encoding == BinaryEncoding.BigEndian)
            {
                throw new NotSupportedException("Big endian encoding is not supported in the current version of WkbReader.");
            }

            return ReadGeometry(_inputReader);
        }
        catch (EndOfStreamException)
        {
            throw new WkbFormatException("End of stream reached before end of valid WKB geometry end.");
        }
    }

    /// <summary>
    /// Read geometry in WKB format from the input.
    /// </summary>
    /// <typeparam name="T">The Geometry type to be parsed.</typeparam>
    /// <returns>Geometry object of specific type read from the input, or null if no other geometry is available.</returns>
    /// <exception cref="WkbFormatException">Throws exception if wkb array does not contains valid WKB geometry of specific type.</exception>
    public T? Read<T>() where T : IGeometry
    {
        var parsed = Read();

        if (parsed != null)
        {
            if (parsed is not T result)
            {
                throw new WkbFormatException("Input doesn't contain valid WKB representation of the specified geometry type.");
            }

            return result;
        }
        else
        {
            return default;
        }
    }

    /// <summary>
    /// Reads Coordinate from the BinaryReader.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="is3D">Bool value indicating whether Coordinate has Z value.</param>
    /// <param name="isMeasured">Bool value indicating whether Coordinate has M value.</param>
    /// <returns>Parsed Coordinate.</returns>
    private static Coordinate ReadCoordinate(BinaryReader reader, bool is3D, bool isMeasured)
    {
        double x = reader.ReadDouble();
        double y = reader.ReadDouble();
        _ = is3D ? reader.ReadDouble() : double.NaN;
        _ = isMeasured ? reader.ReadDouble() : double.NaN;

        return new Coordinate(x, y);
    }

    /// <summary>
    /// Reads a list of coordinates from the BinaryReader.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="is3D">Bool value indicating whether coordinates has Z value.</param>
    /// <param name="isMeasured">Bool value indicating whether coordinates has M value.</param>
    /// <returns>Parsed Coordinate.</returns>
    private static List<Coordinate> ReadCoordinates(BinaryReader reader, bool is3D, bool isMeasured)
    {
        int pointCount = (int)reader.ReadUInt32();

        List<Coordinate> result = new(pointCount);
        for (int i = 0; i < pointCount; i++)
        {
            result.Add(ReadCoordinate(reader, is3D, isMeasured));
        }

        return result;
    }

    /// <summary>
    /// Reads Geometry from the reader.
    /// </summary>
    /// <param name="reader">The reader used to read data from input stream.</param>
    /// <returns>Geometry read from the input.</returns>
    private static IGeometry ReadGeometry(BinaryReader reader)
    {
        WkbGeometryType geometryType = (WkbGeometryType)reader.ReadUInt32();

        bool is3D, isMeasured;
        WkbGeometryType basicType;
        GetGeometryTypeDetails(geometryType, out basicType, out is3D, out isMeasured);

        return basicType switch
        {
            WkbGeometryType.Point => ReadPoint(reader, is3D, isMeasured),
            WkbGeometryType.LineString => ReadLineString(reader, is3D, isMeasured),
            WkbGeometryType.Polygon => ReadPolygon(reader, is3D, isMeasured),
            WkbGeometryType.MultiPoint => ReadMultiPoint(reader, is3D, isMeasured),
            WkbGeometryType.MultiLineString => ReadMultiLineString(reader, is3D, isMeasured),
            WkbGeometryType.MultiPolygon => ReadMultiPolygon(reader, is3D, isMeasured),
            WkbGeometryType.GeometryCollection => ReadGeometryCollection(reader),
            _ => throw new WkbFormatException("Unknown geometry type."),
        };
    }

    /// <summary>
    /// Reads Point from the reader.
    /// </summary>
    /// <param name="reader">The reader used to read data from input stream.</param>
    /// <param name="is3D">bool value indicating whether point being read has Z-dimension.</param>
    /// <param name="isMeasured">bool value indicating whether point being read has M-value.</param>
    /// <returns>Point read from the input</returns>
    private static Point ReadPoint(BinaryReader reader, bool is3D, bool isMeasured)
    {
        Coordinate position = ReadCoordinate(reader, is3D, isMeasured);
        return new Point(position);
    }

    /// <summary>
    /// Reads LineString from the reader.
    /// </summary>
    /// <param name="reader">The reader used to read data from input stream.</param>
    /// <param name="is3D">bool value indicating whether linestring being read has Z-dimension.</param>
    /// <param name="isMeasured">bool value indicating whether linestring being read has M-value.</param>
    /// <returns>Linestring read from the input.</returns>
    private static LineString ReadLineString(BinaryReader reader, bool is3D, bool isMeasured)
    {
        IEnumerable<Coordinate> coordinates = ReadCoordinates(reader, is3D, isMeasured);
        return new LineString(coordinates);
    }

    /// <summary>
    /// Reads Polygon from the reader.
    /// </summary>
    /// <param name="reader">The reader used to read data from input stream.</param>
    /// <param name="is3D">bool value indicating whether polygon being read has Z-dimension.</param>
    /// <param name="isMeasured">bool value indicating whether polygon being read has M-value.</param>
    /// <returns>Polygon read from the input.</returns>
    private static Polygon ReadPolygon(BinaryReader reader, bool is3D, bool isMeasured)
    {
        int ringsCount = (int)reader.ReadUInt32();

        if (ringsCount == 0)
        {
            return new Polygon();
        }

        Polygon result = new(ReadCoordinates(reader, is3D, isMeasured));

        for (int i = 1; i < ringsCount; i++)
        {
            result.InteriorRings.Add(ReadCoordinates(reader, is3D, isMeasured));
        }

        return result;
    }

    /// <summary>
    /// Reads MultiLineString from the reader.
    /// </summary>
    /// <param name="reader">The reader used to read data from input stream.</param>
    /// <param name="is3D">bool value indicating whether multilinestring being read has Z-dimension.</param>
    /// <param name="isMeasured">bool value indicating whether multilinestring being read has M-value.</param>
    /// <returns>MultiLineString read from the input.</returns>
    private static MultiPoint ReadMultiPoint(BinaryReader reader, bool is3D, bool isMeasured)
    {
        int pointsCount = (int)reader.ReadUInt32();

        MultiPoint result = new();
        for (int i = 0; i < pointsCount; i++)
        {
            result.Geometries.Add(ReadPoint(reader, is3D, isMeasured));
        }

        return result;
    }

    /// <summary>
    /// Reads MultiLineString from the reader.
    /// </summary>
    /// <param name="reader">The reader used to read data from input stream.</param>
    /// <param name="is3D">bool value indicating whether multilinestring being read has Z-dimension.</param>
    /// <param name="isMeasured">bool value indicating whether multilinestring being read has M-value.</param>
    /// <returns>MultiLineString read from the input</returns>
    private static MultiLineString ReadMultiLineString(BinaryReader reader, bool is3D, bool isMeasured)
    {
        int pointsCount = (int)reader.ReadUInt32();

        MultiLineString result = new();
        for (int i = 0; i < pointsCount; i++)
        {
            result.Geometries.Add(ReadLineString(reader, is3D, isMeasured));
        }

        return result;
    }

    /// <summary>
    /// Reads MultiPolygon from the reader.
    /// </summary>
    /// <param name="reader">The reader used to read data from input stream.</param>
    /// <param name="is3D">bool value indicating whether MultiPolygon being read has Z-dimension.</param>
    /// <param name="isMeasured">bool value indicating whether MultiPolygon being read has M-value.</param>
    /// <returns>MultiPolygon read from the input.</returns>
    private static MultiPolygon ReadMultiPolygon(BinaryReader reader, bool is3D, bool isMeasured)
    {
        int pointsCount = (int)reader.ReadUInt32();

        MultiPolygon result = new();
        for (int i = 0; i < pointsCount; i++)
        {
            result.Geometries.Add(ReadPolygon(reader, is3D, isMeasured));
        }

        return result;
    }

    /// <summary>
    /// Reads GeometryCollection from the reader.
    /// </summary>
    /// <param name="reader">The reader used to read data from input stream.</param>
    /// <returns>GeometryCollection read from the input.</returns>
    private static GeometryCollection<IGeometry> ReadGeometryCollection(BinaryReader reader)
    {
        int pointsCount = (int)reader.ReadUInt32();

        GeometryCollection<IGeometry> result = new();
        for (int i = 0; i < pointsCount; i++)
        {
            result.Geometries.Add(ReadGeometry(reader));
        }

        return result;
    }

    /// <summary>
    /// Gets details form the WkbGeometryType value.
    /// </summary>
    /// <param name="type">The value to be examined.</param>
    /// <param name="basicType">Outputs type striped of dimension information.</param>
    /// <param name="is3D">Outputs bool value indicating whether type represents 3D geometry.</param>
    /// <param name="isMeasured">Outputs bool value indicating whether type represents measured geometry.</param>
    private static void GetGeometryTypeDetails(WkbGeometryType type, out WkbGeometryType basicType, out bool is3D, out bool isMeasured)
    {
        is3D = ((int)type > 1000 && (int)type < 2000) || (int)type > 3000;
        isMeasured = (int)type > 2000;
        basicType = (WkbGeometryType)((int)type % 1000);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the ComponentLibrary and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _inputReader.Dispose();
                _inputFileStream?.Dispose();
            }

            _disposed = true;
        }
    }
}
