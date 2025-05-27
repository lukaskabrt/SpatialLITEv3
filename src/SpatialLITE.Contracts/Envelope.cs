namespace SpatialLITE.Contracts;

/// <summary>
/// Represents minimal bounding box of a geometry.
/// </summary>
public readonly record struct Envelope
{
    /// <summary>
    /// Empty Envelope, that has all its bounds set to double.NaN
    /// </summary>
    public static readonly Envelope Empty = new();

    public readonly double MinX { get; private init; } = double.NaN;
    public readonly double MaxX { get; private init; } = double.NaN;
    public readonly double MinY { get; private init; } = double.NaN;
    public readonly double MaxY { get; private init; } = double.NaN;

    /// <summary>
    /// Initializes a new instance of the <c>Envelope</c> class that is empty and has all its values initialized to <c>double.NaN</c>.
    /// </summary>
    public Envelope()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <c>Envelope</c> class with the single coordinate.
    /// </summary>
    /// <param name="coordinate">The coordinate used initialize <c>Envelope</c></param>
    public Envelope(Coordinate coordinate)
    {
        MinX = coordinate.X;
        MaxX = coordinate.X;
        MinY = coordinate.Y;
        MaxY = coordinate.Y;
    }

    /// <summary>
    /// Initializes a new instance of the <c>Envelope</c> class that covers specified coordinates.
    /// </summary>
    /// <param name="coordinates">The coordinates to be covered.</param>
    public Envelope(IEnumerable<Coordinate> coordinates)
    {
        if (coordinates == null || !coordinates.Any())
        {
            return;
        }

        var first = coordinates.FirstOrDefault(c => c != Coordinate.Empty);
        if (first == Coordinate.Empty)
        {
            return;
        }

        double minX = first.X, maxX = first.X, minY = first.Y, maxY = first.Y;

        foreach (var coordinate in coordinates)
        {
            if (coordinate.X < minX)
            {
                minX = coordinate.X;
            }

            if (coordinate.X > maxX)
            {
                maxX = coordinate.X;
            }

            if (coordinate.Y < minY)
            {
                minY = coordinate.Y;
            }

            if (coordinate.Y > maxY)
            {
                maxY = coordinate.Y;
            }
        }

        MinX = minX;
        MaxX = maxX;
        MinY = minY;
        MaxY = maxY;
    }

    /// <summary>
    /// Returns the difference between the maximum and minimum x values.
    /// </summary>
    /// <returns>max x - min x, or 0 if this is a null <c>Envelope</c>.</returns>
    public double Width
    {
        get
        {
            if (IsEmpty)
            {
                return 0;
            }

            return MaxX - MinX;
        }
    }

    /// <summary>
    /// Returns the difference between the maximum and minimum y values.
    /// </summary>
    /// <returns>max y - min y, or 0 if this is a null <c>Envelope</c>.</returns>
    public double Height
    {
        get
        {
            if (IsEmpty)
            {
                return 0;
            }

            return MaxY - MinY;
        }
    }

    /// <summary>
    /// Checks if this Envelope equals the empty Envelope.
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            return Equals(Empty);
        }
    }

    /// <summary>
    /// Extends this <c>Envelope</c> to cover specified <c>Coordinate</c>.
    /// </summary>
    /// <param name="coordinate">The <c>Coordinate</c> to be covered by extended Envelope.</param>
    public Envelope Extend(Coordinate coordinate)
    {
        if (IsEmpty)
        {
            return new Envelope(coordinate);
        }

        if (coordinate.Equals(Coordinate.Empty))
        {
            return this;
        }

        return new Envelope
        {
            MinX = Math.Min(MinX, coordinate.X),
            MaxX = Math.Max(MaxX, coordinate.X),
            MinY = Math.Min(MinY, coordinate.Y),
            MaxY = Math.Max(MaxY, coordinate.Y)
        };
    }

    /// <summary>
    /// Extends this <c>Envelope</c> to cover specified <c>Coordinates</c>.
    /// </summary>
    /// <param name="coordinates">The collection of Coordinates to be covered by extended Envelope.</param>
    public Envelope Extend(IEnumerable<Coordinate> coordinates)
    {
        var extended = this;

        foreach (var coordinate in coordinates)
        {
            extended = extended.Extend(coordinate);
        }

        return extended;
    }

    /// <summary>
    /// Extends this <c>Envelope</c> to cover specified <c>Envelope</c>.
    /// </summary>
    /// <param name="envelope">The <c>Envelope</c> to be covered by extended Envelope.</param>
    public Envelope Extend(Envelope envelope)
    {
        if (envelope.IsEmpty)
        {
            return this;
        }

        if (IsEmpty)
        {
            return envelope;
        }

        return new Envelope
        {
            MinX = Math.Min(MinX, envelope.MinX),
            MaxX = Math.Max(MaxX, envelope.MaxX),
            MinY = Math.Min(MinY, envelope.MinY),
            MaxY = Math.Max(MaxY, envelope.MaxY)
        };
    }

    /// <summary>
    /// Check if the region defined by <c>other</c>
    /// overlaps (intersects) the region of this <c>Envelope</c>.
    /// </summary>
    /// <param name="other"> the <c>Envelope</c> which this <c>Envelope</c> is being checked for overlapping.</param>
    /// <returns>
    /// <c>true</c> if the <c>Envelope</c>s overlap.
    /// </returns>
    public bool Intersects(Envelope other)
    {
        if (IsEmpty || other.IsEmpty)
        {
            return false;
        }

        return !(other.MinX > MaxX || other.MaxX < MinX || other.MinY > MaxY || other.MaxY < MinY);
    }

    ///<summary>
    /// Tests if the given point lies in or on the envelope.
    ///</summary>
    /// <param name="x">the x-coordinate of the point which this <c>Envelope</c> is being checked for containing</param>
    /// <param name="y">the y-coordinate of the point which this <c>Envelope</c> is being checked for containing</param>
    /// <returns> <c>true</c> if <c>(x, y)</c> lies in the interior or on the boundary of this <c>Envelope</c>.</returns>
    public bool Covers(double x, double y)
    {
        if (IsEmpty)
        {
            return false;
        }

        return x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
    }

    ///<summary>
    /// Tests if the given point lies in or on the envelope.
    ///</summary>
    /// <param name="p">the point which this <c>Envelope</c> is being checked for containing</param>
    /// <returns><c>true</c> if the point lies in the interior or on the boundary of this <c>Envelope</c>.</returns>
    public bool Covers(Coordinate p)
    {
        return Covers(p.X, p.Y);
    }

    ///<summary>
    /// Tests if the <c>Envelope other</c> lies wholly inside this <c>Envelope</c> (inclusive of the boundary).
    ///</summary>
    /// <param name="other">the <c>Envelope</c> to check</param>
    /// <returns>true if this <c>Envelope</c> covers the <c>other</c></returns>
    public bool Covers(Envelope other)
    {
        if (IsEmpty || other.IsEmpty)
        {
            return false;
        }

        return other.MinX >= MinX && other.MaxX <= MaxX && other.MinY >= MinY && other.MaxY <= MaxY;
    }
}
