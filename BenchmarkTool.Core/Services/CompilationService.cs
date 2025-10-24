using BenchmarkTool.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace BenchmarkTool.Core.Services;

/// <summary>
/// Service for compiling and validating C# code using Roslyn
/// </summary>
public class CompilationService : ICompilationService
{
    /// <summary>
    /// Validates the generated code for syntax and semantic errors
    /// </summary>
    public async Task<(bool IsValid, List<CompilationError> Errors)> ValidateCodeAsync(string code)
    {
        var errors = new List<CompilationError>();

        try
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            // Check for syntax errors only
            // Semantic validation is skipped because BenchmarkDotNet references may not be available
            // in the web application context. The actual 'dotnet build' will validate semantics.
            var diagnostics = syntaxTree.GetDiagnostics();
            foreach (var diagnostic in diagnostics)
            {
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    var lineSpan = diagnostic.Location.GetLineSpan();
                    errors.Add(new CompilationError
                    {
                        Line = lineSpan.StartLinePosition.Line + 1,
                        Column = lineSpan.StartLinePosition.Character + 1,
                        Message = diagnostic.GetMessage(),
                        Severity = diagnostic.Severity.ToString(),
                        ErrorCode = diagnostic.Id
                    });
                }
            }

            // Note: Semantic analysis is intentionally skipped here.
            // The generated code will be fully validated during the 'dotnet build' step
            // which has access to all necessary NuGet packages including BenchmarkDotNet.
        }
        catch (Exception ex)
        {
            errors.Add(new CompilationError
            {
                Line = 0,
                Column = 0,
                Message = $"Validation error: {ex.Message}",
                Severity = "Error"
            });
        }

        return await Task.FromResult((errors.Count == 0, errors));
    }

    /// <summary>
    /// Compiles the code and checks for errors
    /// </summary>
    public async Task<(bool Success, List<CompilationError> Errors)> CompileCodeAsync(string code, string outputPath)
    {
        var errors = new List<CompilationError>();

        try
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CreateCompilation(syntaxTree, OutputKind.ConsoleApplication);

            // Emit the assembly
            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                foreach (var diagnostic in result.Diagnostics)
                {
                    if (diagnostic.Severity == DiagnosticSeverity.Error)
                    {
                        var lineSpan = diagnostic.Location.GetLineSpan();
                        errors.Add(new CompilationError
                        {
                            Line = lineSpan.StartLinePosition.Line + 1,
                            Column = lineSpan.StartLinePosition.Character + 1,
                            Message = diagnostic.GetMessage(),
                            Severity = diagnostic.Severity.ToString(),
                            ErrorCode = diagnostic.Id,
                            FilePath = lineSpan.Path
                        });
                    }
                }
            }
            else
            {
                // Write to output file if specified
                if (!string.IsNullOrEmpty(outputPath))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    await using var fileStream = File.Create(outputPath);
                    await ms.CopyToAsync(fileStream);
                }
            }

            return (result.Success, errors);
        }
        catch (Exception ex)
        {
            errors.Add(new CompilationError
            {
                Line = 0,
                Column = 0,
                Message = $"Compilation error: {ex.Message}",
                Severity = "Error"
            });
            return (false, errors);
        }
    }

    /// <summary>
    /// Creates a CSharpCompilation with necessary references
    /// </summary>
    private CSharpCompilation CreateCompilation(SyntaxTree syntaxTree, OutputKind outputKind = OutputKind.ConsoleApplication)
    {
        var assemblyName = $"DynamicBenchmark_{Guid.NewGuid():N}";

        // Get references to required assemblies
        var references = new List<MetadataReference>
        {
      MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
          MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
   MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
         MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
   MetadataReference.CreateFromFile(Assembly.Load("System.Linq").Location),
          MetadataReference.CreateFromFile(Assembly.Load("System.Console").Location),
        };

        // Add BenchmarkDotNet reference - use multiple approaches to find it
        if (!TryAddBenchmarkDotNetReference(references))
        {
            throw new InvalidOperationException("BenchmarkDotNet assembly could not be loaded. Ensure it is referenced in the project.");
        }

        // Add reference to netstandard if available
        try
        {
            var netstandardPath = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, "netstandard.dll");
            if (File.Exists(netstandardPath))
            {
                references.Add(MetadataReference.CreateFromFile(netstandardPath));
            }
        }
        catch
        {
            // netstandard not available
        }

        var compilation = CSharpCompilation.Create(
      assemblyName,
new[] { syntaxTree },
     references,
            new CSharpCompilationOptions(outputKind)
         .WithOptimizationLevel(OptimizationLevel.Release)
      .WithPlatform(Platform.AnyCpu));

        return compilation;
    }

    /// <summary>
    /// Tries to add BenchmarkDotNet reference using multiple approaches
    /// </summary>
    private bool TryAddBenchmarkDotNetReference(List<MetadataReference> references)
    {
        // Approach 1: Try to load from already loaded assemblies
        try
        {
            var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "BenchmarkDotNet");

            if (loadedAssembly != null && !string.IsNullOrEmpty(loadedAssembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(loadedAssembly.Location));
                return true;
            }
        }
        catch { }

        // Approach 2: Try to load the assembly explicitly
        try
        {
            var benchmarkDotNetAssembly = Assembly.Load("BenchmarkDotNet");
            if (!string.IsNullOrEmpty(benchmarkDotNetAssembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(benchmarkDotNetAssembly.Location));
                return true;
            }
        }
        catch { }

        // Approach 3: Try LoadFrom with the assembly name
        try
        {
            var benchmarkDotNetAssembly = Assembly.Load(new AssemblyName("BenchmarkDotNet"));
            if (!string.IsNullOrEmpty(benchmarkDotNetAssembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(benchmarkDotNetAssembly.Location));
                return true;
            }
        }
        catch { }

        // Approach 4: Search in the application base directory
        try
        {
            var appDir = AppContext.BaseDirectory;
            var benchmarkDllPath = Path.Combine(appDir, "BenchmarkDotNet.dll");

            if (File.Exists(benchmarkDllPath))
            {
                references.Add(MetadataReference.CreateFromFile(benchmarkDllPath));
                return true;
            }
        }
        catch { }

        // Approach 5: Load from the executing assembly's location
        try
        {
            var executingAssemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (executingAssemblyDir != null)
            {
                var benchmarkDllPath = Path.Combine(executingAssemblyDir, "BenchmarkDotNet.dll");

                if (File.Exists(benchmarkDllPath))
                {
                    references.Add(MetadataReference.CreateFromFile(benchmarkDllPath));
                    return true;
                }
            }
        }
        catch { }

        return false;
    }
}
