# BenchmarkTool Project Structure

## Phase 1: Project Setup - COMPLETED ?
## Phase 2: Frontend Development - COMPLETED ?
## Phase 3: Backend Services - COMPLETED ?

---

## Project Overview

**BenchmarkTool** is a web-based C# benchmarking application that allows users to compare the performance of two code snippets side-by-side using BenchmarkDotNet.

### Technology Stack
- **Frontend**: ASP.NET Core Razor Pages, Monaco Editor, SignalR, Bootstrap 5
- **Backend**: ASP.NET Core, Roslyn (Microsoft.CodeAnalysis), BenchmarkDotNet
- **Framework**: .NET 10.0

---

## Solution Structure

```
BenchmarkTool/
??? BenchmarkTool.sln   # Solution file
??? implementation plan.md       # Original implementation plan
??? PROJECT_STRUCTURE.md    # This file
??? PHASE1_COMPLETE.md     # Phase 1 documentation
??? PHASE2_COMPLETE.md          # Phase 2 documentation
??? PHASE3_COMPLETE.md   # Phase 3 documentation ? NEW
?
??? BenchmarkTool.Web/# Main web application ?
?   ??? Pages/
? ?   ??? Index.cshtml ?           # Welcome page
??   ??? Benchmark.cshtml ?  # Main benchmark page
?   ???? Benchmark.cshtml.cs ?   # Page model with service integration ? UPDATED
?   ??? wwwroot/
??   ??? css/
?   ?     ??? site.css ?          # Custom styles
?   ??? Hubs/
?   ?   ??? BenchmarkHub.cs ?     # SignalR hub
?   ??? Services/           # Future: Additional services
?   ??? Models/
?   ?   ??? BenchmarkRequest.cs ?   # Request DTO
?   ?   ??? BenchmarkResponse.cs ?  # Response DTO
?   ??? Program.cs ? ? UPDATED    # DI registration with Core services
?   ??? BenchmarkTool.Web.csproj
?
??? BenchmarkTool.Core/    # Core benchmarking logic ? ? IMPLEMENTED
?   ??? Services/ ?
?   ?   ??? ICodeGenerationService.cs ? ? NEW
?   ?   ??? CodeGenerationService.cs ? ? NEW
?   ?   ??? ICompilationService.cs ? ? NEW
?   ?   ??? CompilationService.cs ? ? NEW
?   ?   ??? IBenchmarkRunnerService.cs ? ? NEW
??   ??? BenchmarkRunnerService.cs ? ? NEW
?   ??? Models/ ?
?   ?   ??? BenchmarkRequest.cs ? ? NEW
??   ??? BenchmarkResult.cs ? ? NEW
?   ?   ??? CompilationError.cs ? ? NEW
?   ??? Templates/       # Future: Code templates
?   ??? BenchmarkTool.Core.csproj
?
??? BenchmarkTool.Runner/        # Benchmark runner ?
    ??? Program.cs ? ? UPDATED      # Standalone runner (reference)
    ??? BenchmarkTool.Runner.csproj
```

? = Implemented  
? = New or Updated in Phase 3

---

## NuGet Packages

| Package | Version | Project | Purpose |
|---------|---------|---------|---------|
| BenchmarkDotNet | 0.15.4 | Core, Runner | Performance benchmarking framework |
| Microsoft.CodeAnalysis.CSharp | 4.14.0 | Core | Roslyn compiler for code validation/compilation |
| Microsoft.AspNetCore.SignalR | 1.2.0 | Web | Real-time progress updates |
| Bootstrap | 5.x | Web (CDN) | Responsive UI framework |
| Monaco Editor | 0.44.0 | Web (CDN) | Code editor with IntelliSense |

---

## Phase Summary

### Phase 1: Project Setup ?
- Created 3-project solution structure
- Added NuGet packages
- Configured build system
- **Status**: Complete

### Phase 2: Frontend Development ?
- Implemented Monaco Editor integration
- Created SignalR hub for real-time updates
- Built responsive UI with Bootstrap
- Created Benchmark page with two code editors
- Implemented progress tracking UI
- **Status**: Complete

### Phase 3: Backend Services ?
- **CodeGenerationService**: Generates BenchmarkDotNet code from user input
- **CompilationService**: Validates and compiles code using Roslyn
- **BenchmarkRunnerService**: Executes benchmarks in isolated process
- Integrated services with Web project
- Implemented SignalR progress reporting
- Added automatic cleanup
- **Status**: Complete

---

## How It Works

### Architecture Flow

```
????????????????????????????????????????????????????????????
?  User Interface (Monaco Editors)   ?
?  - Edit Method A and Method B    ?
????????????????????????????????????????????????????????????
       ?
    ?
????????????????????????????????????????????????????????????
?  Benchmark Page Model (Benchmark.cshtml.cs)         ?
?  - Receives form submission  ?
?  - Calls IBenchmarkRunnerService        ?
?  - Reports progress via SignalR       ?
????????????????????????????????????????????????????????????
                ?
               ?
????????????????????????????????????????????????????????????
?  BenchmarkRunnerService      ?
?  1. Generate code (CodeGenerationService)                ?
?  2. Validate code (CompilationService)         ?
?  3. Create temporary project        ?
?  4. Restore NuGet packages (dotnet restore)      ?
?  5. Build project (dotnet build)        ?
?  6. Run benchmarks (dotnet run)  ?
?  7. Parse results (Markdown, JSON)     ?
?  8. Return BenchmarkResult    ?
????????????????????????????????????????????????????????????
       ?
            ?
????????????????????????????????????????????????????????????
?  Results Display         ?
?  - Success: Show benchmark table (timing, memory)        ?
?  - Error: Show compilation errors with line numbers      ?
????????????????????????????????????????????????????????????
```

