namespace BenchmarkTool.Core.Models;

/// <summary>
/// Represents the results of a benchmark execution
/// </summary>
public class BenchmarkResult
{
    /// <summary>
    /// Indicates if the benchmark was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if benchmark failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Compilation errors if any
    /// </summary>
    public List<CompilationError> CompilationErrors { get; set; } = new();

    /// <summary>
    /// Benchmark results in markdown format
    /// </summary>
    public string? ResultsMarkdown { get; set; }

    /// <summary>
    /// Benchmark results in JSON format
    /// </summary>
    public string? ResultsJson { get; set; }

    /// <summary>
    /// Raw console output from BenchmarkDotNet
    /// </summary>
    public string? RawOutput { get; set; }

    /// <summary>
    /// Execution time in milliseconds
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Path to the temporary directory used for benchmarking
    /// </summary>
    public string? TempDirectory { get; set; }
}
