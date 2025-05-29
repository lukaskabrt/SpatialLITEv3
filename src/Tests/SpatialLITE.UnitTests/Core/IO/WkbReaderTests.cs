using SpatialLite.Core.Geometries;
using SpatialLite.Core.IO;
using SpatialLITE.Contracts;
using SpatialLITE.UnitTests.Data;

namespace SpatialLITE.UnitTests.Core.IO;

public class WkbReaderTests
{
    [Fact]
    public void Constructor_Stream_ThrowsArgumentNullExceptionIfStreamIsNull()
    {
        Stream stream = null!;
        Assert.Throws<ArgumentNullException>(() => new WkbReader(stream));
    }

    [Fact]
    public void Constructor_Path_ThrowsFileNotFoundExceptionIfFileDoesNotExists()
    {
        Assert.Throws<FileNotFoundException>(() => new WkbReader("non-existing-file.wkb"));
    }

    [Fact]
    public void Read_ReturnsNullIfStreamIsEmpty()
    {
        var stream = new MemoryStream();

        var target = new WkbReader(stream);
        var read = target.Read();

        Assert.Null(read);
    }

    [Fact]
    public void Read_ReadsGeometry()
    {
        var expected = ParseWKT<Point>("point zm (-10.1 15.5 100.5 1000.5)");

        var target = new WkbReader(TestDataReader.Open("point-3DM.wkb"));
        var parsed = (Point?)target.Read();

        AssertPointsEqual(parsed, expected);
    }

    [Fact]
    public void Read_ReadsMultipleGeometries()
    {
        var expected1 = ParseWKT<Point>("point zm (-10.1 15.5 100.5 1000.5)");
        var expected2 = ParseWKT<Point>("point zm (-10.2 15.6 100.6 1000.6)");

        var target = new WkbReader(TestDataReader.Open("two-points-3DM.wkb"));

        var parsed1 = (Point?)target.Read();
        var parsed2 = (Point?)target.Read();

        AssertPointsEqual(parsed1, expected1);
        AssertPointsEqual(parsed2, expected2);
    }

    [Fact]
    public void Read_ReturnsNullIfNoMoreGeometriesAreAvailable()
    {
        var target = new WkbReader(TestDataReader.Open("point-3DM.wkb"));

        target.Read();
        var parsed = target.Read();

        Assert.Null(parsed);
    }

    [Fact]
    public void Read_ThrowsExceptionIfWKBDoesNotRepresentGeometry()
    {
        var wkb = new byte[] { 12, 0, 0, 45, 78, 124, 36, 0 };

        using var ms = new MemoryStream(wkb);
        var target = new WkbReader(ms);

        Assert.Throws<WkbFormatException>(target.Read);
    }

    [Fact]
    public void ReadT_ReturnsNullIfStreamIsEmpty()
    {
        var stream = new MemoryStream();

        var target = new WkbReader(stream);
        var read = target.Read<Geometry>();

        Assert.Null(read);
    }

    [Fact]
    public void ReadT_ReadsGeometry()
    {
        var expected = ParseWKT<Point>("point zm (-10.1 15.5 100.5 1000.5)");

        var target = new WkbReader(TestDataReader.Open("point-3DM.wkb"));
        var parsed = target.Read<Point>();

        AssertPointsEqual(parsed, expected);
    }

    [Fact]
    public void ReadT_ReturnsNullIfNoMoreGeometriesAreAvailable()
    {
        var target = new WkbReader(TestDataReader.Open("point-3DM.wkb"));

        target.Read<Point>();
        var parsed = target.Read<Point>();

        Assert.Null(parsed);
    }

    [Fact]
    public void ReadT_ThrowsExceptionIfWKBDoesNotRepresentGeometry()
    {
        var wkb = new byte[] { 12, 0, 0, 45, 78, 124, 36, 0 };

        using var ms = new MemoryStream(wkb);
        var target = new WkbReader(ms);

        Assert.Throws<WkbFormatException>(target.Read<Point>);
    }

