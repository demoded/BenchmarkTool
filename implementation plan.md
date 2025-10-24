## Implementation Plan: .NET Benchmark Web Application

### Architecture Overview

**Technology Stack:**
- **Frontend**: ASP.NET Core Razor Pages or Blazor Server
- **Backend**: ASP.NET Core Web API
- **Benchmarking**: BenchmarkDotNet
- **Code Compilation**: Roslyn (Microsoft.CodeAnalysis)

### Project Structure

```
BenchmarkTool/
├── BenchmarkTool.Web/           # Main web application
│   ├── Pages/                   # Razor Pages
│   ├── wwwroot/                 # Static files (CSS, JS)
│   ├── Services/                # Business logic services
│   └── Models/                  # View models and DTOs
├── BenchmarkTool.Core/          # Core benchmarking logic
│   ├── Services/
│   │   ├── CodeGenerationService.cs
│   │   ├── CompilationService.cs
│   │   └── BenchmarkRunnerService.cs
│   ├── Models/
│   └── Templates/
└── BenchmarkTool.Runner/        # Separate process for running benchmarks
    └── Program.cs
```

### Implementation Steps

#### **Phase 1: Project Setup**
1. Create ASP.NET Core Web Application (Razor Pages or Blazor)
2. Add BenchmarkDotNet NuGet package
3. Add Roslyn packages (Microsoft.CodeAnalysis.CSharp)
4. Set up project structure with separate class libraries

#### **Phase 2: Frontend Development**
1. **Main Page Components:**
   - Two code editor panels (using CodeMirror or Monaco Editor)
   - "Run Benchmark" button
   - Results display area
   - Loading/progress indicator
   
2. **Features:**
   - Syntax highlighting for C#
   - Line numbers
   - Auto-indentation
   - Error display

#### **Phase 3: Backend Services**

1. **CodeGenerationService**
   - Generate complete C# class with BenchmarkDotNet attributes
   - Create a benchmark class template
   - Insert user's methodA and methodB into the template
   
2. **CompilationService**
   - Use Roslyn to compile the generated code
   - Validate syntax and semantic errors
   - Return compilation errors to user if any
   
3. **BenchmarkRunnerService**
   - Save compiled code to temporary project
   - Execute BenchmarkDotNet in a separate process
   - Parse BenchmarkDotNet output/results
   - Return formatted results

#### **Phase 4: Benchmark Execution**

1. **Dynamic Code Generation:**
```csharp
[MemoryDiagnoser]
public class DynamicBenchmark
{
    [Benchmark]
    public void MethodA() { /* user code */ }
    
    [Benchmark]
    public void MethodB() { /* user code */ }
}
```

2. **Temporary Project Creation:**
   - Generate a temporary .csproj file
   - Create Program.cs with BenchmarkRunner
   - Copy user code into the project
   
3. **Process Execution:**
   - Run `dotnet run` in the temporary project
   - Capture console output
   - Parse BenchmarkDotNet results (markdown/JSON)

#### **Phase 5: Results Display**

1. **Parse BenchmarkDotNet Output:**
   - Extract timing results (mean, median, stddev)
   - Extract memory allocation data
   - Parse comparison statistics
   
2. **Display Results:**
   - Show results in formatted table
   - Visual charts/graphs (optional)
   - Downloadable reports

### Key Components to Implement

#### **1. Code Template**
```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]
public class DynamicBenchmark
{
    [Benchmark]
    public void MethodA()
    {
        // USER CODE HERE
    }

    [Benchmark]
    public void MethodB()
    {
        // USER CODE HERE
    }
}
```

#### **2. Main Services**

- **ICodeGenerationService**: Generates benchmark class from user input
- **ICompilationService**: Validates and compiles code using Roslyn
- **IBenchmarkRunnerService**: Executes benchmarks and returns results
- **IResultParserService**: Parses BenchmarkDotNet output

#### **3. Models/DTOs**

- **BenchmarkRequest**: Contains methodA and methodB source code
- **BenchmarkResult**: Contains timing, memory, and comparison data
- **CompilationError**: Line number, error message, severity

### Security Considerations

1. **Code Sandboxing:**
   - Run benchmarks in isolated process
   - Set execution timeout limits
   - Restrict file system access
   - Limit memory usage

2. **Input Validation:**
   - Sanitize user input
   - Restrict dangerous APIs (File I/O, Network, etc.)
   - Validate code complexity/size

3. **Resource Management:**
   - Clean up temporary files
   - Kill hanging processes
   - Implement rate limiting

### Technical Challenges & Solutions

| Challenge | Solution |
|-----------|----------|
| Long-running benchmarks | Use SignalR for real-time progress updates |
| Process isolation | Run in separate AppDomain or container |
| Result parsing | Use BenchmarkDotNet exporters (JSON/XML) |
| Code safety | Static analysis with Roslyn before execution |
| Concurrent requests | Queue system with background workers |

### Recommended NuGet Packages

```xml
<PackageReference Include="BenchmarkDotNet" Version="0.13.*" />
<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.*" />
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="8.0.*" />
```
