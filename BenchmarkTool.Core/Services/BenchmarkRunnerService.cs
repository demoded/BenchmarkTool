using BenchmarkTool.Core.Models;
using System.Diagnostics;
using System.Text;

namespace BenchmarkTool.Core.Services;

/// <summary>
/// Service for executing benchmarks in a separate process
/// </summary>
public class BenchmarkRunnerService : IBenchmarkRunnerService
{
    private readonly ICodeGenerationService _codeGenerator;
    private readonly ICompilationService _compilationService;

    public BenchmarkRunnerService(
            ICodeGenerationService codeGenerator,
          ICompilationService compilationService)
    {
        _codeGenerator = codeGenerator;
        _compilationService = compilationService;
    }

    /// <summary>
    /// Runs the benchmark for the given request
    /// </summary>
    public async Task<BenchmarkResult> RunBenchmarkAsync(BenchmarkRequest request,
        IProgress<(string Message, int Percentage)>? progress = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new BenchmarkResult();

        try
        {
            progress?.Report(("Generating benchmark code...", 10));

            // Generate the benchmark code
            var benchmarkCode = _codeGenerator.GenerateBenchmarkClass(request);
            var programCode = _codeGenerator.GenerateProgramFile(request);

            progress?.Report(("Validating code...", 20));

            // Validate the generated code
            var (isValid, validationErrors) = await _compilationService.ValidateCodeAsync(benchmarkCode);

            if (!isValid)
            {
                result.Success = false;
                result.ErrorMessage = "Code validation failed. Please check your code for errors.";
                result.CompilationErrors = validationErrors;
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }

            progress?.Report(("Creating temporary project...", 30));

            // Create temporary directory
            var tempDir = Path.Combine(Path.GetTempPath(), $"BenchmarkTool_{request.RunId}");
            Directory.CreateDirectory(tempDir);
            result.TempDirectory = tempDir;

            // Create project files
            await CreateProjectFilesAsync(tempDir, benchmarkCode, programCode);

            progress?.Report(("Restoring NuGet packages...", 40));

            // Restore packages
            var restoreSuccess = await RestorePackagesAsync(tempDir, progress);
            if (!restoreSuccess)
            {
                result.Success = false;
                result.ErrorMessage = "Failed to restore NuGet packages.";
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }

            progress?.Report(("Building project...", 50));

            // Build the project
            var (buildSuccess, buildOutput) = await BuildProjectAsync(tempDir, progress);
            if (!buildSuccess)
            {
                result.Success = false;
                result.ErrorMessage = "Failed to build the benchmark project.";
                result.RawOutput = buildOutput;
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }

            progress?.Report(("Running benchmarks (this may take a while)...", 60));

            // Run the benchmarks
            var (runSuccess, benchmarkOutput) = await RunBenchmarkProcessAsync(tempDir, progress);

            if (!runSuccess)
            {
                result.Success = false;
                result.ErrorMessage = "Benchmark execution failed.";
                result.RawOutput = benchmarkOutput;
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }

            progress?.Report(("Parsing results...", 90));

            // Parse results
            result.RawOutput = benchmarkOutput;
            result.ResultsMarkdown = await ParseMarkdownResultsAsync(tempDir);
            result.ResultsJson = await ParseJsonResultsAsync(tempDir);
            result.Success = true;
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

            progress?.Report(("Benchmark complete!", 100));

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Unexpected error: {ex.Message}";
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            return result;
        }
    }

    /// <summary>
    /// Cleans up temporary files from a benchmark run
    /// </summary>
    public async Task CleanupAsync(string tempDirectory)
    {
        try
        {
            if (Directory.Exists(tempDirectory))
            {
                await Task.Run(() =>
                 {
                     // Wait a bit to ensure all file handles are released
                     Thread.Sleep(1000);
                     Directory.Delete(tempDirectory, true);
                 });
            }
        }
        catch
        {
            // Cleanup failed, but don't throw - temp files will be cleaned up eventually
        }
    }

