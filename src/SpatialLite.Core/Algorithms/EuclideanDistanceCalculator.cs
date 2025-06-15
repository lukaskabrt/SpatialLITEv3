using SpatialLite.Contracts;
using SpatialLite.Contracts.Algorithms;

namespace SpatialLite.Core.Algorithms;

/// <summary>
/// Provides methods for calculating distance in 2D Euclidean space.
/// </summary>
public class EuclideanDistanceCalculator : IDistanceCalculator
{
    /// <summary>
    /// Calculates distance between two points.
    /// </summary>
    /// <param name="c1">The first point.</param>
    /// <param name="c2">The second point.</param>
    /// <returns>Distance between two points in coordinate's units.</returns>
    public double CalculateDistance(Coordinate c1, Coordinate c2)
    {
        double deltaX = c1.X - c2.X;
        double deltaY = c1.Y - c2.Y;

        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    /// <summary>
    /// Calculates distance between a point and a line AB.
    /// </summary>
    /// <param name="c">The coordinate to compute the distance for.</param>
    /// <param name="a">One point of the line.</param>
    /// <param name="b">Another point of the line.</param>
    /// <param name="mode">LineMode value that specifies whether AB should be treated as an infinite line or as a line segment.</param>
    /// <returns>The distance from C to line AB in coordinate's units.</returns>
    public double CalculateDistance(Coordinate c, Coordinate a, Coordinate b, LineMode mode)
    {
        if (a == b)
        {
            return CalculateDistance(c, a);
        }

        double deltaX = b.X - a.X;
        double deltaY = b.Y - a.Y;

        if (mode == LineMode.LineSegment)
        {
            /*
                Let P be the point of perpendicular projection of C on AB. The parameter
                r, which indicates P's position along AB, is computed by the dot product 
                of AC and AB divided by the square of the length of AB:
                
                    AC dot AB
                r = ---------
                    ||AB||^2
                
                r has the following meaning:
                
                r = 0      P = A
                r = 1      P = B
                r < 0      P is on the backward extension of AB
                r > 1      P is on the forward extension of AB
                0 < r < 1  P is interior to AB
            */

            double r = ((c.X - a.X) * deltaX + (c.Y - a.Y) * deltaY) / (deltaX * deltaX + deltaY * deltaY);

            if (r <= 0.0)
            {
                return CalculateDistance(c, a);
            }

            if (r >= 1.0)
            {
                return CalculateDistance(c, b);
            }
        }

        /*
            Use another parameter s to indicate the location along PC, with the  following meaning:
            s < 0      C is left of AB
            s > 0      C is right of AB
            s = 0      C is on AB
            
            (principally the same as r - only use perpendicular vector)
            
            Compute s as follows:
            
                (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
            s = -----------------------------
                            L^2
        */

        double s = ((a.Y - c.Y) * deltaX - (a.X - c.X) * deltaY) / (deltaX * deltaX + deltaY * deltaY);

        /* Then the distance from C to P = |s|*L */
        return Math.Abs(s) * Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }
}
