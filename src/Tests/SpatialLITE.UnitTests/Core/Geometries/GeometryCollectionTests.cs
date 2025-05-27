using SpatialLite.Core.Geometries;
using SpatialLITE.Contracts;

namespace SpatialLITE.UnitTests.Core.Geometries;

public class GeometryCollectionTests
{
    private readonly Geometry[] _geometries = [
        new Point(1, 2),
        new Point(1.1, 2.1),
        new Point(1.2, 2.2)
    ];

    [Fact]
    public void Constructor__CreatesNewEmptyCollection()
    {
        var target = new GeometryCollection<Geometry>();

        Assert.NotNull(target.Geometries);
        Assert.Empty(target.Geometries);
    }

    [Fact]
    public void Constructor_IEnumerable_CreateNewCollectionWithData()
    {
        var target = new GeometryCollection<Geometry>(_geometries);

        Assert.Equal(_geometries, target.Geometries, (e, a) => e.Equals(a));
    }

    [Fact]
    public void GetEnvelopeReturnsEmptyEnvelopeForEmptyCollection()
    {
        var target = new GeometryCollection<Geometry>();

        Assert.Equal(Envelope.Empty, target.GetEnvelope());
    }

    [Fact]
    public void GetEnvelopeReturnsUnionOfMembersEnvelopes()
    {
        var expected = new Envelope(_geometries.SelectMany(g => g.GetCoordinates()));

        var target = new GeometryCollection<Geometry>(_geometries);

        Assert.Equal(expected, target.GetEnvelope());
    }
}
