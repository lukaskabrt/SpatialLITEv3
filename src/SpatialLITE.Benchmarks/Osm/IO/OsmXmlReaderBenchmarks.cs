using BenchmarkDotNet.Attributes;
using SpatialLite.Osm;
using SpatialLITE.Osm.IO.Xml;

namespace SpatialLITE.Benchmarks.Osm.IO;

[MemoryDiagnoser]
public class OsmXmlReaderBenchmarks
{
    private MemoryStream _multipleNodesStream = null!;
    private MemoryStream _multipleNodesWithTagsStream = null!;
    private MemoryStream _multipleNodesWithMetadataStream = null!;
    private MemoryStream _multipleWaysStream = null!;
    private MemoryStream _multipleWaysWithTagsStream = null!;
    private MemoryStream _multipleRelationsStream = null!;
    private MemoryStream _multipleRelationsWithTagsStream = null!;
    private MemoryStream _complexMixedStream = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Create XML data with multiple entities for more realistic benchmarks
        var multipleNodesXml = """
            <?xml version='1.0' encoding='UTF-8'?>
            <osm version='0.6'>
              <node id='1' lat='50.086758' lon='14.4092038' />
              <node id='2' lat='50.0860597' lon='14.4143866' />
              <node id='3' lat='50.0886819' lon='14.415577' />
              <node id='4' lat='50.087234' lon='14.416123' />
              <node id='5' lat='50.088567' lon='14.417456' />
              <node id='6' lat='50.089123' lon='14.418789' />
              <node id='7' lat='50.090456' lon='14.419012' />
              <node id='8' lat='50.091789' lon='14.420345' />
              <node id='9' lat='50.092012' lon='14.421678' />
              <node id='10' lat='50.093345' lon='14.422901' />
            </osm>
            """;

        var multipleNodesWithTagsXml = """
            <?xml version='1.0' encoding='UTF-8'?>
            <osm version='0.6'>
              <node id='1' lat='50.086758' lon='14.4092038'>
                <tag k='highway' v='traffic_signals' />
                <tag k='name' v='Node 1' />
              </node>
              <node id='2' lat='50.0860597' lon='14.4143866'>
                <tag k='highway' v='crossing' />
                <tag k='name' v='Node 2' />
              </node>
              <node id='3' lat='50.0886819' lon='14.415577'>
                <tag k='highway' v='stop' />
                <tag k='name' v='Node 3' />
              </node>
              <node id='4' lat='50.087234' lon='14.416123'>
                <tag k='amenity' v='restaurant' />
                <tag k='name' v='Node 4' />
              </node>
              <node id='5' lat='50.088567' lon='14.417456'>
                <tag k='shop' v='supermarket' />
                <tag k='name' v='Node 5' />
              </node>
              <node id='6' lat='50.089123' lon='14.418789'>
                <tag k='leisure' v='park' />
                <tag k='name' v='Node 6' />
              </node>
              <node id='7' lat='50.090456' lon='14.419012'>
                <tag k='tourism' v='hotel' />
                <tag k='name' v='Node 7' />
              </node>
              <node id='8' lat='50.091789' lon='14.420345'>
                <tag k='public_transport' v='stop_position' />
                <tag k='name' v='Node 8' />
              </node>
              <node id='9' lat='50.092012' lon='14.421678'>
                <tag k='barrier' v='bollard' />
                <tag k='name' v='Node 9' />
              </node>
              <node id='10' lat='50.093345' lon='14.422901'>
                <tag k='natural' v='tree' />
                <tag k='name' v='Node 10' />
              </node>
            </osm>
            """;

