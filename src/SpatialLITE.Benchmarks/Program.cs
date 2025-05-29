using BenchmarkDotNet.Running;
using SpatialLITE.Benchmarks.Contracts;
using SpatialLITE.Benchmarks.Core.IO;

// Run all benchmarks in the assembly
BenchmarkRunner.Run<EnvelopeBenchmarks>();
BenchmarkRunner.Run<WktReaderBenchmarks>();
BenchmarkRunner.Run<WktWriterBenchmarks>();
