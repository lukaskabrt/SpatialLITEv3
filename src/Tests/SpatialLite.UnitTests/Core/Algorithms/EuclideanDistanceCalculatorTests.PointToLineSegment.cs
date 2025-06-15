using SpatialLite.Contracts;
using SpatialLite.Contracts.Algorithms;

namespace SpatialLite.UnitTests.Core.Algorithms;

public partial class EuclideanDistanceCalculatorTests
{
    public class PointToLineSegment : EuclideanDistanceCalculatorTests
    {
        [Theory]
        [InlineData(1, 4, -2, 3, 2, 1, 2.236067977)]
        [InlineData(-1, 0, -2, 3, 2, 1, 2.236067977)]
        public void CalculatesDistance_IfProjectionIsWithinSegment(double cx, double cy, double ax, double ay, double bx, double by, double expectedDistance)
        {
            var c = new Coordinate(cx, cy);
            var a = new Coordinate(ax, ay);
            var b = new Coordinate(bx, by);

            var distance = _calculator.CalculateDistance(c, a, b, LineMode.LineSegment);

            Assert.Equal(expectedDistance, distance, Precision);
        }

        [Theory]
        [InlineData(-4, 4, -2, 3, 2, 1, 2.236067977)]
        [InlineData(-4, 3, -2, 3, 2, 1, 2)]
        [InlineData(-2, 5, -2, 3, 2, 1, 2)]
        public void CalculatesDistance_IfProjectionIsBeforeSegment(double cx, double cy, double ax, double ay, double bx, double by, double expectedDistance)
        {
            var c = new Coordinate(cx, cy);
            var a = new Coordinate(ax, ay);
            var b = new Coordinate(bx, by);

            var distance = _calculator.CalculateDistance(c, a, b, LineMode.LineSegment);

            Assert.Equal(expectedDistance, distance, Precision);
        }

        [Theory]
        [InlineData(4, 0, -2, 3, 2, 1, 2.236067977)]
        [InlineData(4, 1, -2, 3, 2, 1, 2)]
        [InlineData(2, -1, -2, 3, 2, 1, 2)]
        [InlineData(2, 1, -2, 3, 2, 1, 0)]
        public void CalculatesDistance_IfProjectionIsAfterSegment(double cx, double cy, double ax, double ay, double bx, double by, double expectedDistance)
        {
            var c = new Coordinate(cx, cy);
            var a = new Coordinate(ax, ay);
            var b = new Coordinate(bx, by);

            var distance = _calculator.CalculateDistance(c, a, b, LineMode.LineSegment);

            Assert.Equal(expectedDistance, distance, Precision);
        }

        [Theory]
        [InlineData(-2, 3, -2, 3, 2, 1, 0)]
        [InlineData(0, 2, -2, 3, 2, 1, 0)]
        [InlineData(2, 1, -2, 3, 2, 1, 0)]
        public void CalculatesDistance_IfPointIsOnSegment(double cx, double cy, double ax, double ay, double bx, double by, double expectedDistance)
        {
            var c = new Coordinate(cx, cy);
            var a = new Coordinate(ax, ay);
            var b = new Coordinate(bx, by);

            var distance = _calculator.CalculateDistance(c, a, b, LineMode.LineSegment);

            Assert.Equal(expectedDistance, distance, Precision);
        }

        [Fact]
        public void CalculatesPointToPointDistance_ForDegenerateSegment()
        {
            var c = new Coordinate(3, 5);
            var a = new Coordinate(-1, 2);
            var b = new Coordinate(-1, 2);

            var distance = _calculator.CalculateDistance(c, a, b, LineMode.LineSegment);

            Assert.Equal(5, distance, Precision);
        }

        [Theory]
        [InlineData(double.NaN, double.NaN, 1, 2, 3, 4)]
        [InlineData(3, 5, double.NaN, double.NaN, 3, 4)]
        [InlineData(3, 5, 1, 2, double.NaN, double.NaN)]
        public void ReturnsNull_IfPointIsEmpty(double cx, double cy, double ax, double ay, double bx, double by)
        {
            var c = new Coordinate(cx, cy);
            var a = new Coordinate(ax, ay);
            var b = new Coordinate(bx, by);

            var distance = _calculator.CalculateDistance(c, a, b, LineMode.LineSegment);

            Assert.True(double.IsNaN(distance));
        }
    }
}
