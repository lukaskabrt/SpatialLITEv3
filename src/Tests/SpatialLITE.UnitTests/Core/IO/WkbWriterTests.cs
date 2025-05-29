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
        using (var target = new WkbWriter(new MemoryStream(), settings))
        {
            Assert.Same(settings, target.Settings);
        }
    }

    [Fact]
    public void Constructor_PathSettings_CreatesOutputFile()
    {
        string filename = Path.GetTempFileName();

        var settings = new WkbWriterSettings();
        using (var target = new WkbWriter(filename, settings))
        {
        }

        Assert.True(File.Exists(filename));
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
        string path = Path.GetTempPath();

        Assert.Throws<ArgumentNullException>(() => new WkbWriter(path, null!));
    }

    [Fact]
    public void Constructor_ThrowsExceptionIfEncodingIsSetToBigEndian()
    {
        var stream = new MemoryStream();
        Assert.Throws<NotSupportedException>(() => new WkbWriter(stream, new WkbWriterSettings() { Encoding = BinaryEncoding.BigEndian }));
    }

    [Fact]
    public void WkbWriter_Write_WritesLittleEndianEncodingByte()
    {
        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings() { Encoding = BinaryEncoding.LittleEndian }))
        {
            target.Write(new Point());

            AssertBytesEqual(stream.ToArray(), 0, [(byte)BinaryEncoding.LittleEndian]);
        }
    }

    [Fact]
    public void Write_WritesEmptyPointAsEmptyGeometryCollection()
    {
        var wkt = "point empty";
        var point = (Point)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(point);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("collection-empty.wkb"));
        }
    }

    [Fact]
    public void Write_Writes2DPoint()
    {
        var wkt = "point (-10.1 15.5)";
        var point = (Point)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(point);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("point-2D.wkb"));
        }
    }

    [Fact]
    public void Write_WritesEmptyLineString()
    {
        var wkt = "linestring empty";
        var linestring = (LineString)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(linestring);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("linestring-empty.wkb"));
        }
    }

    [Fact]
    public void Write_Writes2DLineString()
    {
        var wkt = "linestring (-10.1 15.5, 20.2 -25.5, 30.3 35.5)";
        var linestring = (LineString)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(linestring);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("linestring-2D.wkb"));
        }
    }

    [Fact]
    public void Write_WritesEmptyPolygon()
    {
        var wkt = "polygon empty";
        var polygon = (Polygon)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(polygon);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("polygon-empty.wkb"));
        }
    }

    [Fact]
    public void Write_Writes2DPolygonOnlyExteriorRing()
    {
        var wkt = "polygon ((-10.1 15.5, 20.2 -25.5, 30.3 35.5))";
        var polygon = (Polygon)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(polygon);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("polygon-ext-2D.wkb"));
        }
    }

    [Fact]
    public void Write_WritesEmptyMultipoint()
    {
        var wkt = "multipoint empty";
        var multipoint = (MultiPoint)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(multipoint);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("multipoint-empty.wkb"));
        }
    }

    [Fact]
    public void Write_Writes2DMultiPoint()
    {
        var wkt = "multipoint ((-10.1 15.5),(20.2 -25.5))";
        var multipoint = (MultiPoint)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(multipoint);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("multipoint-2D.wkb"));
        }
    }

    [Fact]
    public void Write_WritesEmptyMultiLineString()
    {
        var wkt = "multilinestring empty";
        var multilinestring = (MultiLineString)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(multilinestring);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("multilinestring-empty.wkb"));
        }
    }

    [Fact]
    public void Write_Writes2DMultiLineString()
    {
        var wkt = "multilinestring ((-10.1 15.5, 20.2 -25.5, 30.3 35.5),(-10.1 15.5, 20.2 -25.5, 30.3 35.5))";
        var multilinestring = (MultiLineString)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(multilinestring);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("multilinestring-2D.wkb"));
        }
    }

    [Fact]
    public void Write_WritesEmptyMultiPolygon()
    {
        var wkt = "multipolygon empty";
        var multipolygon = (MultiPolygon)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(multipolygon);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("multipolygon-empty.wkb"));
        }
    }

    [Fact]
    public void Write_Writes2DMultiPolygon()
    {
        var wkt = "multipolygon (((-10.1 15.5, 20.2 -25.5, 30.3 35.5)),((-10.1 15.5, 20.2 -25.5, 30.3 35.5)))";
        var multipolygon = (MultiPolygon)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(multipolygon);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("multipolygon-2D.wkb"));
        }
    }

    [Fact]
    public void Write_WritesEmptyGeometryCollection()
    {
        var wkt = "geometrycollection empty";
        var collection = (GeometryCollection<Geometry>)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(collection);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("collection-empty.wkb"));
        }
    }

    [Fact]
    public void Write_Writes2DGeometryCollection()
    {
        var wkt = "geometrycollection (point (-10.1 15.5))";
        var collection = (GeometryCollection<Geometry>)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(collection);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("collection-2D.wkb"));
        }
    }

    [Fact]
    public void Write_WritesCollectionWithPointLineStringAndPolygon()
    {
        var wkt = "geometrycollection (point (-10.1 15.5),linestring (-10.1 15.5, 20.2 -25.5, 30.3 35.5),polygon ((-10.1 15.5, 20.2 -25.5, 30.3 35.5)))";
        var collection = (GeometryCollection<Geometry>)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(collection);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("collection-pt-ls-poly.wkb"));
        }
    }

    [Fact]
    public void Write_WritesCollectionWithMultiGeometries()
    {
        var wkt = "geometrycollection (multipoint empty,multilinestring empty,multipolygon empty)";
        var collection = (GeometryCollection<Geometry>)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(collection);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("collection-multi.wkb"));
        }
    }

    [Fact]
    public void Write_WritesNestedCollection()
    {
        var wkt = "geometrycollection (geometrycollection (point (-10.1 15.5)))";
        var collection = (GeometryCollection<Geometry>)ParseWKT(wkt);

        var stream = new MemoryStream();
        using (var target = new WkbWriter(stream, new WkbWriterSettings()))
        {
            target.Write(collection);

            AssertBytesEqual(stream.ToArray(), TestDataReader.Read("collection-nested.wkb"));
        }
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
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], array[i + offset]);
        }
    }

    private static Geometry ParseWKT(string wkt)
    {
        return WktReader.Parse(wkt)!;
    }
}
