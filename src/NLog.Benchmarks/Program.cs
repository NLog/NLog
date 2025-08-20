using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace NLog.Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(System.Reflection.Assembly.GetExecutingAssembly()).Run(
                args,
                DefaultConfig.Instance
                    .AddJob(BenchmarkDotNet.Jobs.Job.Default.WithIterationCount(5).WithWarmupCount(1).WithEnvironmentVariables(Microsoft.VSDiagnostics.VSDiagnosticsConfigurations.GetDotNetObjectAllocEnvironmentVariables(1)).AsMutator())
                    .AddDiagnoser(new Microsoft.VSDiagnostics.DotNetObjectAllocDiagnoser())
            ); return;
#pragma warning disable CS0162 // Unreachable code detected
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}
