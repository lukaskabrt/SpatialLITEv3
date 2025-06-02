using BenchmarkDotNet.Running;
using SpatialLite.Benchmarks.Contracts;
using SpatialLite.Benchmarks.Core.IO;
using SpatialLite.Benchmarks.Osm.IO;

BenchmarkRunner.Run<EnvelopeBenchmarks>();
BenchmarkRunner.Run<WktReaderBenchmarks>();
BenchmarkRunner.Run<WktWriterBenchmarks>();
BenchmarkRunner.Run<OsmXmlReaderBenchmarks>();
BenchmarkRunner.Run<OsmXmlWriterBenchmarks>();
BenchmarkRunner.Run<PbfReaderBenchmarks>();
BenchmarkRunner.Run<PbfWriterBenchmarks>();
