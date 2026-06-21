using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

var artifactsPath = Path.GetFullPath(Path.Combine(
    AppContext.BaseDirectory,
    "..",
    "..",
    "..",
    "obj",
    "BenchmarkDotNet.Artifacts"));

var config = ManualConfig.CreateMinimumViable()
    .WithOptions(ConfigOptions.DisableLogFile)
    .WithArtifactsPath(artifactsPath);

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
