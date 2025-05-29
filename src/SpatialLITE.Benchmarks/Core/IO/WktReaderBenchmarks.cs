using BenchmarkDotNet.Attributes;
using SpatialLite.Core.Geometries;
using SpatialLite.Core.IO;

namespace SpatialLITE.Benchmarks.Core.IO;

[MemoryDiagnoser]
public class WktReaderBenchmarks
{
    private readonly string _pointWkt = "point (-10.1 15.5)";
    private readonly string _lineStringWkt = "linestring (-10.1 15.5, 20.2 -25.5, 30.3 35.5)";
    private readonly string _polygonWkt = "polygon ((-10.1 15.5, 20.2 -25.5, 30.3 35.5))";
    private readonly string _multiPointWkt = "multipoint ((-10.1 15.5),(20.2 -25.5))";
    private readonly string _multiLineStringWkt = "multilinestring ((-10.1 15.5, 20.2 -25.5, 30.3 35.5),(-10.1 15.5, 20.2 -25.5, 30.3 35.5))";
    private readonly string _multiPolygonWkt = "multipolygon (((-10.1 15.5, 20.2 -25.5, 30.3 35.5)),((-10.1 15.5, 20.2 -25.5, 30.3 35.5)))";
    private readonly string _geometryCollectionWkt = "geometrycollection (point (-10.1 15.5),linestring (-10.1 15.5, 20.2 -25.5, 30.3 35.5),polygon ((-10.1 15.5, 20.2 -25.5, 30.3 35.5)))";
    private readonly string _polygonWithInnerRingWkt = "polygon ((-10.1 15.5, 20.2 -25.5, 30.3 35.5, -10.1 15.5), (-5.0 5.0, 10.0 -10.0, 15.0 15.0, -5.0 5.0))";

    [Benchmark]
    public Point? ParsePoint()
    {
        return WktReader.Parse<Point>(_pointWkt);
    }

    [Benchmark]
    public LineString? ParseLineString()
    {
        return WktReader.Parse<LineString>(_lineStringWkt);
    }

    [Benchmark]
    public Polygon? ParsePolygon()
    {
        return WktReader.Parse<Polygon>(_polygonWkt);
    }

    [Benchmark]
    public MultiPoint? ParseMultiPoint()
    {
        return WktReader.Parse<MultiPoint>(_multiPointWkt);
    }

    [Benchmark]
    public MultiLineString? ParseMultiLineString()
    {
        return WktReader.Parse<MultiLineString>(_multiLineStringWkt);
    }

    [Benchmark]
    public MultiPolygon? ParseMultiPolygon()
    {
        return WktReader.Parse<MultiPolygon>(_multiPolygonWkt);
    }

    [Benchmark]
    public GeometryCollection<Geometry>? ParseGeometryCollection()
    {
        return WktReader.Parse<GeometryCollection<Geometry>>(_geometryCollectionWkt);
    }

    [Benchmark]
    public Polygon? ParsePolygonWithInnerRing()
    {
        return WktReader.Parse<Polygon>(_polygonWithInnerRingWkt);
    }

    [Benchmark]
    public Geometry? ParseGeneric()
    {
        return WktReader.Parse(_pointWkt);
    }
}