    /// <summary>
    /// Creates the project files in the temporary directory
    /// </summary>
    private async Task CreateProjectFilesAsync(string tempDir, string benchmarkCode, string programCode)
    {
        // Create .csproj file
        var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""BenchmarkDotNet"" Version=""0.15.4"" />
  </ItemGroup>
</Project>";

        await File.WriteAllTextAsync(Path.Combine(tempDir, "BenchmarkRunner.csproj"), csprojContent);

        // Create Program.cs
        await File.WriteAllTextAsync(Path.Combine(tempDir, "Program.cs"), programCode);

        // Create DynamicBenchmark.cs
        await File.WriteAllTextAsync(Path.Combine(tempDir, "DynamicBenchmark.cs"), benchmarkCode);
    }

    /// <summary>
    /// Restores NuGet packages for the project
    /// </summary>
    private async Task<bool> RestorePackagesAsync(string tempDir, IProgress<(string Message, int Percentage)>? progress)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "restore",
                WorkingDirectory = tempDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return false;

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Builds the benchmark project
    /// </summary>
    private async Task<(bool Success, string Output)> BuildProjectAsync(string tempDir, IProgress<(string Message, int Percentage)>? progress)
    {
        try
        {
            var output = new StringBuilder();

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "build -c Release",
                WorkingDirectory = tempDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return (false, "Failed to start build process");

            process.OutputDataReceived += (sender, e) => { if (e.Data != null) output.AppendLine(e.Data); };
            process.ErrorDataReceived += (sender, e) => { if (e.Data != null) output.AppendLine(e.Data); };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            return (process.ExitCode == 0, output.ToString());
        }
        catch (Exception ex)
        {
            return (false, $"Build error: {ex.Message}");
        }
    }

    /// <summary>
    /// Runs the benchmark process
    /// </summary>
    private async Task<(bool Success, string Output)> RunBenchmarkProcessAsync(string tempDir, IProgress<(string Message, int Percentage)>? progress)
    {
        try
        {
            var output = new StringBuilder();
            var binPath = Path.Combine(tempDir, "bin", "Release", "net10.0", "BenchmarkRunner.dll");

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{binPath}\"",
                WorkingDirectory = tempDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return (false, "Failed to start benchmark process");

            var progressUpdate = 60;
            process.OutputDataReceived += (sender, e) =>
           {
               if (e.Data != null)
               {
                   output.AppendLine(e.Data);

                   // Update progress based on benchmark stages
                   if (e.Data.Contains("Benchmark Process"))
                       progress?.Report(("Warming up...", Math.Min(progressUpdate++, 85)));
                   else if (e.Data.Contains("WorkloadActual"))
                       progress?.Report(("Running measurements...", Math.Min(progressUpdate++, 85)));
               }
           };

            process.ErrorDataReceived += (sender, e) => { if (e.Data != null) output.AppendLine(e.Data); };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Set a timeout of 5 minutes
            var completed = await process.WaitForExitAsync(TimeSpan.FromMinutes(5));

            if (!completed)
            {
                process.Kill();
                return (false, "Benchmark execution timed out after 5 minutes.");
            }

            return (process.ExitCode == 0, output.ToString());
        }
        catch (Exception ex)
        {
            return (false, $"Benchmark execution error: {ex.Message}");
        }
    }

    /// <summary>
    /// Parses markdown results from BenchmarkDotNet output
    /// </summary>
    private async Task<string?> ParseMarkdownResultsAsync(string tempDir)
    {
        try
        {
            var resultsDir = Path.Combine(tempDir, "BenchmarkDotNet.Artifacts", "results");
            if (!Directory.Exists(resultsDir))
                return null;

            var mdFiles = Directory.GetFiles(resultsDir, "*-report-github.md");
            if (mdFiles.Length == 0)
                return null;

            return await File.ReadAllTextAsync(mdFiles[0]);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses JSON results from BenchmarkDotNet output
    /// </summary>
    private async Task<string?> ParseJsonResultsAsync(string tempDir)
    {
        try
        {
            var resultsDir = Path.Combine(tempDir, "BenchmarkDotNet.Artifacts", "results");
            if (!Directory.Exists(resultsDir))
                return null;

            var jsonFiles = Directory.GetFiles(resultsDir, "*-report-full.json");
            if (jsonFiles.Length == 0)
                return null;

            return await File.ReadAllTextAsync(jsonFiles[0]);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Extension method for WaitForExitAsync with timeout
/// </summary>
public static class ProcessExtensions
{
    public static async Task<bool> WaitForExitAsync(this Process process, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(cts.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
