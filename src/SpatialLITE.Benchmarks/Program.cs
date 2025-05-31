using BenchmarkDotNet.Running;
using SpatialLITE.Benchmarks.Contracts;
using SpatialLITE.Benchmarks.Core.IO;
using SpatialLITE.Benchmarks.Osm.IO;

// Run all benchmarks in the assembly
// To run specific benchmark classes, comment out the lines you don't want to run:
BenchmarkRunner.Run<EnvelopeBenchmarks>();
BenchmarkRunner.Run<WktReaderBenchmarks>();
BenchmarkRunner.Run<WktWriterBenchmarks>();
BenchmarkRunner.Run<OsmXmlReaderBenchmarks>();
BenchmarkRunner.Run<OsmXmlWriterBenchmarks>();

// Alternative: Run individual benchmark classes
// BenchmarkRunner.Run<WktReaderBenchmarks>();
// BenchmarkRunner.Run<WktWriterBenchmarks>();
// BenchmarkRunner.Run<EnvelopeBenchmarks>();
// BenchmarkRunner.Run<OsmXmlReaderBenchmarks>();
// BenchmarkRunner.Run<OsmXmlWriterBenchmarks>();