        var multipleNodesWithMetadataXml = """
            <?xml version='1.0' encoding='UTF-8'?>
            <osm version='0.6'>
              <node id='1' timestamp='2008-09-25T14:04:49Z' uid='17615' user='User1' visible='true' version='1' changeset='695161' lat='50.086758' lon='14.4092038' />
              <node id='2' timestamp='2008-09-25T14:05:49Z' uid='17616' user='User2' visible='true' version='2' changeset='695162' lat='50.0860597' lon='14.4143866' />
              <node id='3' timestamp='2008-09-25T14:06:49Z' uid='17617' user='User3' visible='true' version='3' changeset='695163' lat='50.0886819' lon='14.415577' />
              <node id='4' timestamp='2008-09-25T14:07:49Z' uid='17618' user='User4' visible='true' version='4' changeset='695164' lat='50.087234' lon='14.416123' />
              <node id='5' timestamp='2008-09-25T14:08:49Z' uid='17619' user='User5' visible='true' version='5' changeset='695165' lat='50.088567' lon='14.417456' />
              <node id='6' timestamp='2008-09-25T14:09:49Z' uid='17620' user='User6' visible='true' version='6' changeset='695166' lat='50.089123' lon='14.418789' />
              <node id='7' timestamp='2008-09-25T14:10:49Z' uid='17621' user='User7' visible='true' version='7' changeset='695167' lat='50.090456' lon='14.419012' />
              <node id='8' timestamp='2008-09-25T14:11:49Z' uid='17622' user='User8' visible='true' version='8' changeset='695168' lat='50.091789' lon='14.420345' />
              <node id='9' timestamp='2008-09-25T14:12:49Z' uid='17623' user='User9' visible='true' version='9' changeset='695169' lat='50.092012' lon='14.421678' />
              <node id='10' timestamp='2008-09-25T14:13:49Z' uid='17624' user='User10' visible='true' version='10' changeset='695170' lat='50.093345' lon='14.422901' />
            </osm>
            """;

        var multipleWaysXml = """
            <?xml version='1.0' encoding='UTF-8'?>
            <osm version='0.6'>
              <way id='1'>
                <nd ref='1' />
                <nd ref='2' />
                <nd ref='3' />
              </way>
              <way id='2'>
                <nd ref='3' />
                <nd ref='4' />
                <nd ref='5' />
              </way>
              <way id='3'>
                <nd ref='5' />
                <nd ref='6' />
                <nd ref='7' />
              </way>
              <way id='4'>
                <nd ref='7' />
                <nd ref='8' />
                <nd ref='9' />
              </way>
              <way id='5'>
                <nd ref='9' />
                <nd ref='10' />
                <nd ref='1' />
              </way>
              <way id='6'>
                <nd ref='2' />
                <nd ref='4' />
                <nd ref='6' />
                <nd ref='8' />
                <nd ref='10' />
              </way>
              <way id='7'>
                <nd ref='1' />
                <nd ref='3' />
                <nd ref='5' />
                <nd ref='7' />
                <nd ref='9' />
              </way>
              <way id='8'>
                <nd ref='10' />
                <nd ref='8' />
                <nd ref='6' />
                <nd ref='4' />
                <nd ref='2' />
              </way>
              <way id='9'>
                <nd ref='9' />
                <nd ref='7' />
                <nd ref='5' />
                <nd ref='3' />
                <nd ref='1' />
              </way>
              <way id='10'>
                <nd ref='2' />
                <nd ref='6' />
                <nd ref='10' />
                <nd ref='4' />
                <nd ref='8' />
              </way>
            </osm>
            """;

        var multipleWaysWithTagsXml = """
            <?xml version='1.0' encoding='UTF-8'?>
            <osm version='0.6'>
              <way id='1'>
                <nd ref='1' />
                <nd ref='2' />
                <nd ref='3' />
                <tag k='highway' v='primary' />
                <tag k='name' v='Main Street' />
              </way>
              <way id='2'>
                <nd ref='3' />
                <nd ref='4' />
                <nd ref='5' />
                <tag k='highway' v='secondary' />
                <tag k='name' v='Second Street' />
              </way>
              <way id='3'>
                <nd ref='5' />
                <nd ref='6' />
                <nd ref='7' />
                <tag k='highway' v='residential' />
                <tag k='name' v='Oak Avenue' />
              </way>
              <way id='4'>
                <nd ref='7' />
                <nd ref='8' />
                <nd ref='9' />
                <tag k='highway' v='footway' />
                <tag k='name' v='Walking Path' />
              </way>
              <way id='5'>
                <nd ref='9' />
                <nd ref='10' />
                <nd ref='1' />
                <tag k='highway' v='cycleway' />
                <tag k='name' v='Bike Lane' />
              </way>
              <way id='6'>
                <nd ref='2' />
                <nd ref='4' />
                <nd ref='6' />
                <nd ref='8' />
                <nd ref='10' />
                <tag k='waterway' v='river' />
                <tag k='name' v='Test River' />
              </way>
              <way id='7'>
                <nd ref='1' />
                <nd ref='3' />
                <nd ref='5' />
                <nd ref='7' />
                <nd ref='9' />
                <tag k='railway' v='rail' />
                <tag k='name' v='Train Line' />
              </way>
              <way id='8'>
                <nd ref='10' />
                <nd ref='8' />
                <nd ref='6' />
                <nd ref='4' />
                <nd ref='2' />
                <tag k='boundary' v='administrative' />
                <tag k='admin_level' v='4' />
              </way>
              <way id='9'>
                <nd ref='9' />
                <nd ref='7' />
                <nd ref='5' />
                <nd ref='3' />
                <nd ref='1' />
                <tag k='landuse' v='residential' />
                <tag k='name' v='Housing Area' />
              </way>
              <way id='10'>
                <nd ref='2' />
                <nd ref='6' />
                <nd ref='10' />
                <nd ref='4' />
                <nd ref='8' />
                <tag k='natural' v='coastline' />
                <tag k='name' v='Coastline' />
              </way>
            </osm>
            """;

