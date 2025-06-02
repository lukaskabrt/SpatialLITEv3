namespace SpatialLite.Core.IO;

/// <summary>
/// Represents exception that occurs if WkbReader encounters invalid data.
/// </summary>
public class WkbFormatException : Exception
{
    /// <summary>
    /// Initializes a new instance of the WkbFormatException class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public WkbFormatException(string message)
        : base(message)
    {
    }
}
