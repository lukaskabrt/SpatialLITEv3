using BenchmarkDotNet.Attributes;

namespace SpatialLITE.Benchmarks.Osm.IO;

public partial class OsmXmlReaderBenchmarks
{
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
                <tag k='highway' v='bus_stop' />
                <tag k='name' v='Node 3' />
              </node>
              <node id='4' lat='50.087234' lon='14.416123'>
                <tag k='amenity' v='restaurant' />
                <tag k='name' v='Node 4' />
              </node>
              <node id='5' lat='50.088567' lon='14.417456'>
                <tag k='amenity' v='cafe' />
                <tag k='name' v='Node 5' />
              </node>
              <node id='6' lat='50.089123' lon='14.418789'>
                <tag k='shop' v='supermarket' />
                <tag k='name' v='Node 6' />
              </node>
              <node id='7' lat='50.090456' lon='14.419012'>
                <tag k='leisure' v='park' />
                <tag k='name' v='Node 7' />
              </node>
              <node id='8' lat='50.091789' lon='14.420345'>
                <tag k='tourism' v='hotel' />
                <tag k='name' v='Node 8' />
              </node>
              <node id='9' lat='50.092012' lon='14.421678'>
                <tag k='natural' v='tree' />
                <tag k='name' v='Node 9' />
              </node>
              <node id='10' lat='50.093345' lon='14.422901'>
                <tag k='historic' v='monument' />
                <tag k='name' v='Node 10' />
              </node>
            </osm>
            """;

        var multipleNodesWithMetadataXml = """
            <?xml version='1.0' encoding='UTF-8'?>
            <osm version='0.6'>
              <node id='1' lat='50.086758' lon='14.4092038' version='1' timestamp='2023-01-01T12:00:00Z' changeset='123' uid='1' user='testuser'>
                <tag k='highway' v='traffic_signals' />
              </node>
              <node id='2' lat='50.0860597' lon='14.4143866' version='2' timestamp='2023-01-02T12:00:00Z' changeset='124' uid='2' user='testuser2'>
                <tag k='highway' v='crossing' />
              </node>
              <node id='3' lat='50.0886819' lon='14.415577' version='1' timestamp='2023-01-03T12:00:00Z' changeset='125' uid='3' user='testuser3'>
                <tag k='highway' v='bus_stop' />
              </node>
              <node id='4' lat='50.087234' lon='14.416123' version='3' timestamp='2023-01-04T12:00:00Z' changeset='126' uid='4' user='testuser4'>
                <tag k='amenity' v='restaurant' />
              </node>
              <node id='5' lat='50.088567' lon='14.417456' version='1' timestamp='2023-01-05T12:00:00Z' changeset='127' uid='5' user='testuser5'>
                <tag k='amenity' v='cafe' />
              </node>
              <node id='6' lat='50.089123' lon='14.418789' version='2' timestamp='2023-01-06T12:00:00Z' changeset='128' uid='6' user='testuser6'>
                <tag k='shop' v='supermarket' />
              </node>
              <node id='7' lat='50.090456' lon='14.419012' version='1' timestamp='2023-01-07T12:00:00Z' changeset='129' uid='7' user='testuser7'>
                <tag k='leisure' v='park' />
              </node>
              <node id='8' lat='50.091789' lon='14.420345' version='4' timestamp='2023-01-08T12:00:00Z' changeset='130' uid='8' user='testuser8'>
                <tag k='tourism' v='hotel' />
              </node>
              <node id='9' lat='50.092012' lon='14.421678' version='1' timestamp='2023-01-09T12:00:00Z' changeset='131' uid='9' user='testuser9'>
                <tag k='natural' v='tree' />
              </node>
              <node id='10' lat='50.093345' lon='14.422901' version='2' timestamp='2023-01-10T12:00:00Z' changeset='132' uid='10' user='testuser10'>
                <tag k='historic' v='monument' />
              </node>
            </osm>
            """;

        var multipleWaysXml = """
            <?xml version='1.0' encoding='UTF-8'?>
            <osm version='0.6'>
              <way id='101'>
                <nd ref='1' />
                <nd ref='2' />
                <nd ref='3' />
              </way>
              <way id='102'>
                <nd ref='4' />
                <nd ref='5' />
                <nd ref='6' />
              </way>
              <way id='103'>
                <nd ref='7' />
                <nd ref='8' />
                <nd ref='9' />
              </way>
              <way id='104'>
                <nd ref='10' />
                <nd ref='11' />
                <nd ref='12' />
              </way>
              <way id='105'>
                <nd ref='13' />
                <nd ref='14' />
                <nd ref='15' />
              </way>
              <way id='106'>
                <nd ref='16' />
                <nd ref='17' />
                <nd ref='18' />
              </way>
              <way id='107'>
                <nd ref='19' />
                <nd ref='20' />
                <nd ref='21' />
              </way>
              <way id='108'>
                <nd ref='22' />
                <nd ref='23' />
                <nd ref='24' />
              </way>
              <way id='109'>
                <nd ref='25' />
                <nd ref='26' />
                <nd ref='27' />
              </way>
              <way id='110'>
                <nd ref='28' />
                <nd ref='29' />
                <nd ref='30' />
              </way>
            </osm>
            """;

        var multipleWaysWithTagsXml = """
            <?xml version='1.0' encoding='UTF-8'?>
            <osm version='0.6'>
              <way id='101'>
                <nd ref='1' />
                <nd ref='2' />
                <nd ref='3' />
                <tag k='highway' v='residential' />
                <tag k='name' v='Test Street 1' />
              </way>
              <way id='102'>
                <nd ref='4' />
                <nd ref='5' />
                <nd ref='6' />
                <tag k='highway' v='primary' />
                <tag k='name' v='Main Street' />
              </way>
              <way id='103'>
                <nd ref='7' />
                <nd ref='8' />
                <nd ref='9' />
                <tag k='highway' v='secondary' />
                <tag k='name' v='Oak Avenue' />
              </way>
              <way id='104'>
                <nd ref='10' />
                <nd ref='11' />
                <nd ref='12' />
                <tag k='highway' v='tertiary' />
                <tag k='name' v='Pine Road' />
              </way>
              <way id='105'>
                <nd ref='13' />
                <nd ref='14' />
                <nd ref='15' />
                <tag k='highway' v='unclassified' />
                <tag k='name' v='Elm Drive' />
              </way>
              <way id='106'>
                <nd ref='16' />
                <nd ref='17' />
                <nd ref='18' />
                <tag k='highway' v='service' />
                <tag k='name' v='Service Lane' />
              </way>
              <way id='107'>
                <nd ref='19' />
                <nd ref='20' />
                <nd ref='21' />
                <tag k='highway' v='track' />
                <tag k='name' v='Forest Trail' />
              </way>
              <way id='108'>
                <nd ref='22' />
                <nd ref='23' />
                <nd ref='24' />
                <tag k='highway' v='path' />
                <tag k='name' v='Walking Path' />
              </way>
              <way id='109'>
                <nd ref='25' />
                <nd ref='26' />
                <nd ref='27' />
                <tag k='highway' v='cycleway' />
                <tag k='name' v='Bike Lane' />
              </way>
              <way id='110'>
                <nd ref='28' />
                <nd ref='29' />
                <nd ref='30' />
                <tag k='highway' v='footway' />
                <tag k='name' v='Sidewalk' />
              </way>
            </osm>
            """;

        var multipleRelationsXml = """
            <?xml version='1.0' encoding='UTF-8'?>
            <osm version='0.6'>
              <relation id='201'>
                <member type='way' ref='101' role='outer' />
                <member type='way' ref='102' role='inner' />
              </relation>
              <relation id='202'>
                <member type='way' ref='103' role='outer' />
                <member type='way' ref='104' role='inner' />
              </relation>
              <relation id='203'>
                <member type='way' ref='105' role='outer' />
                <member type='node' ref='1' role='entrance' />
              </relation>
              <relation id='204'>
                <member type='way' ref='106' role='outer' />
                <member type='node' ref='2' role='entrance' />
              </relation>
              <relation id='205'>
                <member type='way' ref='107' role='outer' />
                <member type='node' ref='3' role='entrance' />
              </relation>
            </osm>
            """;

        var multipleRelationsWithTagsXml = """
            <?xml version='1.0' encoding='UTF-8'?>
            <osm version='0.6'>
              <relation id='201'>
                <member type='way' ref='101' role='outer' />
                <member type='way' ref='102' role='inner' />
                <tag k='type' v='multipolygon' />
                <tag k='name' v='Building Complex 1' />
              </relation>
              <relation id='202'>
                <member type='way' ref='103' role='outer' />
                <member type='way' ref='104' role='inner' />
                <tag k='type' v='multipolygon' />
                <tag k='name' v='Park Area' />
              </relation>
              <relation id='203'>
                <member type='way' ref='105' role='outer' />
                <member type='node' ref='1' role='entrance' />
                <tag k='type' v='building' />
                <tag k='name' v='Shopping Center' />
              </relation>
              <relation id='204'>
                <member type='way' ref='106' role='from' />
                <member type='way' ref='107' role='to' />
                <tag k='type' v='route' />
                <tag k='route' v='bus' />
                <tag k='name' v='Bus Route 42' />
              </relation>
              <relation id='205'>
                <member type='way' ref='108' role='from' />
                <member type='way' ref='109' role='to' />
                <tag k='type' v='route' />
                <tag k='route' v='bicycle' />
                <tag k='name' v='Cycling Route A' />
              </relation>
            </osm>
            """;

        var complexMixedXml = """
            <?xml version='1.0' encoding='UTF-8'?>
            <osm version='0.6'>
              <node id='1' lat='50.086758' lon='14.4092038' version='1' timestamp='2023-01-01T12:00:00Z' changeset='123' uid='1' user='testuser'>
                <tag k='highway' v='traffic_signals' />
                <tag k='name' v='Main Intersection' />
              </node>
              <node id='2' lat='50.0860597' lon='14.4143866'>
                <tag k='amenity' v='restaurant' />
                <tag k='name' v='Good Food' />
              </node>
              <node id='3' lat='50.0886819' lon='14.415577' />
              <node id='4' lat='50.087234' lon='14.416123'>
                <tag k='shop' v='supermarket' />
              </node>
              <node id='5' lat='50.088567' lon='14.417456' />
              <node id='6' lat='50.089123' lon='14.418789' />
              <node id='7' lat='50.090456' lon='14.419012' />
              <node id='8' lat='50.091789' lon='14.420345' />
              <node id='9' lat='50.092012' lon='14.421678' />
              <node id='10' lat='50.093345' lon='14.422901' />
              
