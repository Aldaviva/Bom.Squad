using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Bom.Squad;

namespace Performance;

internal static class PerformanceTest {

    public static void Main() {
        BenchmarkRunner.Run<Benchmarks>();
    }

}

[ShortRunJob]
public class Benchmarks {

    [IterationCleanup]
    public void Reset() {
        BomSquad.RearmUtf8Bom();
    }

    [Benchmark]
    public void Reflection() {
        BomSquad.DefuseUtf8Bom();
    }

}