### Execution Pipeline

1. **User Input**: User edits code in Monaco editors
2. **Submit**: Form posts to `OnPostAsync()`
3. **Generate**: Create BenchmarkDotNet class and Program.cs
4. **Validate**: Use Roslyn to check for syntax/semantic errors
5. **Project**: Create temp directory with .csproj and code files
6. **Restore**: Run `dotnet restore` to get BenchmarkDotNet package
7. **Build**: Compile project with `dotnet build -c Release`
8. **Execute**: Run benchmarks with `dotnet run` (5-minute timeout)
9. **Parse**: Extract markdown and JSON results
10. **Display**: Show formatted results in UI
11. **Cleanup**: Delete temp files after 30 seconds

---

## Key Features

### ? Implemented Features

#### Code Editing
- Monaco Editor with C# syntax highlighting
- IntelliSense and auto-completion
- Dark theme
- Line numbers
- Side-by-side comparison

#### Real-Time Updates
- SignalR integration for live progress
- Progress bar with percentage (0-100%)
- Status messages at each stage
- Error notifications

#### Code Generation
- Automatic BenchmarkDotNet class generation
- Method name sanitization
- Proper indentation
- Using statements
- Baseline marking

#### Compilation & Validation
- Roslyn-based syntax checking
- Semantic analysis
- Detailed error messages with line/column
- Error code identification

#### Benchmark Execution
- Isolated process execution
- NuGet package restoration
- Release mode compilation
- Memory diagnostics
- Multiple export formats (Markdown, JSON, CSV)
- 5-minute timeout protection

#### Results Display
- Formatted benchmark table
- Timing information (mean, median, stddev)
- Memory allocation data
- Raw output view
- Compilation error display

#### Resource Management
- Automatic temp file cleanup
- Process timeout handling
- Async/await throughout
- Proper disposal patterns

---

## Services Overview

### ICodeGenerationService
```csharp
string GenerateBenchmarkClass(BenchmarkRequest request);
string GenerateProgramFile(BenchmarkRequest request);
```
Generates complete BenchmarkDotNet code with attributes and configuration.

### ICompilationService
```csharp
Task<(bool IsValid, List<CompilationError> Errors)> ValidateCodeAsync(string code);
Task<(bool Success, List<CompilationError> Errors)> CompileCodeAsync(string code, string outputPath);
```
Validates and compiles C# code using Roslyn, returns detailed errors.

### IBenchmarkRunnerService
```csharp
Task<BenchmarkResult> RunBenchmarkAsync(BenchmarkRequest request, IProgress<(string, int)>? progress);
Task CleanupAsync(string tempDirectory);
```
Orchestrates the entire benchmark pipeline from code generation to results parsing.

---

## Sample Generated Code

### DynamicBenchmark.cs
```csharp
using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace BenchmarkRunner;

[MemoryDiagnoser]
[RankColumn]
public class DynamicBenchmark
{
    [Benchmark(Baseline = true)]
    public void MethodA()
    {
        // User's code here
    }

    [Benchmark]
    public void MethodB()
    {
        // User's code here
    }
}
```

---

## Testing

### Build and Run
```bash
# Build entire solution
dotnet build

# Run web application
cd BenchmarkTool.Web
dotnet run

# Access at https://localhost:5001
```

### Test Scenarios

1. **Valid Code**: List vs Array comparison
2. **Syntax Error**: Missing semicolon
3. **Semantic Error**: Undefined variable
4. **Long Running**: Loop with many iterations

---

## Build Status

? **Phase 1**: Complete - Project structure created  
? **Phase 2**: Complete - Frontend UI implemented  
? **Phase 3**: Complete - Backend services implemented  
? **Phase 4**: Pending - Testing & Optimization  
? **Phase 5**: Pending - Deployment

**Current Build**: Success (0 errors, 2 warnings - non-critical)  
**Build Time**: ~5 seconds  
**Total Lines of Code**: ~2,300

---

## Commands Reference

```bash
# Build solution
dotnet build

# Run web app
cd BenchmarkTool.Web && dotnet run

# Clean
dotnet clean

# Restore packages
dotnet restore

# Run tests (Phase 4)
dotnet test
```

---

## Next Steps: Phase 4

**Testing & Optimization**
- [ ] Add unit tests for services
- [ ] Add integration tests
- [ ] Performance optimization
- [ ] Add code examples/templates
- [ ] Implement request queue
- [ ] Add result caching
- [ ] Enhanced security (sandboxing)
- [ ] Better error messages
- [ ] Docker support
- [ ] Rate limiting

---

## Documentation

- `implementation plan.md` - Original requirements and design
- `PHASE1_COMPLETE.md` - Phase 1 setup documentation
- `PHASE2_COMPLETE.md` - Phase 2 frontend documentation
- `PHASE2_TESTING_GUIDE.md` - Frontend testing guide
- `PHASE2_SUMMARY.md` - Phase 2 technical summary
- `PHASE3_COMPLETE.md` - Phase 3 backend documentation ? NEW

---

**Project Status**: Fully Functional ?  
**Last Updated**: October 24, 2025  
**Ready For**: End-to-end testing and Phase 4 enhancements ??
