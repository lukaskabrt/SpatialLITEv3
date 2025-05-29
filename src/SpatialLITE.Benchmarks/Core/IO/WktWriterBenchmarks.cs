using BenchmarkDotNet.Attributes;
using SpatialLite.Core.Geometries;
using SpatialLite.Core.IO;
using SpatialLITE.Contracts;

namespace SpatialLITE.Benchmarks.Core.IO;

[MemoryDiagnoser]
public class WktWriterBenchmarks
{
    private readonly Point _point;
    private readonly LineString _lineString;
    private readonly Polygon _polygon;
    private readonly MultiPoint _multiPoint;
    private readonly MultiLineString _multiLineString;
    private readonly MultiPolygon _multiPolygon;
    private readonly GeometryCollection<Geometry> _geometryCollection;
    private readonly Polygon _complexPolygon;

    public WktWriterBenchmarks()
    {
        var coordinates = new List<Coordinate>
        {
            new(-10.1, 15.5), new(20.2, -25.5), new(30.3, 35.5)
        };

        var complexCoordinates = new List<Coordinate>
        {
            new(-10.1, 15.5), new(20.2, -25.5), new(30.3, 35.5),
            new(40.4, -45.5), new(50.5, 55.5), new(60.6, -65.5),
            new(70.7, 75.5), new(80.8, -85.5), new(90.9, 95.5),
            new(-10.1, 15.5)
        };

        _point = new Point(coordinates[0]);
        _lineString = new LineString(coordinates);
        _polygon = new Polygon(coordinates);
        _multiPoint = new MultiPoint([new Point(coordinates[0]), new Point(coordinates[1])]);
        _multiLineString = new MultiLineString([new LineString(coordinates), new LineString(coordinates)]);
        _multiPolygon = new MultiPolygon([new Polygon(coordinates), new Polygon(coordinates)]);
        _complexPolygon = new Polygon(complexCoordinates);

        _geometryCollection = new GeometryCollection<Geometry>();
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
    public string WriteComplexPolygon()
    {
        return WktWriter.WriteToString(_complexPolygon);
    }
}