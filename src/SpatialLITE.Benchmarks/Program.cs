using BenchmarkDotNet.Running;
using SpatialLITE.Benchmarks.Contracts;

// Run all benchmarks in the assembly
BenchmarkRunner.Run<EnvelopeBenchmarks>();
