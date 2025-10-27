using BenchmarkTool.Core.Services;
using BenchmarkTool.Web.Hubs;
using BenchmarkTool.Web.Models;
using BenchmarkTool.Web.Services;
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
    private readonly IBenchmarkConcurrencyService _concurrency;

    public BenchmarkModel(
     ILogger<BenchmarkModel> logger,
        IBenchmarkRunnerService benchmarkRunner,
        IHubContext<BenchmarkHub> hubContext,
        IBenchmarkConcurrencyService concurrency)
    {
        _logger = logger;
        _benchmarkRunner = benchmarkRunner;
        _hubContext = hubContext;
        _concurrency = concurrency;
    }

    [BindProperty]
    public new BenchmarkRequest Request { get; set; } = new();

    [BindProperty]
    public string? ConnectionId { get; set; }

    public new BenchmarkResponse? Response { get; set; }

    public void OnGet()
    {
        // Initialize with sample code
        Request.DeclarationsCode = @"// Sample Declarations
private int size;";

        Request.SetupCode = @"// Sample Setup
// Runs once before benchmarks
size =1000;";

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
            _logger.LogInformation("Starting benchmark run for request {RunId}. ConnId={ConnId}", Request.MethodAName, ConnectionId);

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

            // Create progress reporter that sends updates via SignalR ONLY to the initiating connection
            var progress = new Progress<(string Message, int Percentage)>(update =>
            {
                if (!string.IsNullOrWhiteSpace(ConnectionId))
                {
                    _hubContext.Clients.Client(ConnectionId).SendAsync("ReceiveProgress", update.Message, update.Percentage);
                }
                _logger.LogInformation("[Conn {Conn}] Progress: {Message} - {Percentage}%", ConnectionId, update.Message, update.Percentage);
            });

            // If someone else is running, inform this client that it's queued (simple status message)
            if (!string.IsNullOrWhiteSpace(ConnectionId) && (_concurrency.IsRunning || _concurrency.WaitingCount >0))
            {
                await _hubContext.Clients.Client(ConnectionId).SendAsync("ReceiveStatus", "Another benchmark is running. You are queued and will start automatically...");
            }

            // Acquire exclusive run slot; this may wait and will report waiting status
            await using var runnerLock = await AcquireRunnerSlotAsync(progress);

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

            // Send completion notification via SignalR to ONLY this client
            if (!string.IsNullOrWhiteSpace(ConnectionId))
            {
                if (result.Success)
                {
                    await _hubContext.Clients.Client(ConnectionId).SendAsync("ReceiveResults", "Benchmark completed successfully!");
                }
                else
                {
                    await _hubContext.Clients.Client(ConnectionId).SendAsync("ReceiveError", result.ErrorMessage ?? "Benchmark failed");
                }
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

            if (!string.IsNullOrWhiteSpace(ConnectionId))
            {
                await _hubContext.Clients.Client(ConnectionId).SendAsync("ReceiveError", ex.Message);
            }

            return Page();
        }
    }

    private async ValueTask<IAsyncDisposable> AcquireRunnerSlotAsync(IProgress<(string Message, int Percentage)> progress)
    {
        // Wrap IDisposable as IAsyncDisposable for convenient await using in handler
        var releaser = await _concurrency.AcquireAsync(progress, HttpContext.RequestAborted);
        return new AsyncDisposableAdapter(releaser);
    }

    private sealed class AsyncDisposableAdapter : IAsyncDisposable
    {
        private IDisposable? _inner;
        public AsyncDisposableAdapter(IDisposable inner) => _inner = inner;
        public ValueTask DisposeAsync()
        {
            var inner = Interlocked.Exchange(ref _inner, null);
            inner?.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
