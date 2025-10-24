using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters.Json;

namespace BenchmarkRunner;

/// <summary>
/// Entry point for the BenchmarkTool Runner
/// This is a standalone project that can be used to test benchmark execution
/// In production, benchmarks are generated and run dynamically by BenchmarkRunnerService
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("BenchmarkTool Runner");
        Console.WriteLine("====================");
        Console.WriteLine();
        Console.WriteLine("This is a standalone runner for testing purposes.");
        Console.WriteLine("Actual benchmarks are executed dynamically by the web application.");
        Console.WriteLine();

        // Example: Run a sample benchmark if one is provided
        // In practice, the BenchmarkRunnerService generates projects on the fly

        if (args.Length > 0 && args[0] == "--test")
        {
            Console.WriteLine("Running sample benchmark...");
            RunSampleBenchmark();
        }
        else
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  BenchmarkTool.Runner --test    Run a sample benchmark");
            Console.WriteLine();
            Console.WriteLine("To run custom benchmarks, use the web application at https://localhost:5001");
        }
    }

    private static void RunSampleBenchmark()
    {
        // Sample benchmark class for testing
        var config = DefaultConfig.Instance
               .AddExporter(MarkdownExporter.GitHub)
    .AddExporter(JsonExporter.Full)
    .AddExporter(CsvExporter.Default);

        // In a real scenario, this would be a dynamically generated class
        Console.WriteLine("Note: This runner is a placeholder.");
        Console.WriteLine("Dynamic benchmarks are created by BenchmarkRunnerService at runtime.");
    }
}
