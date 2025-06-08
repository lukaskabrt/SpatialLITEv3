using BenchmarkDotNet.Attributes;
using SpatialLite.Contracts;
using SpatialLite.Core.Geometries;
using SpatialLite.Core.IO;

namespace SpatialLite.Benchmarks.Core.IO;

[MemoryDiagnoser]
public class WktWriterBenchmarks
{
    private readonly Point _point;
    private readonly LineString _lineString;
    private readonly Polygon _polygon;
    private readonly MultiPoint _multiPoint;
    private readonly MultiLineString _multiLineString;
    private readonly MultiPolygon _multiPolygon;
    private readonly GeometryCollection<IGeometry> _geometryCollection;
    private readonly Polygon _polygonWithInnerRing;

    public WktWriterBenchmarks()
    {
        var coordinates = new List<Coordinate>
        {
            new(-10.1, 15.5), new(20.2, -25.5), new(30.3, 35.5)
        };

        var exteriorRing = new List<Coordinate>
        {
            new(-10.1, 15.5), new(20.2, -25.5), new(30.3, 35.5), new(-10.1, 15.5)
        };

        var innerRing = new List<Coordinate>
        {
            new(-5.0, 5.0), new(10.0, -10.0), new(15.0, 15.0), new(-5.0, 5.0)
        };

        _point = new Point(coordinates[0]);
        _lineString = new LineString(coordinates);
        _polygon = new Polygon(coordinates);
        _multiPoint = new MultiPoint([new Point(coordinates[0]), new Point(coordinates[1])]);
        _multiLineString = new MultiLineString([new LineString(coordinates), new LineString(coordinates)]);
        _multiPolygon = new MultiPolygon([new Polygon(coordinates), new Polygon(coordinates)]);

        _polygonWithInnerRing = new Polygon(exteriorRing);
        _polygonWithInnerRing.InteriorRings.Add(innerRing);

        _geometryCollection = new GeometryCollection<IGeometry>();
        _geometryCollection.Geometries.Add(_point);
        _geometryCollection.Geometries.Add(_lineString);
        _geometryCollection.Geometries.Add(_polygon);
    }

    [Benchmark]
    public string WritePoint()
    {
        return WktWriter.WriteToString(_point);
    }

    [Benchmark]
    public string WriteLineString()
    {
        return WktWriter.WriteToString(_lineString);
    }

    [Benchmark]
    public string WritePolygon()
    {
        return WktWriter.WriteToString(_polygon);
    }

    [Benchmark]
    public string WriteMultiPoint()
    {
        return WktWriter.WriteToString(_multiPoint);
    }

    [Benchmark]
    public string WriteMultiLineString()
    {
        return WktWriter.WriteToString(_multiLineString);
    }

    [Benchmark]
    public string WriteMultiPolygon()
    {
        return WktWriter.WriteToString(_multiPolygon);
    }

    [Benchmark]
    public string WriteGeometryCollection()
    {
        return WktWriter.WriteToString(_geometryCollection);
    }

    [Benchmark]
    public string WritePolygonWithInnerRing()
    {
        return WktWriter.WriteToString(_polygonWithInnerRing);
    }
}