        var multipleRelationsXml = """
            <?xml version='1.0' encoding='UTF-8'?>
            <osm version='0.6'>
              <relation id='1'>
                <member type='way' ref='1' role='outer' />
                <member type='node' ref='1' role='' />
              </relation>
              <relation id='2'>
                <member type='way' ref='2' role='inner' />
                <member type='node' ref='2' role='center' />
              </relation>
              <relation id='3'>
                <member type='way' ref='3' role='outer' />
                <member type='way' ref='4' role='inner' />
                <member type='node' ref='3' role='entrance' />
              </relation>
              <relation id='4'>
                <member type='relation' ref='1' role='parent' />
                <member type='way' ref='5' role='border' />
              </relation>
              <relation id='5'>
                <member type='node' ref='4' role='point' />
                <member type='node' ref='5' role='point' />
                <member type='node' ref='6' role='point' />
              </relation>
              <relation id='6'>
                <member type='way' ref='6' role='from' />
                <member type='way' ref='7' role='to' />
                <member type='way' ref='8' role='via' />
              </relation>
              <relation id='7'>
                <member type='relation' ref='2' role='subarea' />
                <member type='relation' ref='3' role='subarea' />
              </relation>
              <relation id='8'>
                <member type='way' ref='9' role='platform' />
                <member type='way' ref='10' role='stop' />
                <member type='node' ref='7' role='entrance' />
              </relation>
              <relation id='9'>
                <member type='node' ref='8' role='start' />
                <member type='node' ref='9' role='end' />
                <member type='way' ref='1' role='route' />
              </relation>
              <relation id='10'>
                <member type='relation' ref='4' role='boundary' />
                <member type='way' ref='2' role='admin_centre' />
                <member type='node' ref='10' role='capital' />
              </relation>
            </osm>
            """;

        var multipleRelationsWithTagsXml = """
            <?xml version='1.0' encoding='UTF-8'?>
            <osm version='0.6'>
              <relation id='1'>
                <member type='way' ref='1' role='outer' />
                <member type='node' ref='1' role='' />
                <tag k='type' v='multipolygon' />
                <tag k='name' v='Test Area 1' />
              </relation>
              <relation id='2'>
                <member type='way' ref='2' role='inner' />
                <member type='node' ref='2' role='center' />
                <tag k='type' v='boundary' />
                <tag k='boundary' v='administrative' />
              </relation>
              <relation id='3'>
                <member type='way' ref='3' role='outer' />
                <member type='way' ref='4' role='inner' />
                <member type='node' ref='3' role='entrance' />
                <tag k='type' v='multipolygon' />
                <tag k='building' v='yes' />
                <tag k='name' v='Complex Building' />
              </relation>
              <relation id='4'>
                <member type='relation' ref='1' role='parent' />
                <member type='way' ref='5' role='border' />
                <tag k='type' v='route' />
                <tag k='route' v='bus' />
                <tag k='ref' v='42' />
              </relation>
              <relation id='5'>
                <member type='node' ref='4' role='point' />
                <member type='node' ref='5' role='point' />
                <member type='node' ref='6' role='point' />
                <tag k='type' v='collection' />
                <tag k='name' v='Point Collection' />
              </relation>
              <relation id='6'>
                <member type='way' ref='6' role='from' />
                <member type='way' ref='7' role='to' />
                <member type='way' ref='8' role='via' />
                <tag k='type' v='restriction' />
                <tag k='restriction' v='no_left_turn' />
              </relation>
              <relation id='7'>
                <member type='relation' ref='2' role='subarea' />
                <member type='relation' ref='3' role='subarea' />
                <tag k='type' v='site' />
                <tag k='site' v='administrative' />
                <tag k='name' v='Administrative Site' />
              </relation>
              <relation id='8'>
                <member type='way' ref='9' role='platform' />
                <member type='way' ref='10' role='stop' />
                <member type='node' ref='7' role='entrance' />
                <tag k='type' v='public_transport' />
                <tag k='public_transport' v='stop_area' />
                <tag k='name' v='Transit Hub' />
              </relation>
              <relation id='9'>
                <member type='node' ref='8' role='start' />
                <member type='node' ref='9' role='end' />
                <member type='way' ref='1' role='route' />
                <tag k='type' v='route' />
                <tag k='route' v='hiking' />
                <tag k='network' v='lwn' />
              </relation>
              <relation id='10'>
                <member type='relation' ref='4' role='boundary' />
                <member type='way' ref='2' role='admin_centre' />
                <member type='node' ref='10' role='capital' />
                <tag k='type' v='boundary' />
                <tag k='boundary' v='political' />
                <tag k='admin_level' v='2' />
                <tag k='name' v='Country Boundary' />
              </relation>
            </osm>
            """;

