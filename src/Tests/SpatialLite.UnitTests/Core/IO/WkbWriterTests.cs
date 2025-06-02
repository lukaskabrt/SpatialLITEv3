using SpatialLite.Core.Geometries;
using SpatialLite.Core.IO;
using SpatialLITE.UnitTests.Data;

namespace SpatialLITE.UnitTests.Core.IO;

public class WkbWriterTests
{
    [Fact]
    public void Constructor_StreamSettings_SetsSettings()
    {
        var settings = new WkbWriterSettings();
        using var target = new WkbWriter(new MemoryStream(), settings);

        Assert.Same(settings, target.Settings);
    }

    [Fact]
    public void Constructor_StreamSettings_ThrowsArgumentNullExceptionIfStreamIsNull()
    {
        Stream stream = null!;
        Assert.Throws<ArgumentNullException>(() => new WkbWriter(stream, new WkbWriterSettings()));
    }

    [Fact]
    public void Constructor_StreamSettings_ThrowsArgumentNullExceptionIfSettingsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new WkbWriter(new MemoryStream(), null!));
    }

    [Fact]
    public void Constructor_PathSettings_SetsSettings()
    {
        var settings = new WkbWriterSettings();
        using var target = new WkbWriter(new MemoryStream(), settings);
        Assert.Same(settings, target.Settings);
    }

    [Fact]
    public void Constructor_PathSettings_CreatesOutputFile()
    {
        var filename = Path.GetTempFileName();
        try
        {
            var settings = new WkbWriterSettings();
            using var target = new WkbWriter(filename, settings);
            Assert.True(File.Exists(filename));
        }
        finally
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }
    }

    [Fact]
    public void Constructor_PathSettings_ThrowsArgumentNullExceptionIfStreamIsNull()
    {
        string path = null!;
        Assert.Throws<ArgumentNullException>(() => new WkbWriter(path, new WkbWriterSettings()));
    }

    [Fact]
    public void Constructor_PathSettings_ThrowsArgumentNullExceptionIfSettingsIsNull()
    {
        var path = Path.GetTempPath();
        Assert.Throws<ArgumentNullException>(() => new WkbWriter(path, null!));
    }

    [Fact]
    public void Constructor_ThrowsExceptionIfEncodingIsSetToBigEndian()
    {
        using var stream = new MemoryStream();
        Assert.Throws<NotSupportedException>(() => new WkbWriter(stream, new WkbWriterSettings { Encoding = BinaryEncoding.BigEndian }));
    }

    [Fact]
    public void WkbWriter_Write_WritesLittleEndianEncodingByte()
    {
        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings { Encoding = BinaryEncoding.LittleEndian });

        target.Write(new Point());

        AssertBytesEqual(stream.ToArray(), 0, [(byte)BinaryEncoding.LittleEndian]);
    }

    [Fact]
    public void Write_WritesEmptyPointAsEmptyGeometryCollection()
    {
        var point = (Point)ParseWKT("point empty");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(point);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("collection-empty.wkb"));
    }

    [Fact]
    public void Write_Writes2DPoint()
    {
        var point = (Point)ParseWKT("point (-10.1 15.5)");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(point);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("point-2D.wkb"));
    }

    [Fact]
    public void Write_WritesEmptyLineString()
    {
        var linestring = (LineString)ParseWKT("linestring empty");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(linestring);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("linestring-empty.wkb"));
    }

    [Fact]
    public void Write_Writes2DLineString()
    {
        var linestring = (LineString)ParseWKT("linestring (-10.1 15.5, 20.2 -25.5, 30.3 35.5)");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(linestring);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("linestring-2D.wkb"));
    }

    [Fact]
    public void Write_WritesEmptyPolygon()
    {
        var polygon = (Polygon)ParseWKT("polygon empty");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(polygon);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("polygon-empty.wkb"));
    }

    [Fact]
    public void Write_Writes2DPolygonOnlyExteriorRing()
    {
        var polygon = (Polygon)ParseWKT("polygon ((-10.1 15.5, 20.2 -25.5, 30.3 35.5))");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(polygon);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("polygon-ext-2D.wkb"));
    }

    [Fact]
    public void Write_WritesEmptyMultipoint()
    {
        var multipoint = (MultiPoint)ParseWKT("multipoint empty");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(multipoint);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("multipoint-empty.wkb"));
    }

    [Fact]
    public void Write_Writes2DMultiPoint()
    {
        var multipoint = (MultiPoint)ParseWKT("multipoint ((-10.1 15.5),(20.2 -25.5))");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(multipoint);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("multipoint-2D.wkb"));
    }

    [Fact]
    public void Write_WritesEmptyMultiLineString()
    {
        var multilinestring = (MultiLineString)ParseWKT("multilinestring empty");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(multilinestring);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("multilinestring-empty.wkb"));
    }

    [Fact]
    public void Write_Writes2DMultiLineString()
    {
        var multilinestring = (MultiLineString)ParseWKT("multilinestring ((-10.1 15.5, 20.2 -25.5, 30.3 35.5),(-10.1 15.5, 20.2 -25.5, 30.3 35.5))");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(multilinestring);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("multilinestring-2D.wkb"));
    }

    [Fact]
    public void Write_WritesEmptyMultiPolygon()
    {
        var multipolygon = (MultiPolygon)ParseWKT("multipolygon empty");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(multipolygon);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("multipolygon-empty.wkb"));
    }

    [Fact]
    public void Write_Writes2DMultiPolygon()
    {
        var multipolygon = (MultiPolygon)ParseWKT("multipolygon (((-10.1 15.5, 20.2 -25.5, 30.3 35.5)),((-10.1 15.5, 20.2 -25.5, 30.3 35.5)))");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(multipolygon);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("multipolygon-2D.wkb"));
    }

    [Fact]
    public void Write_WritesEmptyGeometryCollection()
    {
        var collection = (GeometryCollection<Geometry>)ParseWKT("geometrycollection empty");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(collection);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("collection-empty.wkb"));
    }

    [Fact]
    public void Write_Writes2DGeometryCollection()
    {
        var collection = (GeometryCollection<Geometry>)ParseWKT("geometrycollection (point (-10.1 15.5))");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(collection);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("collection-2D.wkb"));
    }

    [Fact]
    public void Write_WritesCollectionWithPointLineStringAndPolygon()
    {
        var collection = (GeometryCollection<Geometry>)ParseWKT("geometrycollection (point (-10.1 15.5),linestring (-10.1 15.5, 20.2 -25.5, 30.3 35.5),polygon ((-10.1 15.5, 20.2 -25.5, 30.3 35.5)))");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(collection);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("collection-pt-ls-poly.wkb"));
    }

    [Fact]
    public void Write_WritesCollectionWithMultiGeometries()
    {
        var collection = (GeometryCollection<Geometry>)ParseWKT("geometrycollection (multipoint empty,multilinestring empty,multipolygon empty)");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(collection);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("collection-multi.wkb"));
    }

    [Fact]
    public void Write_WritesNestedCollection()
    {
        var collection = (GeometryCollection<Geometry>)ParseWKT("geometrycollection (geometrycollection (point (-10.1 15.5)))");

        using var stream = new MemoryStream();
        using var target = new WkbWriter(stream, new WkbWriterSettings());

        target.Write(collection);

        AssertBytesEqual(stream.ToArray(), TestDataReader.CoreIO.Read("collection-nested.wkb"));
    }

    private static void AssertBytesEqual(byte[] array, byte[] expected)
    {
        Assert.Equal(array.Length, expected.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], array[i]);
        }
    }

    private static void AssertBytesEqual(byte[] array, int offset, byte[] expected)
    {
        foreach (var (expectedByte, i) in expected.Select((b, i) => (b, i)))
        {
            Assert.Equal(expectedByte, array[i + offset]);
        }
    }

    private static Geometry ParseWKT(string wkt) => WktReader.Parse(wkt)!;
}
