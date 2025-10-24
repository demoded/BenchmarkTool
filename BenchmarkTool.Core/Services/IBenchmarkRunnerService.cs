using BenchmarkTool.Core.Models;

namespace BenchmarkTool.Core.Services;

/// <summary>
/// Service for executing benchmarks in a separate process
/// </summary>
public interface IBenchmarkRunnerService
{
    /// <summary>
    /// Runs the benchmark for the given request
    /// </summary>
    Task<BenchmarkResult> RunBenchmarkAsync(BenchmarkRequest request, IProgress<(string Message, int Percentage)>? progress = null);

    /// <summary>
    /// Cleans up temporary files from a benchmark run
    /// </summary>
    Task CleanupAsync(string tempDirectory);
}
