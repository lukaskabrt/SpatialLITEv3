using SpatialLite.Contracts;

namespace SpatialLite.UnitTests.Core.Algorithms;

public partial class EuclideanDistanceCalculatorTests
{
    public class PointToPoint : EuclideanDistanceCalculatorTests
    {
        [Fact]
        public void ReturnsZero_IfPointsAreIdentical()
        {
            var c1 = new Coordinate(1.0, 2.0);
            var c2 = new Coordinate(1.0, 2.0);

            var distance = _calculator.CalculateDistance(c1, c2);

            Assert.Equal(0.0, distance, Precision);
        }

        [Theory]
        [InlineData(double.NaN, double.NaN, 1, 1)]
        [InlineData(1, 1, double.NaN, double.NaN)]
        public void ReturnsNaN_IfCoordinatesAreEmpty(double c1x, double c1y, double c2x, double c2y)
        {
            var c1 = new Coordinate(c1x, c1y);
            var c2 = new Coordinate(c2x, c2y);

            var distance = _calculator.CalculateDistance(c1, c2);

            Assert.True(double.IsNaN(distance));
        }

        [Theory]
        [InlineData(2, 1, 6, 4)]
        [InlineData(2, 1, 5, 5)]
        [InlineData(2, 1, -2, 4)]
        [InlineData(2, 1, -1, 5)]
        [InlineData(2, 1, -2, -2)]
        [InlineData(2, 1, -1, -3)]
        [InlineData(2, 1, 6, -2)]
        [InlineData(2, 1, 5, -3)]
        [InlineData(6, 4, 2, 1)]
        [InlineData(5, 5, 2, 1)]
        [InlineData(-2, 4, 2, 1)]
        [InlineData(-1, 5, 2, 1)]
        [InlineData(-2, -2, 2, 1)]
        [InlineData(-1, -3, 2, 1)]
        [InlineData(6, -2, 2, 1)]
        [InlineData(5, -3, 2, 1)]
        public void CalculatesDistance_ForArbitraryPoints(double c1x, double c1y, double c2x, double c2y)
        {
            var c1 = new Coordinate(c1x, c1y);
            var c2 = new Coordinate(c2x, c2y);

            var distance = _calculator.CalculateDistance(c1, c2);

            Assert.Equal(5.0, distance, Precision);
        }
    }
}
