using BenchmarkTool.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
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
  
   // Check for syntax errors
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

          // Perform semantic analysis
    var compilation = CreateCompilation(syntaxTree);
            var semanticDiagnostics = compilation.GetDiagnostics();
       
       foreach (var diagnostic in semanticDiagnostics)
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
  var compilation = CreateCompilation(syntaxTree);

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
    private CSharpCompilation CreateCompilation(SyntaxTree syntaxTree)
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

        // Add BenchmarkDotNet reference
        try
   {
       var benchmarkDotNetAssembly = Assembly.Load("BenchmarkDotNet");
            references.Add(MetadataReference.CreateFromFile(benchmarkDotNetAssembly.Location));
        }
        catch
        {
            // BenchmarkDotNet not loaded yet, will be available at runtime
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
         new CSharpCompilationOptions(OutputKind.ConsoleApplication)
    .WithOptimizationLevel(OptimizationLevel.Release)
      .WithPlatform(Platform.AnyCpu));

        return compilation;
    }
}
