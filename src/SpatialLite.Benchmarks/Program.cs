#pragma warning disable IDE0005

using BenchmarkDotNet.Running;
using SpatialLite.Benchmarks.Contracts;
using SpatialLite.Benchmarks.Core.Algorithms;
using SpatialLite.Benchmarks.Core.IO;
using SpatialLite.Benchmarks.Osm.IO;

#pragma warning restore IDE0005

//BenchmarkRunner.Run<EnvelopeBenchmarks>();
BenchmarkRunner.Run<EuclideanDistanceCalculatorBenchmarks>();
//BenchmarkRunner.Run<WktReaderBenchmarks>();
//BenchmarkRunner.Run<WktWriterBenchmarks>();
//BenchmarkRunner.Run<OsmXmlReaderBenchmarks>();
//BenchmarkRunner.Run<OsmXmlWriterBenchmarks>();
//BenchmarkRunner.Run<PbfReaderBenchmarks>();
//BenchmarkRunner.Run<PbfWriterBenchmarks>();
