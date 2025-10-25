using BenchmarkTool.Core.Models;
using System.Text;

namespace BenchmarkTool.Core.Services;

/// <summary>
/// Service for generating BenchmarkDotNet code from user input
/// </summary>
public class CodeGenerationService : ICodeGenerationService
{
    /// <summary>
    /// Generates a complete benchmark class from user code
    /// </summary>
    public string GenerateBenchmarkClass(BenchmarkRequest request)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine("using BenchmarkDotNet.Attributes;");
        sb.AppendLine("using BenchmarkDotNet.Running;");
        sb.AppendLine();
        sb.AppendLine("namespace BenchmarkToolGenerated;");
        sb.AppendLine();
        sb.AppendLine("[MemoryDiagnoser]");
        sb.AppendLine("[RankColumn]");
        sb.AppendLine("public class DynamicBenchmark");
        sb.AppendLine("{");

        // Global setup method if provided
        if (!string.IsNullOrWhiteSpace(request.SetupCode))
        {
            sb.AppendLine("    [GlobalSetup]");
            sb.AppendLine("    public void Setup()");
            sb.AppendLine("    {");
            sb.AppendLine(IndentCode(request.SetupCode, 8));
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        // Method A
        sb.AppendLine("    [Benchmark(Baseline = true)]");
        sb.AppendLine($"    public void {SanitizeMethodName(request.MethodAName)}()");
        sb.AppendLine("  {");
        sb.AppendLine(IndentCode(request.MethodACode, 8));
        sb.AppendLine("    }");
        sb.AppendLine();

        // Method B
        sb.AppendLine("    [Benchmark]");
        sb.AppendLine($"    public void {SanitizeMethodName(request.MethodBName)}()");
        sb.AppendLine("    {");
        sb.AppendLine(IndentCode(request.MethodBCode, 8));
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates a complete Program.cs for the benchmark runner
    /// </summary>
    public string GenerateProgramFile(BenchmarkRequest request)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using BenchmarkDotNet.Running;");
        sb.AppendLine("using BenchmarkDotNet.Configs;");
        sb.AppendLine("using BenchmarkDotNet.Exporters;");
        sb.AppendLine("using BenchmarkDotNet.Exporters.Csv;");
        sb.AppendLine("using BenchmarkDotNet.Exporters.Json;");
        sb.AppendLine();
        sb.AppendLine("namespace BenchmarkToolGenerated;");
        sb.AppendLine();
        sb.AppendLine("public class Program");
        sb.AppendLine("{");
        sb.AppendLine("    public static void Main(string[] args)");
        sb.AppendLine("    {");
        sb.AppendLine("        var config = DefaultConfig.Instance");
        sb.AppendLine("        .AddExporter(MarkdownExporter.GitHub)");
        sb.AppendLine("   .AddExporter(JsonExporter.Full)");
        sb.AppendLine("    .AddExporter(CsvExporter.Default);");
        sb.AppendLine();
        sb.AppendLine("        var summary = BenchmarkRunner.Run<DynamicBenchmark>(config);");
        sb.AppendLine("  }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Sanitizes method names to be valid C# identifiers
    /// </summary>
    private string SanitizeMethodName(string methodName)
    {
        if (string.IsNullOrWhiteSpace(methodName))
            return "Method";

        // Remove invalid characters
        var sanitized = new string(methodName
                .Where(c => char.IsLetterOrDigit(c) || c == '_')
                .ToArray());

        // Ensure it starts with a letter or underscore
        if (sanitized.Length == 0 || char.IsDigit(sanitized[0]))
            sanitized = "_" + sanitized;

        return string.IsNullOrWhiteSpace(sanitized) ? "Method" : sanitized;
    }

    /// <summary>
    /// Indents code by the specified number of spaces
    /// </summary>
    private string IndentCode(string code, int spaces)
    {
        if (string.IsNullOrWhiteSpace(code))
            return string.Empty;

        var indent = new string(' ', spaces);
        var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        return string.Join(Environment.NewLine,
            lines.Select(line => string.IsNullOrWhiteSpace(line) ? line : indent + line));
    }
}
