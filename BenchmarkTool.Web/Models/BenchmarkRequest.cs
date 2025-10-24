namespace BenchmarkTool.Web.Models;

/// <summary>
/// Represents a request to run benchmarks on two code methods
/// </summary>
public class BenchmarkRequest
{
    /// <summary>
    /// Source code for Method A
    /// </summary>
    public string MethodACode { get; set; } = string.Empty;

    /// <summary>
    /// Source code for Method B
    /// </summary>
    public string MethodBCode { get; set; } = string.Empty;

    /// <summary>
    /// Optional name for Method A
    /// </summary>
    public string MethodAName { get; set; } = "MethodA";

    /// <summary>
    /// Optional name for Method B
    /// </summary>
    public string MethodBName { get; set; } = "MethodB";
}