        var complexMixedXml = """
            <?xml version='1.0' encoding='UTF-8'?>
            <osm version='0.6'>
              <node id='1' lat='50.086758' lon='14.4092038'>
                <tag k='highway' v='traffic_signals' />
                <tag k='name' v='Junction 1' />
              </node>
              <node id='2' lat='50.0860597' lon='14.4143866'>
                <tag k='amenity' v='restaurant' />
                <tag k='name' v='Test Restaurant' />
              </node>
              <node id='3' lat='50.0886819' lon='14.415577'>
                <tag k='shop' v='supermarket' />
                <tag k='name' v='Local Market' />
              </node>
              <node id='4' lat='50.087234' lon='14.416123' />
              <node id='5' lat='50.088567' lon='14.417456' />
              <node id='6' lat='50.089123' lon='14.418789' />
              <node id='7' lat='50.090456' lon='14.419012' />
              <node id='8' lat='50.091789' lon='14.420345' />
              <node id='9' lat='50.092012' lon='14.421678' />
              <node id='10' lat='50.093345' lon='14.422901' />
              
              <way id='100'>
                <nd ref='1' />
                <nd ref='2' />
                <nd ref='3' />
                <nd ref='4' />
                <nd ref='5' />
                <tag k='highway' v='primary' />
                <tag k='name' v='Main Highway' />
                <tag k='maxspeed' v='80' />
              </way>
              <way id='101'>
                <nd ref='5' />
                <nd ref='6' />
                <nd ref='7' />
                <nd ref='8' />
                <tag k='highway' v='secondary' />
                <tag k='name' v='Secondary Road' />
              </way>
              <way id='102'>
                <nd ref='8' />
                <nd ref='9' />
                <nd ref='10' />
                <nd ref='1' />
                <tag k='highway' v='residential' />
                <tag k='name' v='Residential Street' />
              </way>
              <way id='103'>
                <nd ref='2' />
                <nd ref='4' />
                <nd ref='6' />
                <nd ref='8' />
                <nd ref='10' />
                <tag k='waterway' v='river' />
                <tag k='name' v='Test River' />
              </way>
              <way id='104'>
                <nd ref='3' />
                <nd ref='7' />
                <nd ref='9' />
                <nd ref='1' />
                <tag k='landuse' v='forest' />
                <tag k='name' v='City Forest' />
              </way>
              
              <relation id='200'>
                <member type='way' ref='100' role='outer' />
                <member type='way' ref='101' role='inner' />
                <member type='node' ref='1' role='entrance' />
                <tag k='type' v='multipolygon' />
                <tag k='landuse' v='commercial' />
                <tag k='name' v='Shopping District' />
              </relation>
              <relation id='201'>
                <member type='way' ref='102' role='route' />
                <member type='way' ref='103' role='route' />
                <member type='node' ref='2' role='stop' />
                <member type='node' ref='3' role='stop' />
                <tag k='type' v='route' />
                <tag k='route' v='bus' />
                <tag k='ref' v='42' />
                <tag k='name' v='City Bus Route' />
              </relation>
              <relation id='202'>
                <member type='relation' ref='200' role='subarea' />
                <member type='way' ref='104' role='boundary' />
                <member type='node' ref='4' role='admin_centre' />
                <tag k='type' v='boundary' />
                <tag k='boundary' v='administrative' />
                <tag k='admin_level' v='8' />
                <tag k='name' v='City District' />
              </relation>
              <relation id='203'>
                <member type='node' ref='5' role='via' />
                <member type='way' ref='100' role='from' />
                <member type='way' ref='101' role='to' />
                <tag k='type' v='restriction' />
                <tag k='restriction' v='no_left_turn' />
              </relation>
              <relation id='204'>
                <member type='way' ref='103' role='waterway' />
                <member type='node' ref='6' role='spring' />
                <member type='node' ref='10' role='mouth' />
                <tag k='type' v='waterway' />
                <tag k='waterway' v='river' />
                <tag k='name' v='River System' />
              </relation>
            </osm>
            """;

