using SpatialLite.Core.Geometries;
using SpatialLite.Core.IO;
using SpatialLITE.Contracts;

namespace SpatialLITE.UnitTests.Core.IO;

public class WktWriterTests
{
    private static readonly List<Coordinate> CoordinatesXY = [
            new(-10.1, 15.5), new(20.2, -25.5), new(30.3, 35.5)
    ];

    private static readonly List<Coordinate> CoordinatesXY2 = [
        new(-1.1, 1.5), new(2.2, -2.5), new(3.3, 3.5)
    ];

    [Fact]
    public void Constructor_StreamSettings_ThrowsArgumentNullExceptionIfStreamIsNull()
    {
        Stream? stream = null;
        Assert.Throws<ArgumentNullException>(() => new WktWriter(stream!, new WktWriterSettings()));
    }

    [Fact]
    public void Constructor_StreamSettings_ThrowsArgumentNullExceptionIfSettingsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new WktWriter(new MemoryStream(), null!));
    }

    [Fact]
    public void Constructor_PathSettings_CreatesOutputFile()
    {
        var filename = Path.GetTempFileName();
        File.Delete(filename);

        var settings = new WktWriterSettings();
        using (var target = new WktWriter(filename, settings))
        {
            ;
        }

        Assert.True(File.Exists(filename));
    }

    [Fact]
    public void Constructor_PathSettings_ThrowsArgumentNullExceptionIfStreamIsNull()
    {
        string? path = null;
        Assert.Throws<ArgumentNullException>(() => new WktWriter(path!, new WktWriterSettings()));
    }

    [Fact]
    public void Constructor_PathSettings_ThrowsArgumentNullExceptionIfSettingsIsNull()
    {
        var path = Path.GetTempFileName();

        Assert.Throws<ArgumentNullException>(() => new WktWriter(path, null!));
    }

    public static IEnumerable<object[]> WriteToStringTestData
    {
        get
        {
            yield return new object[] { new Point(), "point empty" };
            yield return new object[] { new LineString(), "linestring empty" };
            yield return new object[] { new Polygon(), "polygon empty" };
            yield return new object[] { new MultiPoint(), "multipoint empty" };
            yield return new object[] { new MultiLineString(), "multilinestring empty" };
            yield return new object[] { new MultiPolygon(), "multipolygon empty" };
            yield return new object[] { new GeometryCollection<Geometry>(), "geometrycollection empty" };

        }
    }

    [Theory]
    [MemberData(nameof(WriteToStringTestData))]
    public void WriteToString_WritesAllGeometryTypes(Geometry toWrite, string expectedWkt)
    {
        TestWriteGeometry(toWrite, expectedWkt);
    }

    [Fact]
    public void Dispose_ClosesOutputStreamIfWritingToStream()
    {
        var stream = new MemoryStream();

        var target = new WktWriter(stream, new WktWriterSettings());
        target.Dispose();

        Assert.False(stream.CanRead);
    }

    public static IEnumerable<object[]> Write_WritesPointsOfAllDimensionsTestData
    {
        get
        {
            yield return new object[] { new Point(), "point empty" };
            yield return new object[] { new Point(CoordinatesXY[0]), "point (-10.1 15.5)" };
        }
    }

    [Theory]
    [MemberData(nameof(Write_WritesPointsOfAllDimensionsTestData))]
    public void Write_WritesPointsOfAllDimensions(Point toWrite, string expectedWkt)
    {
        TestWriteGeometry(toWrite, expectedWkt);
    }

    public static IEnumerable<object[]> Write_WritesLinestringOfAllDimensionsTestData
    {
        get
        {
            yield return new object[] { new LineString(), "linestring empty" };
            yield return new object[] { new LineString(CoordinatesXY), "linestring (-10.1 15.5, 20.2 -25.5, 30.3 35.5)" };
        }
    }

    [Theory]
    [MemberData(nameof(Write_WritesLinestringOfAllDimensionsTestData))]
    public void Write_WritesLinestringsOfAllDimensions(LineString toWrite, string expectedWkt)
    {
        TestWriteGeometry(toWrite, expectedWkt);
    }

    public static IEnumerable<object[]> Write_WritesPolygonsOfAllDimensionsTestData
    {
        get
        {
            yield return new object[] { new Polygon(), "polygon empty" };
            yield return new object[] { new Polygon(CoordinatesXY), "polygon ((-10.1 15.5, 20.2 -25.5, 30.3 35.5))" };
        }
    }

    [Theory]
    [MemberData(nameof(Write_WritesPolygonsOfAllDimensionsTestData))]
    public void Write_WritesPolygonsOfAllDimensions(Polygon toWrite, string expectedWkt)
    {
        TestWriteGeometry(toWrite, expectedWkt);
    }

    [Fact]
    public void Write_WritesComplexPolygonWitOuterAndInnerRings()
    {
        var wkt = "polygon ((-10.1 15.5, 20.2 -25.5, 30.3 35.5),(-1.1 1.5, 2.2 -2.5, 3.3 3.5),(-1.1 1.5, 2.2 -2.5, 3.3 3.5))";
        var polygon = new Polygon(CoordinatesXY);
        polygon.InteriorRings.Add(CoordinatesXY2);
        polygon.InteriorRings.Add(CoordinatesXY2);

        TestWriteGeometry(polygon, wkt);
    }

    public static IEnumerable<object[]> Write_WritesMultiPointsOfAllDimensionsTestData
    {
        get
        {
            yield return new object[] { new MultiPoint(), "multipoint empty" };
            yield return new object[] { new MultiPoint([new Point(CoordinatesXY[0]), new Point(CoordinatesXY[1])]), "multipoint ((-10.1 15.5),(20.2 -25.5))" };
        }
    }

    [Theory]
    [MemberData(nameof(Write_WritesMultiPointsOfAllDimensionsTestData))]
    public void Write_WritesMultiPointsOfAllDimensions(MultiPoint toWrite, string expectedWkt)
    {
        TestWriteGeometry(toWrite, expectedWkt);
    }

    public static IEnumerable<object[]> Write_WritesMultiLineStringsOfAllDimensionsTestData
    {
        get
        {
            yield return new object[] { new MultiLineString(), "multilinestring empty" };
            yield return new object[] { new MultiLineString([new LineString(CoordinatesXY), new LineString(CoordinatesXY)]),
                "multilinestring ((-10.1 15.5, 20.2 -25.5, 30.3 35.5),(-10.1 15.5, 20.2 -25.5, 30.3 35.5))" };
        }
    }

    [Theory]
    [MemberData(nameof(Write_WritesMultiLineStringsOfAllDimensionsTestData))]
    public void Write_WritesMultiLineStringsOfAllDimensions(MultiLineString toWrite, string expectedWkt)
    {
        TestWriteGeometry(toWrite, expectedWkt);
    }

    public static IEnumerable<object[]> Write_WritesMultiPolygonsOfAllDimensionsTestData
    {
        get
        {
            yield return new object[] { new MultiPolygon(), "multipolygon empty" };
            yield return new object[] { new MultiPolygon([new Polygon(CoordinatesXY), new Polygon(CoordinatesXY)]),
                "multipolygon (((-10.1 15.5, 20.2 -25.5, 30.3 35.5)),((-10.1 15.5, 20.2 -25.5, 30.3 35.5)))" };
        }
    }

    [Theory]
    [MemberData(nameof(Write_WritesMultiPolygonsOfAllDimensionsTestData))]
    public void Write_WritesMultiPolygonsOfAllDimensions(MultiPolygon toWrite, string expectedWkt)
    {
        TestWriteGeometry(toWrite, expectedWkt);
    }

    public static IEnumerable<object[]> Write_WritesGeometryCollectionOfAllDimensionsTestData
    {
        get
        {
            yield return new object[] { new GeometryCollection<Geometry>(), "geometrycollection empty" };
            yield return new object[] { new GeometryCollection<Geometry>([new Point(CoordinatesXY[0])]), "geometrycollection (point (-10.1 15.5))" };
        }
    }

    [Theory]
    [MemberData(nameof(Write_WritesGeometryCollectionOfAllDimensionsTestData))]
    public void Write_WritesGeometryCollectionOfAllDimensions(GeometryCollection<Geometry> toWrite, string expectedWkt)
    {
        TestWriteGeometry(toWrite, expectedWkt);
    }

    [Fact]
    public void Write_WritesCollectionWithAllGeometryTypes()
    {
        var wkt = "geometrycollection (point (-10.1 15.5),linestring (-10.1 15.5, 20.2 -25.5, 30.3 35.5),polygon ((-10.1 15.5, 20.2 -25.5, 30.3 35.5)),multipoint empty,multilinestring empty,multipolygon empty)";
        var collection = new GeometryCollection<Geometry>();
        collection.Geometries.Add(new Point(CoordinatesXY[0]));
        collection.Geometries.Add(new LineString(CoordinatesXY));
        collection.Geometries.Add(new Polygon(CoordinatesXY));
        collection.Geometries.Add(new MultiPoint());
        collection.Geometries.Add(new MultiLineString());
        collection.Geometries.Add(new MultiPolygon());

        TestWriteGeometry(collection, wkt);
    }

    [Fact]
    public void Write_WritesNestedCollection()
    {
        var wkt = "geometrycollection (geometrycollection (point (-10.1 15.5)))";
        var collection = new GeometryCollection<Geometry>();
        var nested = new GeometryCollection<Geometry>();
        nested.Geometries.Add(new Point(CoordinatesXY[0]));
        collection.Geometries.Add(nested);

        TestWriteGeometry(collection, wkt);
    }

    private static void TestWriteGeometry(IGeometry geometry, string expectedWkt)
    {
        var stream = new MemoryStream();
        using (var writer = new WktWriter(stream, new WktWriterSettings()))
        {
            writer.Write(geometry);
        }

        using TextReader tr = new StreamReader(new MemoryStream(stream.ToArray()));

        var wkt = tr.ReadToEnd();
        Assert.Equal(expectedWkt, wkt, StringComparer.OrdinalIgnoreCase);
    }
}
