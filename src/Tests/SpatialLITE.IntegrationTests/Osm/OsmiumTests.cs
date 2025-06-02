using SpatialLite.Osm;
using SpatialLite.Osm.IO;
using SpatialLITE.IntegrationTests.Data;
using SpatialLITE.Osm.IO.Pbf;
using System.Diagnostics;
using System.Reflection;

namespace SpatialLITE.IntegrationTests.Osm;

public class OsmiumTests
{
    protected void AssertReadWithOsmium(string file, string targetFile)
    {
        var assembly = typeof(OsmiumTests).GetTypeInfo().Assembly;
        var assemblyDirectory = Path.GetDirectoryName(assembly.Location)!;

        var osmiumInfo = new ProcessStartInfo()
        {
            FileName = Path.Combine(assemblyDirectory, "Tools", "Osmium", "Windows", "osmium.exe"),
            Arguments = $"cat {file} --output {targetFile}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        string? stdOut = null;
        string? stdErr = null;
        int? exitCode = null;

        using (var osmium = Process.Start(osmiumInfo))
        {
            stdOut = osmium?.StandardOutput.ReadToEnd();
            stdErr = osmium?.StandardError.ReadToEnd();
            osmium?.WaitForExit();
            exitCode = osmium?.ExitCode;
        }

        if (exitCode != 0)
        {
            var message = $"Osmium exited with code {exitCode}.\nStandard Output:\n{stdOut}\nStandard Error:\n{stdErr}";
            throw new Xunit.Sdk.XunitException(message);
        }
    }

    protected void GenerateOutputFile(IOsmWriter writer)
    {
        using var reader = new PbfReader(TestDataReader.OsmPbf.GetPath("monaco-metadata.osm.pbf"), new OsmReaderSettings { ReadMetadata = true });

        IOsmEntity? entity = null;
        while ((entity = reader.Read()) != null)
        {
            writer.Write(entity);
        }
    }

    protected static string GetTempFileName(string extension) =>
        Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), extension));
}
