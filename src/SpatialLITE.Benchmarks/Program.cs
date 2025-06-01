using BenchmarkDotNet.Running;
using SpatialLITE.Benchmarks.Contracts;
using SpatialLITE.Benchmarks.Core.IO;
using SpatialLITE.Benchmarks.Osm.IO;

BenchmarkRunner.Run<EnvelopeBenchmarks>();
BenchmarkRunner.Run<WktReaderBenchmarks>();
BenchmarkRunner.Run<WktWriterBenchmarks>();
BenchmarkRunner.Run<OsmXmlReaderBenchmarks>();
BenchmarkRunner.Run<OsmXmlWriterBenchmarks>();
BenchmarkRunner.Run<PbfReaderBenchmarks>();
BenchmarkRunner.Run<PbfWriterBenchmarks>();
