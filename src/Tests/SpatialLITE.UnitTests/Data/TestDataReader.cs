using System.Reflection;

namespace SpatialLITE.UnitTests.Data;

/// <summary>
/// Helper class for loading test data files.
/// </summary>
public class TestDataReader
{
    private readonly string _resourcePrefix;
    private readonly Assembly _assembly;
    private readonly string _dataFolderPath;

    /// <summary>
    /// Initializes a new instance of the TestDataReader class.
    /// </summary>
    /// <param name="resourcePrefix">Prefix for embedded resources</param>
    /// <param name="folderPath">Relative folder path for outputted files</param>
    public TestDataReader(string resourcePrefix, string folderPath)
    {
        _resourcePrefix = resourcePrefix;
        _assembly = typeof(TestDataReader).GetTypeInfo().Assembly;
        
        // Build path to the output directory where files are copied
        var assemblyLocation = _assembly.Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) 
            ?? throw new InvalidOperationException("Could not determine assembly directory.");
        _dataFolderPath = Path.Combine(assemblyDirectory, "Data", folderPath);
    }

    /// <summary>
    /// Opens a stream to the test data file.
    /// </summary>
    /// <param name="name">Name of the test data file</param>
    /// <returns>Stream of the test data</returns>
    public Stream Open(string name)
    {
        // Try to get from file first
        var filePath = GetPath(name);
        if (File.Exists(filePath))
        {
            return File.OpenRead(filePath);
        }

        // Fall back to embedded resources if file doesn't exist
        return _assembly.GetManifestResourceStream(_resourcePrefix + name)
            ?? throw new Exception($"Resource {_resourcePrefix + name} not found.");
    }

    /// <summary>
    /// Reads the test data file into a byte array.
    /// </summary>
    /// <param name="name">Name of the test data file</param>
    /// <returns>Byte array containing the test data</returns>
    public byte[] Read(string name)
    {
        // Try to get from file first
        var filePath = GetPath(name);
        if (File.Exists(filePath))
        {
            return File.ReadAllBytes(filePath);
        }

        // Fall back to embedded resources if file doesn't exist
        using var stream = new MemoryStream();
        using var resourceStream = _assembly.GetManifestResourceStream(_resourcePrefix + name)
            ?? throw new Exception($"Resource {_resourcePrefix + name} not found.");

        resourceStream.CopyTo(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Gets the full path to a test data file.
    /// </summary>
    /// <param name="name">Name of the test data file</param>
    /// <returns>Full path to the test data file</returns>
    public string GetPath(string name)
    {
        return Path.Combine(_dataFolderPath, name);
    }

    /// <summary>
    /// TestDataReader for Core IO test data.
    /// </summary>
    public static readonly TestDataReader CoreIO = new("SpatialLITE.UnitTests.Data.Core.IO.", "Core/IO");

    /// <summary>
    /// TestDataReader for OSM XML test data.
    /// </summary>
    public static readonly TestDataReader OsmXml = new("SpatialLITE.UnitTests.Data.Osm.Xml.", "Osm/Xml");
}
