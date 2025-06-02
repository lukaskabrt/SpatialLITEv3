using SpatialLite.Contracts;

namespace SpatialLite.UnitTests.Contracts;

public class CoordinateTests
{
    private readonly double _xCoordinate = 3.5;
    private readonly double _yCoordinate = 4.2;

    [Fact]
    public void Constructor_XY_SetsXY()
    {
        var target = new Coordinate(_xCoordinate, _yCoordinate);

        Assert.Equal(_xCoordinate, target.X);
        Assert.Equal(_yCoordinate, target.Y);
    }
}