              <way id='101'>
                <nd ref='1' />
                <nd ref='2' />
                <nd ref='3' />
                <tag k='highway' v='residential' />
                <tag k='name' v='Test Street' />
              </way>
              <way id='102'>
                <nd ref='4' />
                <nd ref='5' />
                <nd ref='6' />
                <tag k='highway' v='primary' />
                <tag k='name' v='Main Street' />
                <tag k='maxspeed' v='50' />
              </way>
              <way id='103'>
                <nd ref='7' />
                <nd ref='8' />
                <nd ref='9' />
                <nd ref='10' />
                <tag k='highway' v='secondary' />
              </way>
              <way id='104'>
                <nd ref='1' />
                <nd ref='4' />
                <nd ref='7' />
                <tag k='highway' v='tertiary' />
                <tag k='surface' v='asphalt' />
              </way>
              <way id='105'>
                <nd ref='2' />
                <nd ref='5' />
                <nd ref='8' />
              </way>
              <way id='106'>
                <nd ref='3' />
                <nd ref='6' />
                <nd ref='9' />
                <tag k='highway' v='footway' />
              </way>
              <way id='107'>
                <nd ref='1' />
                <nd ref='10' />
                <tag k='highway' v='cycleway' />
              </way>
              <way id='108'>
                <nd ref='5' />
                <nd ref='6' />
                <tag k='highway' v='path' />
              </way>
              <way id='109'>
                <nd ref='2' />
                <nd ref='9' />
                <tag k='railway' v='rail' />
              </way>
              <way id='110'>
                <nd ref='4' />
                <nd ref='8' />
                <tag k='waterway' v='river' />
                <tag k='name' v='Test River' />
              </way>
              
