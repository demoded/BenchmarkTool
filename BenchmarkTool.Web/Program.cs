using BenchmarkTool.Core.Services;
using BenchmarkTool.Web.Hubs;
using BenchmarkTool.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// Register BenchmarkTool services
builder.Services.AddScoped<ICodeGenerationService, CodeGenerationService>();
builder.Services.AddScoped<ICompilationService, CompilationService>();
builder.Services.AddScoped<IBenchmarkRunnerService, BenchmarkRunnerService>();

// Concurrency: allow only one benchmark at a time across the app
builder.Services.AddSingleton<IBenchmarkConcurrencyService, BenchmarkConcurrencyService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// Map SignalR hub
app.MapHub<BenchmarkHub>("/benchmarkHub");

app.Run();
