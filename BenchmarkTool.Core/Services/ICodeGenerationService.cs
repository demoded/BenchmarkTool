using BenchmarkTool.Core.Models;

namespace BenchmarkTool.Core.Services;

/// <summary>
/// Service for generating BenchmarkDotNet code from user input
/// </summary>
public interface ICodeGenerationService
{
    /// <summary>
    /// Generates a complete benchmark class from user code
    /// </summary>
    string GenerateBenchmarkClass(BenchmarkRequest request);

    /// <summary>
    /// Generates a complete Program.cs for the benchmark runner
    /// </summary>
 string GenerateProgramFile(BenchmarkRequest request);
}
