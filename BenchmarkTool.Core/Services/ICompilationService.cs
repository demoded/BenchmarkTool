using BenchmarkTool.Core.Models;

namespace BenchmarkTool.Core.Services;

/// <summary>
/// Service for compiling and validating C# code using Roslyn
/// </summary>
public interface ICompilationService
{
    /// <summary>
    /// Validates the generated code for syntax and semantic errors
    /// </summary>
    Task<(bool IsValid, List<CompilationError> Errors)> ValidateCodeAsync(string code);

    /// <summary>
    /// Compiles the code and checks for errors
    /// </summary>
    Task<(bool Success, List<CompilationError> Errors)> CompileCodeAsync(string code, string outputPath);
}
