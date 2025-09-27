using SpatialLite.Osm;
using SpatialLite.Osm.IO.Pbf;

namespace SpatialLite.Workbench;

internal class Program
{
    public static void Main(string[] args)
    {
        var start = DateTime.Now;

        var nodesCount = 0;
        var waysCount = 0;
        var relationsCount = 0;

        using var reader = new PbfReader("C:\\OSM\\valhalla\\cz.pbf", new Osm.IO.OsmReaderSettings { ReadMetadata = false });
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            if (entity is Node node)
            {
                nodesCount++;
            }
            else if (entity is Way way)
            {
                waysCount++;
            }
            else if (entity is Relation relation)
            {
                relationsCount++;
            }
        }

        Console.WriteLine($"Nodes: {nodesCount}, Ways: {waysCount}, Relations: {relationsCount}");
        Console.WriteLine($"Elapsed time: {DateTime.Now - start}");
    }
}