        // Create streams
        _multipleNodesStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(multipleNodesXml));
        _multipleNodesWithTagsStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(multipleNodesWithTagsXml));
        _multipleNodesWithMetadataStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(multipleNodesWithMetadataXml));
        _multipleWaysStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(multipleWaysXml));
        _multipleWaysWithTagsStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(multipleWaysWithTagsXml));
        _multipleRelationsStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(multipleRelationsXml));
        _multipleRelationsWithTagsStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(multipleRelationsWithTagsXml));
        _complexMixedStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(complexMixedXml));
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _multipleNodesStream?.Dispose();
        _multipleNodesWithTagsStream?.Dispose();
        _multipleNodesWithMetadataStream?.Dispose();
        _multipleWaysStream?.Dispose();
        _multipleWaysWithTagsStream?.Dispose();
        _multipleRelationsStream?.Dispose();
        _multipleRelationsWithTagsStream?.Dispose();
        _complexMixedStream?.Dispose();
    }

    [Benchmark]
    public int ReadMultipleNodes()
    {
        // Reset stream position and create new reader for this iteration
        _multipleNodesStream.Position = 0;
        using var reader = new OsmXmlReader(_multipleNodesStream, new OsmXmlReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }

    [Benchmark]
    public int ReadMultipleNodesWithTags()
    {
        // Reset stream position and create new reader for this iteration
        _multipleNodesWithTagsStream.Position = 0;
        using var reader = new OsmXmlReader(_multipleNodesWithTagsStream, new OsmXmlReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }

    [Benchmark]
    public int ReadMultipleNodesWithMetadata()
    {
        // Reset stream position and create new reader for this iteration
        _multipleNodesWithMetadataStream.Position = 0;
        using var reader = new OsmXmlReader(_multipleNodesWithMetadataStream, new OsmXmlReaderSettings { ReadMetadata = true });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }

    [Benchmark]
    public int ReadMultipleWays()
    {
        // Reset stream position and create new reader for this iteration
        _multipleWaysStream.Position = 0;
        using var reader = new OsmXmlReader(_multipleWaysStream, new OsmXmlReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }

    [Benchmark]
    public int ReadMultipleWaysWithTags()
    {
        // Reset stream position and create new reader for this iteration
        _multipleWaysWithTagsStream.Position = 0;
        using var reader = new OsmXmlReader(_multipleWaysWithTagsStream, new OsmXmlReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }

    [Benchmark]
    public int ReadMultipleRelations()
    {
        // Reset stream position and create new reader for this iteration
        _multipleRelationsStream.Position = 0;
        using var reader = new OsmXmlReader(_multipleRelationsStream, new OsmXmlReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }

    [Benchmark]
    public int ReadMultipleRelationsWithTags()
    {
        // Reset stream position and create new reader for this iteration
        _multipleRelationsWithTagsStream.Position = 0;
        using var reader = new OsmXmlReader(_multipleRelationsWithTagsStream, new OsmXmlReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }

    [Benchmark]
    public int ReadComplexMixedData()
    {
        // Reset stream position and create new reader for this iteration
        _complexMixedStream.Position = 0;
        using var reader = new OsmXmlReader(_complexMixedStream, new OsmXmlReaderSettings { ReadMetadata = false });

        int count = 0;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            count++;
        }

        return count;
    }
}