    [Fact]
    public void ReadT_ThrowsExceptionIfWKBDoesNotRepresentSpecificGeometryType()
    {
        var target = new WkbReader(TestDataReader.Open("point-3DM.wkb"));
        Assert.Throws<WkbFormatException>(target.Read<LineString>);
    }

    [Fact]
    public void Parse_ThrowsExceptionIfWKBDoesNotRepresentGeometry()
    {
        var wkb = new byte[] { 12, 0, 0, 45, 78, 124, 36, 0 };

        Assert.Throws<WkbFormatException>(() => WkbReader.Parse(wkb));
    }

    [Fact]
    public void Parse_ReturnsNullForEmptyInput()
    {
        var wkb = Array.Empty<byte>();

        Assert.Null(WkbReader.Parse(wkb));
    }

    [Fact]
    public void Parse_ThrowsArgumentNullExceptionIfDataIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => WkbReader.Parse(null!));
    }

    [Fact]
    public void Parse_ReturnsParsedGeometry()
    {
        var wkt = "point m (-10.1 15.5 1000.5)";
        var expected = ParseWKT<Point>(wkt);
        var parsed = WkbReader.Parse<Point>(TestDataReader.Read("point-2DM.wkb"));

        AssertPointsEqual(parsed, expected);
    }

    [Fact]
    public void ParseT_ThrowsExceptionIfWKBDoesNotRepresentSpecifiedType()
    {
        var wkb = TestDataReader.Read("linestring-2D.wkb");

        Assert.Throws<WkbFormatException>(() => WkbReader.Parse<Point>(wkb));
    }

    [Fact]
    public void ParseT_ReturnsNullForEmptyInput()
    {
        var wkb = Array.Empty<byte>();

        Assert.Null(WkbReader.Parse<Geometry>(wkb));
    }

    [Fact]
    public void ParseT_ThrowsArgumentNullExceptionIfDataIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => WkbReader.Parse<Point>(null!));
    }

    [Fact]
    public void ParsePoint_Parses2DPoint()
    {
        var wkt = "point (-10.1 15.5)";
        byte[] wkb = TestDataReader.Read("point-2D.wkb");

        TestParsePoint(wkb, wkt);
    }

    [Fact]
    public void ParsePoint_Parses2DMeasuredPoint()
    {
        var wkt = "point m (-10.1 15.5 1000.5)";
        byte[] wkb = TestDataReader.Read("point-2DM.wkb");

        TestParsePoint(wkb, wkt);
    }

    [Fact]
    public void ParsePoint_Parses3DPoint()
    {
        var wkt = "point z (-10.1 15.5 100.5)";
        byte[] wkb = TestDataReader.Read("point-3D.wkb");

        TestParsePoint(wkb, wkt);
    }

    [Fact]
    public void ParsePoint_Parses3DMeasuredPoint()
    {
        var wkt = "point zm (-10.1 15.5 100.5 1000.5)";
        byte[] wkb = TestDataReader.Read("point-3DM.wkb");

        TestParsePoint(wkb, wkt);
    }

    [Fact]
    public void Parse_ParsesEmptyLineString()
    {
        var wkt = "linestring empty";
        byte[] wkb = TestDataReader.Read("linestring-empty.wkb");

        TestParseLineString(wkb, wkt);
    }

    [Fact]
    public void Parse_Parses2DLineString()
    {
        var wkt = "linestring (-10.1 15.5, 20.2 -25.5, 30.3 35.5)";
        byte[] wkb = TestDataReader.Read("linestring-2D.wkb");

        TestParseLineString(wkb, wkt);
    }

    [Fact]
    public void Parse_Parses2DMeasuredLineString()
    {
        var wkt = "linestring m (-10.1 15.5 1000.5, 20.2 -25.5 2000.5, 30.3 35.5 -3000.5)";
        byte[] wkb = TestDataReader.Read("linestring-2DM.wkb");

        TestParseLineString(wkb, wkt);
    }

    [Fact]
    public void Parse_Parses3DLineString()
    {
        var wkt = "linestring z (-10.1 15.5 100.5, 20.2 -25.5 200.5, 30.3 35.5 -300.5)";
        byte[] wkb = TestDataReader.Read("linestring-3D.wkb");
        TestParseLineString(wkb, wkt);
    }

    [Fact]
    public void Parse_Parses3DMeasuredLineString()
    {
        var wkt = "linestring zm (-10.1 15.5 100.5 1000.5, 20.2 -25.5 200.5 2000.5, 30.3 35.5 -300.5 -3000.5)";
        byte[] wkb = TestDataReader.Read("linestring-3DM.wkb");

        TestParseLineString(wkb, wkt);
    }

    [Fact]
    public void Parse_ParsesEmptyPolygon()
    {
        var wkt = "polygon empty";
        byte[] wkb = TestDataReader.Read("polygon-empty.wkb");

        TestParsePolygon(wkb, wkt);
    }

    [Fact]
    public void Parse_Parses2DPolygonOnlyExteriorRing()
    {
        var wkt = "polygon ((-10.1 15.5, 20.2 -25.5, 30.3 35.5))";
        byte[] wkb = TestDataReader.Read("polygon-ext-2D.wkb");

        TestParsePolygon(wkb, wkt);
    }

    [Fact]
    public void Parse_Parses2DMeasuredPolygonOnlyExteriorRing()
    {
        var wkt = "polygon m ((-10.1 15.5 1000.5, 20.2 -25.5 2000.5, 30.3 35.5 -3000.5))";
        byte[] wkb = TestDataReader.Read("polygon-ext-2DM.wkb");

        TestParsePolygon(wkb, wkt);
    }

    [Fact]
    public void Parse_Parses3DPolygonOnlyExteriorRing()
    {
        var wkt = "polygon z ((-10.1 15.5 100.5, 20.2 -25.5 200.5, 30.3 35.5 -300.5))";
        byte[] wkb = TestDataReader.Read("polygon-ext-3D.wkb");

        TestParsePolygon(wkb, wkt);
    }

    [Fact]
    public void Parse_Parses3DMeasuredPolygonOnlyExteriorRing()
    {
        var wkt = "polygon zm ((-10.1 15.5 100.5 1000.5, 20.2 -25.5 200.5 2000.5, 30.3 35.5 -300.5 -3000.5))";
        byte[] wkb = TestDataReader.Read("polygon-ext-3DM.wkb");

        TestParsePolygon(wkb, wkt);
    }

    [Fact]
    public void Parse_Parses3DMeasuredPolygon()
    {
        var wkt = "polygon zm ((-10.1 15.5 100.5 1000.5, 20.2 -25.5 200.5 2000.5, 30.3 35.5 -300.5 -3000.5),(-1.1 1.5 10.5 100.5, 2.2 -2.5 20.5 200.5, 3.3 3.5 -30.5 -300.5),(-1.1 1.5 10.5 100.5, 2.2 -2.5 20.5 200.5, 3.3 3.5 -30.5 -300.5))";
        byte[] wkb = TestDataReader.Read("polygon-3DM.wkb");

        TestParsePolygon(wkb, wkt);
    }

    [Fact]
    public void ParseMultiPoint_ParsesEmptyMultipoint()
    {
        var wkt = "multipoint empty";
        byte[] wkb = TestDataReader.Read("multipoint-empty.wkb");

        TestParseMultiPoint(wkb, wkt);
    }

    [Fact]
    public void ParseMultiPoint_Parses2DMultiPoint()
    {
        var wkt = "multipoint ((-10.1 15.5),(20.2 -25.5))";
        byte[] wkb = TestDataReader.Read("multipoint-2D.wkb");

        TestParseMultiPoint(wkb, wkt);
    }

    [Fact]
    public void ParseMultiPoint_Parses2DMeasuredMultiPoint()
    {
        var wkt = "multipoint m ((-10.1 15.5 1000.5),(20.2 -25.5 2000.5))";
        byte[] wkb = TestDataReader.Read("multipoint-2DM.wkb");

        TestParseMultiPoint(wkb, wkt);
    }

    [Fact]
    public void ParseMultiPoint_Parses3DMultiPoint()
    {
        var wkt = "multipoint z ((-10.1 15.5 100.5),(20.2 -25.5 200.5))";
        byte[] wkb = TestDataReader.Read("multipoint-3D.wkb");

        TestParseMultiPoint(wkb, wkt);
    }

    [Fact]
    public void ParseMultiPoint_Parses3DMeasuredMultiPoint()
    {
        var wkt = "multipoint zm ((-10.1 15.5 100.5 1000.5),(20.2 -25.5 200.5 2000.5))";
        byte[] wkb = TestDataReader.Read("multipoint-3DM.wkb");

        TestParseMultiPoint(wkb, wkt);
    }

    [Fact]
    public void ParseMultiLineString_ParsesEmptyMultiLineString()
    {
        var wkt = "multilinestring empty";
        byte[] wkb = TestDataReader.Read("multilinestring-empty.wkb");

        TestParseMultiLineString(wkb, wkt);
    }

    [Fact]
    public void ParseMultiLineString_Parses2DMultiLineString()
    {
        var wkt = "multilinestring ((-10.1 15.5, 20.2 -25.5, 30.3 35.5),(-10.1 15.5, 20.2 -25.5, 30.3 35.5))";
        byte[] wkb = TestDataReader.Read("multilinestring-2D.wkb");

        TestParseMultiLineString(wkb, wkt);
    }

    [Fact]
    public void ParseMultiLineString_Parses2DMeasuredMultiLineString()
    {
        var wkt = "multilinestring m ((-10.1 15.5 1000.5, 20.2 -25.5 2000.5, 30.3 35.5 -3000.5),(-10.1 15.5 1000.5, 20.2 -25.5 2000.5, 30.3 35.5 -3000.5))";
        byte[] wkb = TestDataReader.Read("multilinestring-2DM.wkb");

        TestParseMultiLineString(wkb, wkt);
    }

    [Fact]
    public void ParseMultiLineString_Parses3DMultiLineString()
    {
        var wkt = "multilinestring z ((-10.1 15.5 100.5, 20.2 -25.5 200.5, 30.3 35.5 -300.5),(-10.1 15.5 100.5, 20.2 -25.5 200.5, 30.3 35.5 -300.5))";
        byte[] wkb = TestDataReader.Read("multilinestring-3D.wkb");

        TestParseMultiLineString(wkb, wkt);
    }

    [Fact]
    public void ParseMultiLineString_Parses3DMeasuredMultiLineString()
    {
        var wkt = "multilinestring zm ((-10.1 15.5 100.5 1000.5, 20.2 -25.5 200.5 2000.5, 30.3 35.5 -300.5 -3000.5),(-10.1 15.5 100.5 1000.5, 20.2 -25.5 200.5 2000.5, 30.3 35.5 -300.5 -3000.5))";
        byte[] wkb = TestDataReader.Read("multilinestring-3DM.wkb");

        TestParseMultiLineString(wkb, wkt);
    }

    [Fact]
    public void ParseMultiPolygon_ParsesEmptyMultiPolygon()
    {
        var wkt = "multipolygon empty";
        byte[] wkb = TestDataReader.Read("multipolygon-empty.wkb");

        TestParseMultiPolygon(wkb, wkt);
    }

    [Fact]
    public void ParseMultiPolygon_Parses2DMultiPolygon()
    {
        var wkt = "multipolygon (((-10.1 15.5, 20.2 -25.5, 30.3 35.5)),((-10.1 15.5, 20.2 -25.5, 30.3 35.5)))";
        byte[] wkb = TestDataReader.Read("multipolygon-2D.wkb");

        TestParseMultiPolygon(wkb, wkt);
    }

    [Fact]
    public void ParseMultiPolygon_Parses2DMeasuredMultiPolygon()
    {
        var wkt = "multipolygon m (((-10.1 15.5 1000.5, 20.2 -25.5 2000.5, 30.3 35.5 -3000.5)),((-10.1 15.5 1000.5, 20.2 -25.5 2000.5, 30.3 35.5 -3000.5)))";
        byte[] wkb = TestDataReader.Read("multipolygon-2DM.wkb");

        TestParseMultiPolygon(wkb, wkt);
    }

    [Fact]
    public void ParseMultiPolygon_Parses3DMultiPolygon()
    {
        var wkt = "multipolygon z (((-10.1 15.5 100.5, 20.2 -25.5 200.5, 30.3 35.5 -300.5)),((-10.1 15.5 100.5, 20.2 -25.5 200.5, 30.3 35.5 -300.5)))";
        byte[] wkb = TestDataReader.Read("multipolygon-3D.wkb");

        TestParseMultiPolygon(wkb, wkt);
    }

    [Fact]
    public void ParseMultiPolygon_Parses3DMeasuredMultiPolygon()
    {
        var wkt = "multipolygon zm (((-10.1 15.5 100.5 1000.5, 20.2 -25.5 200.5 2000.5, 30.3 35.5 -300.5 -3000.5)),((-10.1 15.5 100.5 1000.5, 20.2 -25.5 200.5 2000.5, 30.3 35.5 -300.5 -3000.5)))";
        byte[] wkb = TestDataReader.Read("multipolygon-3DM.wkb");

        TestParseMultiPolygon(wkb, wkt);
    }

    [Fact]
    public void ParseGeometryCollection_ParsesEmptyGeometryCollection()
    {
        var parsed = WkbReader.Parse<GeometryCollection<Geometry>>(TestDataReader.Read("collection-empty.wkb"));

        Assert.NotNull(parsed);
        Assert.Empty(parsed.Geometries);
    }

    [Fact]
    public void ParseGeometryCollection_Parses2DGeometryCollection()
    {
        var wkt = "geometrycollection (point (-10.1 15.5))";
        var expected = ParseWKT<GeometryCollection<Geometry>>(wkt);
        var parsed = WkbReader.Parse<GeometryCollection<Geometry>>(TestDataReader.Read("collection-2D.wkb"));

        Assert.NotNull(parsed);
        Assert.Single(parsed.Geometries);
        AssertPointsEqual((Point)parsed.Geometries[0], (Point)expected!.Geometries[0]);
    }

    [Fact]
    public void ParseGeometryCollection_Parses2DMeasuredGeometryCollection()
    {
        var wkt = "geometrycollection m (point m (-10.1 15.5 1000.5))";
        var expected = ParseWKT<GeometryCollection<Geometry>>(wkt);
        var parsed = WkbReader.Parse<GeometryCollection<Geometry>>(TestDataReader.Read("collection-2DM.wkb"));

        Assert.NotNull(parsed);
        Assert.Single(parsed.Geometries);
        AssertPointsEqual((Point)parsed.Geometries[0], (Point)expected!.Geometries[0]);
    }

    [Fact]
    public void ParseGeometryCollection_Parses3DGeometryCollection()
    {
        var wkt = "geometrycollection z (point z (-10.1 15.5 100.5))";
        var expected = ParseWKT<GeometryCollection<Geometry>>(wkt);
        var parsed = WkbReader.Parse<GeometryCollection<Geometry>>(TestDataReader.Read("collection-3D.wkb"));

        Assert.NotNull(parsed);
        Assert.Single(parsed.Geometries);
        AssertPointsEqual((Point)parsed.Geometries[0], (Point)expected!.Geometries[0]);
    }

    [Fact]
    public void ParseGeometryCollection_Parses3DMeasuredGeometryCollection()
    {
        var wkt = "geometrycollection zm (point zm (-10.1 15.5 100.5 1000.5))";
        var expected = ParseWKT<GeometryCollection<Geometry>>(wkt);
        var parsed = WkbReader.Parse<GeometryCollection<Geometry>>(TestDataReader.Read("collection-3DM.wkb"));

        Assert.NotNull(parsed);
        Assert.Single(parsed.Geometries);
        AssertPointsEqual((Point)parsed.Geometries[0], (Point)expected!.Geometries[0]);
    }

    [Fact]
    public void ParseGeometryCollection_ParsesCollectionWithPointLineStringAndPolygon()
    {
        var wkt = "geometrycollection (point (-10.1 15.5),linestring (-10.1 15.5, 20.2 -25.5, 30.3 35.5),polygon ((-10.1 15.5, 20.2 -25.5, 30.3 35.5)))";
        var expected = ParseWKT<GeometryCollection<Geometry>>(wkt);
        var parsed = WkbReader.Parse<GeometryCollection<Geometry>>(TestDataReader.Read("collection-pt-ls-poly.wkb"));

        Assert.NotNull(parsed);
        Assert.Equal(3, parsed.Geometries.Count);

        AssertPointsEqual((Point)parsed.Geometries[0], (Point)expected.Geometries[0]);
        AssertLineStringsEqual((LineString)parsed.Geometries[1], (LineString)expected.Geometries[1]);
        AssertPolygonsEqual((Polygon)parsed.Geometries[2], (Polygon)expected.Geometries[2]);
    }

    [Fact]
    public void ParseGeometryCollection_ParsesCollectionWithMultiGeometries()
    {
        var wkt = "geometrycollection (multipoint empty,multilinestring empty,multipolygon empty)";
        var expected = ParseWKT<GeometryCollection<Geometry>>(wkt);
        var parsed = WkbReader.Parse<GeometryCollection<Geometry>>(TestDataReader.Read("collection-multi.wkb"));

        Assert.NotNull(parsed);
        Assert.Equal(3, parsed.Geometries.Count);

        AssertMultiPointsEqual((MultiPoint)parsed.Geometries[0], (MultiPoint)expected.Geometries[0]);
        AssertMultiLineStringsEqual((MultiLineString)parsed.Geometries[1], (MultiLineString)expected.Geometries[1]);
        AssertMultiPolygonsEqual((MultiPolygon)parsed.Geometries[2], (MultiPolygon)expected.Geometries[2]);
    }

    [Fact]
    public void ParseGeometryCollection_ParsesNestedCollection()
    {
        var wkt = "geometrycollection (geometrycollection (point (-10.1 15.5)))";
        var expected = ParseWKT<GeometryCollection<Geometry>>(wkt);
        var parsed = WkbReader.Parse<GeometryCollection<Geometry>>(TestDataReader.Read("collection-nested.wkb"));

        Assert.NotNull(parsed);
        Assert.Single(parsed.Geometries);
        Assert.Equal(((GeometryCollection<Geometry>)expected.Geometries[0]).Geometries.Count, ((GeometryCollection<Geometry>)parsed.Geometries[0]).Geometries.Count);
        AssertPointsEqual((Point)((GeometryCollection<Geometry>)parsed.Geometries[0]).Geometries[0], (Point)((GeometryCollection<Geometry>)expected.Geometries[0]).Geometries[0]);
    }

    private void TestParsePoint(byte[] wkb, string expectedAsWkt)
    {
        var expected = ParseWKT<Point>(expectedAsWkt);
        var parsed = WkbReader.Parse<Point>(wkb);

        AssertPointsEqual(parsed, expected);
    }

    private void TestParseLineString(byte[] wkb, string expectedAsWkt)
    {
        var expected = ParseWKT<LineString>(expectedAsWkt);
        var parsed = WkbReader.Parse<LineString>(wkb);

        Assert.NotNull(parsed);
        AssertLineStringsEqual(parsed, expected);
    }

    private void TestParsePolygon(byte[] wkb, string expectedAsWkt)
    {
        var expected = ParseWKT<Polygon>(expectedAsWkt);
        var parsed = WkbReader.Parse<Polygon>(wkb);

        Assert.NotNull(parsed);
        AssertPolygonsEqual(parsed, expected);
    }

    private void TestParseMultiPoint(byte[] wkb, string expectedAsWkt)
    {
        var expected = ParseWKT<MultiPoint>(expectedAsWkt);
        var parsed = WkbReader.Parse<MultiPoint>(wkb);

        Assert.NotNull(parsed);
        AssertMultiPointsEqual(parsed, expected);
    }

    private void TestParseMultiLineString(byte[] wkb, string expectedAsWkt)
    {
        var expected = ParseWKT<MultiLineString>(expectedAsWkt);
        var parsed = WkbReader.Parse<MultiLineString>(wkb);

        Assert.NotNull(parsed);
        AssertMultiLineStringsEqual(parsed, expected);
    }

    private void TestParseMultiPolygon(byte[] wkb, string expectedAsWkt)
    {
        var expected = ParseWKT<MultiPolygon>(expectedAsWkt);
        var parsed = WkbReader.Parse<MultiPolygon>(wkb);

        Assert.NotNull(parsed);
        AssertMultiPolygonsEqual(parsed, expected);
    }

    private static void AssertPointsEqual(Point? point, Point? expected)
    {
        Assert.NotNull(point);
        Assert.NotNull(expected);

        Assert.Equal(expected.Position, point.Position);
    }

    private static void AssertCoordinatesEqual(IReadOnlyList<Coordinate> list, IReadOnlyList<Coordinate> expected)
    {
        Assert.Equal(expected.Count, list.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i], list[i]);
        }
    }

    private static void AssertLineStringsEqual(LineString linestring, LineString expected)
    {
        AssertCoordinatesEqual(linestring.Coordinates, expected.Coordinates);
    }

    private static void AssertPolygonsEqual(Polygon polygon, Polygon expected)
    {
        AssertCoordinatesEqual(polygon.ExteriorRing, expected.ExteriorRing);

        Assert.Equal(expected.InteriorRings.Count, polygon.InteriorRings.Count);
        for (var i = 0; i < expected.InteriorRings.Count; i++)
        {
            AssertCoordinatesEqual(polygon.InteriorRings[i], expected.InteriorRings[i]);
        }
    }

    private static void AssertMultiPointsEqual(MultiPoint multipoint, MultiPoint expected)
    {
        Assert.Equal(expected.Geometries.Count, multipoint.Geometries.Count);

        for (var i = 0; i < expected.Geometries.Count; i++)
        {
            AssertPointsEqual(multipoint.Geometries[i], expected.Geometries[i]);
        }
    }

    private static void AssertMultiLineStringsEqual(MultiLineString multilinestring, MultiLineString expected)
    {
        Assert.Equal(expected.Geometries.Count, multilinestring.Geometries.Count);

        for (var i = 0; i < expected.Geometries.Count; i++)
        {
            AssertLineStringsEqual(multilinestring.Geometries[i], expected.Geometries[i]);
        }
    }

    private static void AssertMultiPolygonsEqual(MultiPolygon multipolygon, MultiPolygon expected)
    {
        Assert.Equal(expected.Geometries.Count, multipolygon.Geometries.Count);

        for (var i = 0; i < expected.Geometries.Count; i++)
        {
            AssertPolygonsEqual(multipolygon.Geometries[i], expected.Geometries[i]);
        }
    }

    private static T ParseWKT<T>(string wkt) where T : Geometry => WktReader.Parse<T>(wkt)!;
}
