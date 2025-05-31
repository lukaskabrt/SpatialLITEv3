using BenchmarkDotNet.Attributes;
using SpatialLite.Osm;
using SpatialLITE.Osm.IO.Xml;

namespace SpatialLITE.Benchmarks.Osm.IO;

[MemoryDiagnoser]
public class OsmXmlReaderBenchmarks
{
    private readonly string _simpleNodeXml = """
        <?xml version='1.0' encoding='UTF-8'?>
        <osm version='0.6'>
          <node id='247748' lat='50.086758' lon='14.4092038' />
        </osm>
        """;

    private readonly string _nodeWithTagsXml = """
        <?xml version='1.0' encoding='UTF-8'?>
        <osm version='0.6'>
          <node id='247749' lat='50.0860597' lon='14.4143866'>
            <tag k='highway' v='traffic_signals' />
            <tag k='name' v='Test Node' />
          </node>
        </osm>
        """;

    private readonly string _nodeWithMetadataXml = """
        <?xml version='1.0' encoding='UTF-8'?>
        <osm version='0.6'>
          <node id='247748' timestamp='2008-09-25T14:04:49Z' uid='17615' user='Test User' visible='true' version='5' changeset='695161' lat='50.086758' lon='14.4092038' />
        </osm>
        """;

    private readonly string _simpleWayXml = """
        <?xml version='1.0' encoding='UTF-8'?>
        <osm version='0.6'>
          <way id='4084656'>
            <nd ref='247748' />
            <nd ref='247749' />
            <nd ref='247751' />
          </way>
        </osm>
        """;

    private readonly string _wayWithTagsXml = """
        <?xml version='1.0' encoding='UTF-8'?>
        <osm version='0.6'>
          <way id='4084656'>
            <nd ref='247748' />
            <nd ref='247749' />
            <nd ref='247751' />
            <tag k='highway' v='primary' />
            <tag k='name' v='Test Way' />
          </way>
        </osm>
        """;

    private readonly string _simpleRelationXml = """
        <?xml version='1.0' encoding='UTF-8'?>
        <osm version='0.6'>
          <relation id='1'>
            <member type='way' ref='4084656' role='outer' />
            <member type='node' ref='247748' role='' />
          </relation>
        </osm>
        """;

    private readonly string _relationWithTagsXml = """
        <?xml version='1.0' encoding='UTF-8'?>
        <osm version='0.6'>
          <relation id='1'>
            <member type='way' ref='4084656' role='outer' />
            <member type='way' ref='4084657' role='inner' />
            <member type='node' ref='247748' role='' />
            <tag k='type' v='multipolygon' />
            <tag k='name' v='Test Relation' />
          </relation>
        </osm>
        """;

    private readonly string _complexMixedXml = """
        <?xml version='1.0' encoding='UTF-8'?>
        <osm version='0.6'>
          <node id='1' lat='50.086758' lon='14.4092038'>
            <tag k='highway' v='traffic_signals' />
          </node>
          <node id='2' lat='50.0860597' lon='14.4143866' />
          <node id='3' lat='50.0886819' lon='14.415577' />
          <way id='100'>
            <nd ref='1' />
            <nd ref='2' />
            <nd ref='3' />
            <tag k='highway' v='primary' />
          </way>
          <relation id='200'>
            <member type='way' ref='100' role='outer' />
            <member type='node' ref='1' role='entrance' />
            <tag k='type' v='multipolygon' />
          </relation>
        </osm>
        """;

    [Benchmark]
    public Node? ReadSimpleNode()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_simpleNodeXml));
        using var reader = new OsmXmlReader(stream, new OsmXmlReaderSettings { ReadMetadata = false });
        return reader.Read() as Node;
    }

    [Benchmark]
    public Node? ReadNodeWithTags()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_nodeWithTagsXml));
        using var reader = new OsmXmlReader(stream, new OsmXmlReaderSettings { ReadMetadata = false });
        return reader.Read() as Node;
    }

    [Benchmark]
    public Node? ReadNodeWithMetadata()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_nodeWithMetadataXml));
        using var reader = new OsmXmlReader(stream, new OsmXmlReaderSettings { ReadMetadata = true });
        return reader.Read() as Node;
    }

    [Benchmark]
    public Way? ReadSimpleWay()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_simpleWayXml));
        using var reader = new OsmXmlReader(stream, new OsmXmlReaderSettings { ReadMetadata = false });
        return reader.Read() as Way;
    }

    [Benchmark]
    public Way? ReadWayWithTags()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_wayWithTagsXml));
        using var reader = new OsmXmlReader(stream, new OsmXmlReaderSettings { ReadMetadata = false });
        return reader.Read() as Way;
    }

    [Benchmark]
    public Relation? ReadSimpleRelation()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_simpleRelationXml));
        using var reader = new OsmXmlReader(stream, new OsmXmlReaderSettings { ReadMetadata = false });
        return reader.Read() as Relation;
    }

    [Benchmark]
    public Relation? ReadRelationWithTags()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_relationWithTagsXml));
        using var reader = new OsmXmlReader(stream, new OsmXmlReaderSettings { ReadMetadata = false });
        return reader.Read() as Relation;
    }

    [Benchmark]
    public IOsmEntity? ReadComplexMixedData()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_complexMixedXml));
        using var reader = new OsmXmlReader(stream, new OsmXmlReaderSettings { ReadMetadata = false });
        IOsmEntity? lastEntity = null;
        IOsmEntity? entity;
        while ((entity = reader.Read()) != null)
        {
            lastEntity = entity;
        }

        return lastEntity;
    }
}
