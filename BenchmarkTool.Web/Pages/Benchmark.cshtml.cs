using BenchmarkTool.Core.Services;
using BenchmarkTool.Web.Hubs;
using BenchmarkTool.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using CoreModels = BenchmarkTool.Core.Models;

namespace BenchmarkTool.Web.Pages;

public class BenchmarkModel : PageModel
{
    private readonly ILogger<BenchmarkModel> _logger;
    private readonly IBenchmarkRunnerService _benchmarkRunner;
    private readonly IHubContext<BenchmarkHub> _hubContext;

    public BenchmarkModel(
     ILogger<BenchmarkModel> logger,
        IBenchmarkRunnerService benchmarkRunner,
        IHubContext<BenchmarkHub> hubContext)
    {
        _logger = logger;
        _benchmarkRunner = benchmarkRunner;
        _hubContext = hubContext;
    }

    [BindProperty]
    public new BenchmarkRequest Request { get; set; } = new();

    public new BenchmarkResponse? Response { get; set; }

    public void OnGet()
    {
        // Initialize with sample code
        Request.DeclarationsCode = @"// Sample Declarations
private int size;";

        Request.SetupCode = @"// Sample Setup
// Runs once before benchmarks
size = 1000;";

        Request.MethodACode = @"// Sample Method A
var list = new List<int>();
for (int i =0; i <size; i++)
{
    list.Add(i);
}";

        Request.MethodBCode = @"// Sample Method B
var array = new int[size];
for (int i =0; i <size; i++)
{
    array[i] = i;
}";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            _logger.LogInformation("Starting benchmark run for request {RunId}", Request.MethodAName);

            // Convert Web model to Core model
            var coreRequest = new CoreModels.BenchmarkRequest
            {
                DeclarationsCode = Request.DeclarationsCode,
                SetupCode = Request.SetupCode,
                MethodACode = Request.MethodACode,
                MethodBCode = Request.MethodBCode,
                MethodAName = Request.MethodAName,
                MethodBName = Request.MethodBName
            };

            // Create progress reporter that sends updates via SignalR
            var progress = new Progress<(string Message, int Percentage)>(update =>
            {
                _hubContext.Clients.All.SendAsync("ReceiveProgress", update.Message, update.Percentage);
                _logger.LogInformation("Progress: {Message} - {Percentage}%", update.Message, update.Percentage);
            });

            // Run the benchmark
            var result = await _benchmarkRunner.RunBenchmarkAsync(coreRequest, progress);

            // Convert Core result to Web response
            Response = new BenchmarkResponse
            {
                Success = result.Success,
                ErrorMessage = result.ErrorMessage,
                CompilationErrors = result.CompilationErrors.Select(e => new CompilationError
                {
                    Line = e.Line,
                    Column = e.Column,
                    Message = e.Message,
                    Severity = e.Severity,
                    ErrorCode = e.ErrorCode
                }).ToList(),
                ResultsMarkdown = result.ResultsMarkdown,
                ResultsJson = result.ResultsJson,
                RawOutput = result.RawOutput,
                ExecutionTimeMs = result.ExecutionTimeMs
            };

            // Send completion notification via SignalR
            if (result.Success)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveResults", "Benchmark completed successfully!");
            }
            else
            {
                await _hubContext.Clients.All.SendAsync("ReceiveError", result.ErrorMessage ?? "Benchmark failed");
            }

            // Clean up temporary files after a delay
            if (!string.IsNullOrEmpty(result.TempDirectory))
            {
                _ = Task.Run(async () =>
                 {
                     await Task.Delay(TimeSpan.FromSeconds(30));
                     await _benchmarkRunner.CleanupAsync(result.TempDirectory);
                 });
            }

            _logger.LogInformation("Benchmark completed. Success: {Success}, Duration: {Duration}ms",
          result.Success, result.ExecutionTimeMs);

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running benchmark");

            Response = new BenchmarkResponse
            {
                Success = false,
                ErrorMessage = $"Unexpected error: {ex.Message}"
            };

            await _hubContext.Clients.All.SendAsync("ReceiveError", ex.Message);

            return Page();
        }
    }
}
