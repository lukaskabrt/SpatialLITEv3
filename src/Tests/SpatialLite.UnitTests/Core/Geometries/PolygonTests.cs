using SpatialLite.Contracts;
using SpatialLite.Core.Geometries;

namespace SpatialLite.UnitTests.Core.Geometries;

public class PolygonTests
{
    [Fact]
    public void Constructor__CreatesPolygonWithEmptyRings()
    {
        var polygon = new Polygon();

        Assert.NotNull(polygon.ExteriorRing);
        Assert.Empty(polygon.ExteriorRing);

        Assert.NotNull(polygon.InteriorRings);
        Assert.Empty(polygon.InteriorRings);
    }

    [Fact]
    public void Constructor_ExteriorRing_CreatesPolygonWithExteriorRingAndEmptyInteriorRings()
    {
        var exterior = new List<Coordinate>
        {
            new(0, 0), new(0, 1), new(1, 1), new(1, 0)
        };

        var polygon = new Polygon(exterior);

        Assert.Equal(exterior, polygon.ExteriorRing, (e, a) => e == a);

        Assert.NotNull(polygon.InteriorRings);
        Assert.Empty(polygon.InteriorRings);
    }

    [Fact]
    public void GetEnvelope__ReturnsEmptyEnvelopeForEmptyPolygon()
    {
        var polygon = new Polygon();

        var envelope = polygon.GetEnvelope();

        Assert.Equal(Envelope.Empty, envelope);
    }

    [Fact]
    public void GetEnvelope_ReturnsEnvelopeThatCoversPolygon()
    {
        var exterior = new List<Coordinate>
        {
            new(0, 0), new(0, 1), new(1, 1), new(1, 0)
        };
        var polygon = new Polygon(exterior);

        var envelope = polygon.GetEnvelope();

        Assert.Equal(new Envelope(exterior), envelope);
    }
}
