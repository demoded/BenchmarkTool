namespace BenchmarkTool.Core.Models;

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

    /// <summary>
    /// Source file path where error occurred
    /// </summary>
    public string? FilePath { get; set; }
}
