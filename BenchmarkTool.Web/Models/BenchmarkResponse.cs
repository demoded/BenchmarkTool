namespace BenchmarkTool.Web.Models;

/// <summary>
/// Represents the results of a benchmark execution
/// </summary>
public class BenchmarkResponse
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
}

/// <summary>
/// Represents a compilation error
/// </summary>
public class CompilationError
{
    /// <summary>
    /// Line number where error occurred
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// Column number where error occurred
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Error severity (Error, Warning, Info)
    /// </summary>
    public string Severity { get; set; } = "Error";

    /// <summary>
    /// Error code (e.g., CS0103)
    /// </summary>
    public string? ErrorCode { get; set; }
}