              <relation id='201'>
                <member type='way' ref='101' role='outer' />
                <member type='way' ref='102' role='inner' />
                <tag k='type' v='multipolygon' />
                <tag k='name' v='Building Complex' />
                <tag k='building' v='yes' />
              </relation>
              <relation id='202'>
                <member type='way' ref='103' role='from' />
                <member type='way' ref='104' role='to' />
                <member type='node' ref='1' role='via' />
                <tag k='type' v='route' />
                <tag k='route' v='bus' />
                <tag k='name' v='Bus Route 42' />
                <tag k='operator' v='City Transport' />
              </relation>
              <relation id='203'>
                <member type='relation' ref='201' role='part' />
                <member type='way' ref='105' role='boundary' />
                <tag k='type' v='site' />
                <tag k='name' v='Complex Site' />
              </relation>
              <relation id='204'>
                <member type='way' ref='106' role='platform' />
                <member type='way' ref='107' role='track' />
                <tag k='type' v='public_transport' />
                <tag k='public_transport' v='stop_area' />
              </relation>
              <relation id='205'>
                <member type='way' ref='108' role='outer' />
                <member type='way' ref='109' role='inner' />
                <member type='way' ref='110' role='water' />
                <tag k='type' v='multipolygon' />
                <tag k='natural' v='park' />
                <tag k='name' v='City Park' />
              </relation>
            </osm>
            """;

        // Initialize memory streams
        _multipleNodesStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(multipleNodesXml));
        _multipleNodesWithTagsStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(multipleNodesWithTagsXml));
        _multipleNodesWithMetadataStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(multipleNodesWithMetadataXml));
        _multipleWaysStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(multipleWaysXml));
        _multipleWaysWithTagsStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(multipleWaysWithTagsXml));
        _multipleRelationsStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(multipleRelationsXml));
        _multipleRelationsWithTagsStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(multipleRelationsWithTagsXml));
        _complexMixedStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(complexMixedXml));
    }
}
