using SpatialLite.Contracts;
using SpatialLite.Core.Geometries;
using SpatialLite.Core.IO;
using SpatialLite.UnitTests.Data;

namespace SpatialLite.UnitTests.Core.IO;

public class WktReaderTests
{

    private readonly Coordinate[] _coordinatesXY = [
        new(-10.1, 15.5),
        new(20.2, -25.5),
        new(30.3, 35.5)
    ];

    private readonly Coordinate[] _coordinatesXY2 = [
        new(-1.1, 1.5),
        new(2.2, -2.5),
        new(3.3, 3.5)
    ];

    [Fact]
    public void Constructor_Stream_ThrowsArgumentNullExceptionIfStreamIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new WktReader((Stream)null!));
    }

    [Fact]
    public void Constructor_Path_ThrowsFileNotFoundExceptionIfFileDoesNotExists()
    {
        Assert.Throws<FileNotFoundException>(() => new WktReader("non-existing-file.wkt"));
    }

    public static IEnumerable<object[]> Read_ReadsAllGeometryTypesTestData
    {
        get
        {
            yield return new object[] { TestDataReader.CoreIO.Read("wkt-point-3DM.wkt") };
            yield return new object[] { TestDataReader.CoreIO.Read("wkt-linestring-3DM.wkt") };
            yield return new object[] { TestDataReader.CoreIO.Read("wkt-polygon-3DM.wkt") };
            yield return new object[] { TestDataReader.CoreIO.Read("wkt-multipoint-3DM.wkt") };
            yield return new object[] { TestDataReader.CoreIO.Read("wkt-multilinestring-3DM.wkt") };
            yield return new object[] { TestDataReader.CoreIO.Read("wkt-multipolygon-3DM.wkt") };
            yield return new object[] { TestDataReader.CoreIO.Read("wkt-geometry-collection-3DM.wkt") };
        }
    }

    [Theory]
    [MemberData(nameof(Read_ReadsAllGeometryTypesTestData))]
    public void Read_ReadsAllGeometryTypes(byte[] data)
    {
        using var target = new WktReader(new MemoryStream(data));
        var readGeometry = target.Read();
        Assert.NotNull(readGeometry);
    }

    [Fact]
    public void Read_ReturnsNullIfStreamIsEmpty()
    {
        using var target = new WktReader(new MemoryStream());
        var readGeometry = target.Read();

        Assert.Null(readGeometry);
    }

    [Fact]
    public void Read_ReturnsNullIfNoMoreGeometriesAreAvailableInInputStream()
    {
        using var target = new WktReader(TestDataReader.CoreIO.Open("wkt-point-3DM.wkt"));
        target.Read();
        var readGeometry = target.Read();

        Assert.Null(readGeometry);
    }

    [Fact]
    public void Read_ReadsMultipleGeometries()
    {
        using var target = new WktReader(TestDataReader.CoreIO.Open("wkt-point-and-linestring-3DM.wkt"));
        var readGeometry = target.Read();
        Assert.True(readGeometry is Point);

        readGeometry = target.Read();
        Assert.True(readGeometry is LineString);
    }

    [Fact]
    public void ReadT_ReadsGeometry()
    {
        using var target = new WktReader(TestDataReader.CoreIO.Open("wkt-point-3DM.wkt"));
        var read = target.Read<Point>();
        Assert.NotNull(read);
    }

    [Fact]
    public void ReadT_ReturnsNullIfStreamIsEmpty()
    {
        using var target = new WktReader(new MemoryStream());
        var readGeometry = target.Read<Point>();

        Assert.Null(readGeometry);
    }

    [Fact]
    public void ReadT_ThrowsExceptionIfWKTDoesNotRepresentGeometryOfSpecificType()
    {
        using var target = new WktReader(TestDataReader.CoreIO.Open("wkt-point-3DM.wkt"));
        Assert.Throws<WktParseException>(target.Read<LineString>);
    }

    [Fact]
    public void Parse_ParsesPoint()
    {
        var wkt = "point empty";

        var parsed = (Point?)WktReader.Parse(wkt);

        Assert.NotNull(parsed);
        CompareCoordinate(Coordinate.Empty, parsed.Position);
    }

    [Fact]
    public void Parse_ParsesLineString()
    {
        var wkt = "linestring empty";

        var parsed = (LineString?)WktReader.Parse(wkt);

        Assert.NotNull(parsed);
        Assert.Empty(parsed.Coordinates);
    }

    [Fact]
    public void Parse_ParsesPolygon()
    {
        var wkt = "polygon empty";

        var parsed = (Polygon?)WktReader.Parse(wkt);

        Assert.NotNull(parsed);
        Assert.Empty(parsed.ExteriorRing);
    }

    [Fact]
    public void Parse_ParsesGeometryCollection()
    {
        var wkt = "geometrycollection empty";

        var parsed = (GeometryCollection<Geometry>?)WktReader.Parse(wkt);

        Assert.NotNull(parsed);
        Assert.Empty(parsed.Geometries);
    }

    [Fact]
    public void Parse_ParsesMultiPoint()
    {
        var wkt = "multipoint empty";

        var parsed = (MultiPoint?)WktReader.Parse(wkt);

        Assert.NotNull(parsed);
        Assert.Empty(parsed.Geometries);
    }

    [Fact]
    public void Parse_ParsesMultiLineString()
    {
        var wkt = "multilinestring empty";

        var parsed = (MultiLineString?)WktReader.Parse(wkt);

        Assert.NotNull(parsed);
        Assert.Empty(parsed.Geometries);
    }

    [Fact]
    public void Parse_ParsesMultiPolygon()
    {
        var wkt = "multipolygon empty";

        var parsed = (MultiPolygon?)WktReader.Parse(wkt);

        Assert.NotNull(parsed);
        Assert.Empty(parsed.Geometries);
    }

    [Fact]
    public void Parse_ThrowsExceptionIfWktDoNotRepresentGeometry()
    {
        var wkt = "invalid string";

        Assert.Throws<WktParseException>(() => WktReader.Parse(wkt));
    }

    [Theory]
    [InlineData("point  zm \t(-10.1 15.5 100.5 \n1000.5)")]
    [InlineData("\n\rlinestring (-10.1 15.5, 20.2 -25.5, \t30.3  35.5)")]
    [InlineData("polygon\n\r m\t ((-10.1 15.5 1000.5,    20.2 -25.5  2000.5, 30.3 35.5 -3000.5))")]
    [InlineData("multipoint\t\r z ((-10.1 15.5 100.5),(20.2 -25.5 200.5))")]
    [InlineData("\nmultilinestring \tempty")]
    [InlineData("multipolygon\n\r (((-10.1\t 15.5,  20.2 -25.5,  30.3 35.5)),((-10.1  15.5, 20.2 -25.5, 30.3 35.5)))")]
    [InlineData("geometrycollection \t\n\r(point  (-10.1  15.5))")]
    public void Parse_ParsesGeometriesWithMultipleWhitespacesInsteadOneSpace(string wkt)
    {
        WktReader.Parse(wkt);
    }

    [Theory]
    [InlineData("point  zm ( -10.1 15.5 100.5 1000.5 )")]
    [InlineData("linestring ( -10.1 15.5 , 20.2 -25.5 , 30.3  35.5 )")]
    [InlineData("polygon m ( ( -10.1 15.5 1000.5 , 20.2 -25.5 2000.5,  30.3 35.5 -3000.5 ) )")]
    [InlineData("multipoint z ( ( -10.1 15.5 100.5 ) , ( 20.2 -25.5 200.5 ) )")]
    [InlineData("multilinestring ( ( -10.1 15.5 , 20.2 -25.5 , 30.3 35.5 ) , ( -10.1 15.5, 20.2 -25.5, 30.3 35.5 ) ) ")]
    [InlineData("multipolygon ( ( ( -10.1 15.5 , 20.2 -25.5 ,  30.3 35.5 ) ) , ( ( -10.1  15.5 , 20.2 -25.5 , 30.3 35.5 ) ) )")]
    [InlineData("geometrycollection ( point ( -10.1  15.5 ) )")]
    public void Parse_ParsesGeometriesWithWhitespacesAtPlacesWhereTheyAreNotExpected(string wkt)
    {
        WktReader.Parse(wkt);
    }

    [Fact]
    public void Parse_ParsesEmptyPoint()
    {
        var wkt = "point empty";

        var parsed = WktReader.Parse<Point>(wkt);

        Assert.NotNull(parsed);
        CompareCoordinate(Coordinate.Empty, parsed.Position);
    }

    [Fact]
    public void Parse_Parses2DPoint()
    {
        var wkt = "point (-10.1 15.5)";

        var parsed = WktReader.Parse<Point>(wkt);

        Assert.NotNull(parsed);
        CompareCoordinate(_coordinatesXY[0], parsed.Position);
    }

    [Fact]
    public void Parse_Parses2DMeasuredPoint()
    {
        var wkt = "point m (-10.1 15.5 1000.5)";

        var parsed = WktReader.Parse<Point>(wkt);

        Assert.NotNull(parsed);
        CompareCoordinate(_coordinatesXY[0], parsed.Position);
    }

    [Fact]
    public void Parse_Parses3DPoint()
    {
        var wkt = "point z (-10.1 15.5 100.5)";

        var parsed = WktReader.Parse<Point>(wkt);

        Assert.NotNull(parsed);
        CompareCoordinate(_coordinatesXY[0], parsed.Position);
    }

    [Fact]
    public void Parse_Parses3DMeasuredPoint()
    {
        var wkt = "point zm (-10.1 15.5 100.5 1000.5)";

        var parsed = WktReader.Parse<Point>(wkt);

        Assert.NotNull(parsed);
        CompareCoordinate(_coordinatesXY[0], parsed.Position);
    }

    [Fact]
    public void Parse_ThrowsExceptionIfWktDoNotRepresentPoint()
    {
        var wkt = "linestring empty";

        Assert.Throws<WktParseException>(() => WktReader.Parse<Point>(wkt));
    }

    [Fact]
    public void Parse_ParsesEmptyLineString()
    {
        var wkt = "linestring empty";

        var parsed = WktReader.Parse<LineString>(wkt);

        Assert.NotNull(parsed);
        Assert.Empty(parsed.Coordinates);
    }

    [Fact]
    public void Parse_Parses2DLineString()
    {
        var wkt = "linestring (-10.1 15.5, 20.2 -25.5, 30.3 35.5)";

        var parsed = WktReader.Parse<LineString>(wkt);

        Assert.NotNull(parsed);
        CompareCoordinates(_coordinatesXY, parsed.Coordinates);
    }

    [Fact]
    public void Parse_Parses2DMeasuredLineString()
    {
        var wkt = "linestring m (-10.1 15.5 1000.5, 20.2 -25.5 2000.5, 30.3 35.5 -3000.5)";

        var parsed = WktReader.Parse<LineString>(wkt);

        Assert.NotNull(parsed);
        CompareCoordinates(_coordinatesXY, parsed.Coordinates);
    }

    [Fact]
    public void Parse_Parses3DLineString()
    {
        var wkt = "linestring z (-10.1 15.5 100.5, 20.2 -25.5 200.5, 30.3 35.5 -300.5)";

        var parsed = WktReader.Parse<LineString>(wkt);

        Assert.NotNull(parsed);
        CompareCoordinates(_coordinatesXY, parsed.Coordinates);
    }

    [Fact]
    public void Parse_Parses3DMeasuredLineString()
    {
        var wkt = "linestring zm (-10.1 15.5 100.5 1000.5, 20.2 -25.5 200.5 2000.5, 30.3 35.5 -300.5 -3000.5)";

        var parsed = WktReader.Parse<LineString>(wkt);

        Assert.NotNull(parsed);
        CompareCoordinates(_coordinatesXY, parsed.Coordinates);
    }

    [Fact]
    public void Parse_ThrowsExceptionIfWktDoNotRepresentLineString()
    {
        var wkt = "point empty";

        Assert.Throws<WktParseException>(() => WktReader.Parse<LineString>(wkt));
    }

    [Fact]
    public void ParsePolygon_ParsesEmptyPolygon()
    {
        var wkt = "polygon empty";

        var parsed = WktReader.Parse<Polygon>(wkt);

        Assert.NotNull(parsed);
        Assert.Empty(parsed.ExteriorRing);
        Assert.Empty(parsed.InteriorRings);
    }

    [Fact]
    public void ParsePolygon_Parses2DPolygonOnlyExteriorRing()
    {
        var wkt = "polygon ((-10.1 15.5, 20.2 -25.5, 30.3 35.5))";

        var parsed = WktReader.Parse<Polygon>(wkt);

        Assert.NotNull(parsed);
        CompareCoordinates(_coordinatesXY, parsed.ExteriorRing);
        Assert.Empty(parsed.InteriorRings);
    }

    [Fact]
    public void ParsePolygon_Parses3DPolygonOnlyExteriorRing()
    {
        var wkt = "polygon z ((-10.1 15.5 100.5, 20.2 -25.5 200.5, 30.3 35.5 -300.5))";

        var parsed = WktReader.Parse<Polygon>(wkt);

        Assert.NotNull(parsed);
        CompareCoordinates(_coordinatesXY, parsed.ExteriorRing);
        Assert.Empty(parsed.InteriorRings);
    }

    [Fact]
    public void ParsePolygon_Parses2DMeasuredPolygonOnlyExteriorRing()
    {
        var wkt = "polygon m ((-10.1 15.5 1000.5, 20.2 -25.5 2000.5, 30.3 35.5 -3000.5))";

        var parsed = WktReader.Parse<Polygon>(wkt);

        Assert.NotNull(parsed);
        CompareCoordinates(_coordinatesXY, parsed.ExteriorRing);
        Assert.Empty(parsed.InteriorRings);
    }

    [Fact]
    public void ParsePolygon_Parses3DMeasuredPolygonOnlyExteriorRing()
    {
        var wkt = "polygon zm ((-10.1 15.5 100.5 1000.5, 20.2 -25.5 200.5 2000.5, 30.3 35.5 -300.5 -3000.5))";

        var parsed = WktReader.Parse<Polygon>(wkt);

        Assert.NotNull(parsed);
        CompareCoordinates(_coordinatesXY, parsed.ExteriorRing);
        Assert.Empty(parsed.InteriorRings);
    }

    [Fact]
    public void ParsePolygon_Parses3DMeasuredPolygonWithInteriorRings()
    {
        var wkt = "polygon zm ((-10.1 15.5 100.5 1000.5, 20.2 -25.5 200.5 2000.5, 30.3 35.5 -300.5 -3000.5),(-1.1 1.5 10.5 100.5, 2.2 -2.5 20.5 200.5, 3.3 3.5 -30.5 -300.5),(-1.1 1.5 10.5 100.5, 2.2 -2.5 20.5 200.5, 3.3 3.5 -30.5 -300.5))";

        var parsed = WktReader.Parse<Polygon>(wkt);

        Assert.NotNull(parsed);
        CompareCoordinates(_coordinatesXY, parsed.ExteriorRing);
        Assert.Equal(2, parsed.InteriorRings.Count);
        CompareCoordinates(_coordinatesXY2, parsed.InteriorRings[0]);
        CompareCoordinates(_coordinatesXY2, parsed.InteriorRings[1]);
    }

    [Fact]
    public void ParsePolygon_ThrowsExceptionIfWktDoNotRepresentPolygon()
    {
        var wkt = "point empty";

        Assert.Throws<WktParseException>(() => WktReader.Parse<Polygon>(wkt));
    }

    [Fact]
    public void Parse_ParsesEmptyMultiPoint()
    {
        var wkt = "multipoint empty";

        var parsed = WktReader.Parse<MultiPoint>(wkt);

        Assert.NotNull(parsed);
        Assert.Empty(parsed.Geometries);
    }

    [Fact]
    public void Parse_Parses2DMultiPoint()
    {
        var wkt = "multipoint ((-10.1 15.5),(20.2 -25.5))";

        var parsed = WktReader.Parse<MultiPoint>(wkt);

        Assert.NotNull(parsed);
        Assert.Equal(2, parsed.Geometries.Count);
        CompareCoordinate(_coordinatesXY[0], parsed.Geometries[0].Position);
        CompareCoordinate(_coordinatesXY[1], parsed.Geometries[1].Position);
    }

    [Fact]
    public void Parse_Parses2DMeasuredMultiPoint()
    {
        var wkt = "multipoint m ((-10.1 15.5 1000.5),(20.2 -25.5 2000.5))";

        var parsed = WktReader.Parse<MultiPoint>(wkt);

        Assert.NotNull(parsed);
        Assert.Equal(2, parsed.Geometries.Count);
        CompareCoordinate(_coordinatesXY[0], parsed.Geometries[0].Position);
        CompareCoordinate(_coordinatesXY[1], parsed.Geometries[1].Position);
    }

    [Fact]
    public void Parse_Parses3DMultiPoint()
    {
        var wkt = "multipoint z ((-10.1 15.5 100.5),(20.2 -25.5 200.5))";

        var parsed = WktReader.Parse<MultiPoint>(wkt);

        Assert.NotNull(parsed);
        Assert.Equal(2, parsed.Geometries.Count);
        CompareCoordinate(_coordinatesXY[0], parsed.Geometries[0].Position);
        CompareCoordinate(_coordinatesXY[1], parsed.Geometries[1].Position);
    }

    [Fact]
    public void Parse_Parses3DMeasuredMultiPoint()
    {
        var wkt = "multipoint zm ((-10.1 15.5 100.5 1000.5),(20.2 -25.5 200.5 2000.5))";

        var parsed = WktReader.Parse<MultiPoint>(wkt);

        Assert.NotNull(parsed);
        Assert.Equal(2, parsed.Geometries.Count);
        CompareCoordinate(_coordinatesXY[0], parsed.Geometries[0].Position);
        CompareCoordinate(_coordinatesXY[1], parsed.Geometries[1].Position);
    }

    [Fact]
    public void Parse_ThrowsExceptionIfWktDoNotRepresentMultiPoint()
    {
        var wkt = "point empty";

        Assert.Throws<WktParseException>(() => WktReader.Parse<MultiPoint>(wkt));
    }

    [Fact]
    public void Parse_ParsesEmptyMultiLineString()
    {
        var wkt = "multilinestring empty";

        var parsed = WktReader.Parse<MultiLineString>(wkt);

        Assert.NotNull(parsed);
        Assert.Empty(parsed.Geometries);
    }

    [Fact]
    public void Parse_Parses2DMultiLineString()
    {
        var wkt = "multilinestring ((-10.1 15.5, 20.2 -25.5, 30.3 35.5),(-10.1 15.5, 20.2 -25.5, 30.3 35.5))";

        var parsed = WktReader.Parse<MultiLineString>(wkt);

        Assert.NotNull(parsed);
        Assert.Equal(2, parsed.Geometries.Count);
        CompareCoordinates(_coordinatesXY, parsed.Geometries[0].Coordinates);
        CompareCoordinates(_coordinatesXY, parsed.Geometries[1].Coordinates);
    }

    [Fact]
    public void Parse_Parses2DMeasuredMultiLineString()
    {
        var wkt = "multilinestring m ((-10.1 15.5 1000.5, 20.2 -25.5 2000.5, 30.3 35.5 -3000.5),(-10.1 15.5 1000.5, 20.2 -25.5 2000.5, 30.3 35.5 -3000.5))";

        var parsed = WktReader.Parse<MultiLineString>(wkt);

        Assert.NotNull(parsed);
        Assert.Equal(2, parsed.Geometries.Count);
        CompareCoordinates(_coordinatesXY, parsed.Geometries[0].Coordinates);
        CompareCoordinates(_coordinatesXY, parsed.Geometries[1].Coordinates);
    }

    [Fact]
    public void Parse_Parses3DMultiLineString()
    {
        var wkt = "multilinestring z ((-10.1 15.5 100.5, 20.2 -25.5 200.5, 30.3 35.5 -300.5),(-10.1 15.5 100.5, 20.2 -25.5 200.5, 30.3 35.5 -300.5))";

        var parsed = WktReader.Parse<MultiLineString>(wkt);

        Assert.NotNull(parsed);
        Assert.Equal(2, parsed.Geometries.Count);
        CompareCoordinates(_coordinatesXY, parsed.Geometries[0].Coordinates);
        CompareCoordinates(_coordinatesXY, parsed.Geometries[1].Coordinates);
    }

    [Fact]
    public void Parse_Parses3DMeasuredMultiLineString()
    {
        var wkt = "multilinestring zm ((-10.1 15.5 100.5 1000.5, 20.2 -25.5 200.5 2000.5, 30.3 35.5 -300.5 -3000.5),(-10.1 15.5 100.5 1000.5, 20.2 -25.5 200.5 2000.5, 30.3 35.5 -300.5 -3000.5))";

        var parsed = WktReader.Parse<MultiLineString>(wkt);

        Assert.NotNull(parsed);
        Assert.Equal(2, parsed.Geometries.Count);
        CompareCoordinates(_coordinatesXY, parsed.Geometries[0].Coordinates);
        CompareCoordinates(_coordinatesXY, parsed.Geometries[1].Coordinates);
    }

    [Fact]
    public void Parse_ThrowsExceptionIfWktDoNotRepresentMultiLineString()
    {
        var wkt = "point empty";

        Assert.Throws<WktParseException>(() => WktReader.Parse<MultiLineString>(wkt));
    }

    [Fact]
    public void Parse_ParsesEmptyMultiPolygon()
    {
        var wkt = "multipolygon empty";

        var parsed = WktReader.Parse<MultiPolygon>(wkt);

        Assert.NotNull(parsed);
        Assert.Empty(parsed.Geometries);
    }

    [Fact]
    public void Parse_Parses2DMultiPolygon()
    {
        var wkt = "multipolygon (((-10.1 15.5, 20.2 -25.5, 30.3 35.5)),((-10.1 15.5, 20.2 -25.5, 30.3 35.5)))";

        var parsed = WktReader.Parse<MultiPolygon>(wkt);

        Assert.NotNull(parsed);
        Assert.Equal(2, parsed.Geometries.Count);
        CompareCoordinates(_coordinatesXY, parsed.Geometries[0].ExteriorRing);
        CompareCoordinates(_coordinatesXY, parsed.Geometries[1].ExteriorRing);
    }

    [Fact]
    public void Parse_Parses2DMeasuredMultiPolygon()
    {
        var wkt = "multipolygon m (((-10.1 15.5 1000.5, 20.2 -25.5 2000.5, 30.3 35.5 -3000.5)),((-10.1 15.5 1000.5, 20.2 -25.5 2000.5, 30.3 35.5 -3000.5)))";

        var parsed = WktReader.Parse<MultiPolygon>(wkt);

        Assert.NotNull(parsed);
        Assert.Equal(2, parsed.Geometries.Count);
        CompareCoordinates(_coordinatesXY, parsed.Geometries[0].ExteriorRing);
        CompareCoordinates(_coordinatesXY, parsed.Geometries[1].ExteriorRing);
    }

    [Fact]
    public void Parse_Parses3DMultiPolygon()
    {
        var wkt = "multipolygon z (((-10.1 15.5 100.5, 20.2 -25.5 200.5, 30.3 35.5 -300.5)),((-10.1 15.5 100.5, 20.2 -25.5 200.5, 30.3 35.5 -300.5)))";

        var parsed = WktReader.Parse<MultiPolygon>(wkt);

        Assert.NotNull(parsed);
        Assert.Equal(2, parsed.Geometries.Count);
        CompareCoordinates(_coordinatesXY, parsed.Geometries[0].ExteriorRing);
        CompareCoordinates(_coordinatesXY, parsed.Geometries[1].ExteriorRing);
    }

    [Fact]
    public void Parse_Parses3DMeasuredMultiPolygon()
    {
        var wkt = "multipolygon zm (((-10.1 15.5 100.5 1000.5, 20.2 -25.5 200.5 2000.5, 30.3 35.5 -300.5 -3000.5)),((-10.1 15.5 100.5 1000.5, 20.2 -25.5 200.5 2000.5, 30.3 35.5 -300.5 -3000.5)))";

        var parsed = WktReader.Parse<MultiPolygon>(wkt);

        Assert.NotNull(parsed);
        Assert.Equal(2, parsed.Geometries.Count);
        CompareCoordinates(_coordinatesXY, parsed.Geometries[0].ExteriorRing);
        CompareCoordinates(_coordinatesXY, parsed.Geometries[1].ExteriorRing);
    }

    [Fact]
    public void Parse_ThrowsExceptionIfWktDoNotRepresentMultiPolygon()
    {
        var wkt = "point empty";

        Assert.Throws<WktParseException>(() => WktReader.Parse<MultiPolygon>(wkt));
    }

    [Fact]
    public void Parse_ParsesEmptyGeometryCollection()
    {
        var wkt = "geometrycollection empty";

        var parsed = WktReader.Parse<GeometryCollection<Geometry>>(wkt);

        Assert.NotNull(parsed);
        Assert.Empty(parsed.Geometries);
    }

    [Fact]
    public void Parse_Parses2DGeometryCollectionWithPoint()
    {
        var wkt = "geometrycollection (point (-10.1 15.5))";

        var parsed = WktReader.Parse<GeometryCollection<Geometry>>(wkt);

        Assert.NotNull(parsed);
        Assert.Single(parsed.Geometries);
        CompareCoordinate(_coordinatesXY[0], ((Point)parsed.Geometries[0]).Position);
    }

    [Fact]
    public void Parse_Parses2DMeasuredGeometryCollectionWithPoint()
    {
        var wkt = "geometrycollection m (point m (-10.1 15.5 1000.5))";

        var parsed = WktReader.Parse<GeometryCollection<Geometry>>(wkt);

        Assert.NotNull(parsed);
        Assert.Single(parsed.Geometries);
        CompareCoordinate(_coordinatesXY[0], ((Point)parsed.Geometries[0]).Position);
    }

    [Fact]
    public void Parse_Parses3DGeometryCollectionWithPoint()
    {
        var wkt = "geometrycollection z (point z (-10.1 15.5 100.5))";

        var parsed = WktReader.Parse<GeometryCollection<Geometry>>(wkt);

        Assert.NotNull(parsed);
        Assert.Single(parsed.Geometries);
        CompareCoordinate(_coordinatesXY[0], ((Point)parsed.Geometries[0]).Position);
    }

    [Fact]
    public void Parse_Parses3DMeasuredGeometryCollectionWithPoint()
    {
        var wkt = "geometrycollection zm (point zm (-10.1 15.5 100.5 1000.5))";

        var parsed = WktReader.Parse<GeometryCollection<Geometry>>(wkt);

        Assert.NotNull(parsed);
        Assert.Single(parsed.Geometries);
        CompareCoordinate(_coordinatesXY[0], ((Point)parsed.Geometries[0]).Position);
    }

    [Fact]
    public void Parse_ParsesCollectionWithPointLineStringAndPolygon()
    {
        var wkt = "geometrycollection (point (-10.1 15.5),linestring (-10.1 15.5, 20.2 -25.5, 30.3 35.5),polygon ((-10.1 15.5, 20.2 -25.5, 30.3 35.5)))";

        var parsed = WktReader.Parse<GeometryCollection<Geometry>>(wkt);

        Assert.NotNull(parsed);
        Assert.Equal(3, parsed.Geometries.Count);
        CompareCoordinate(_coordinatesXY[0], ((Point)parsed.Geometries[0]).Position);
        CompareCoordinates(_coordinatesXY, ((LineString)parsed.Geometries[1]).Coordinates);
        CompareCoordinates(_coordinatesXY, ((Polygon)parsed.Geometries[2]).ExteriorRing);
    }

    [Fact]
    public void Parse_ParsesNestedCollections()
    {
        var wkt = "geometrycollection (geometrycollection (point (-10.1 15.5)))";

        var parsed = WktReader.Parse<GeometryCollection<Geometry>>(wkt);

        Assert.NotNull(parsed);
        Assert.Single(parsed.Geometries);
        var nested = (GeometryCollection<Geometry>)parsed.Geometries[0];
        CompareCoordinate(_coordinatesXY[0], ((Point)nested.Geometries[0]).Position);
    }

    [Fact]
    public void Parse_ThrowsExceptionIfWktDoNotRepresentGeometryCollection()
    {
        var wkt = "point empty";

        Assert.Throws<WktParseException>(() => WktReader.Parse<GeometryCollection<Geometry>>(wkt));
    }

    private void CompareCoordinate(Coordinate expected, Coordinate actual)
    {
        Assert.Equal(expected, actual);
    }

    private void CompareCoordinates(Coordinate[] expected, IReadOnlyList<Coordinate> actual)
    {
        Assert.Equal(expected.Length, actual.Count);

        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], actual[i]);
        }
    }
}
