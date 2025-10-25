# Phase 3 Testing Guide

## ?? Testing the Complete BenchmarkTool Application

Phase 3 is complete! This guide will help you test the fully functional benchmarking application.

---

## Prerequisites

- ? .NET 10.0 SDK installed
- ? Solution builds successfully (`dotnet build`)
- ? BenchmarkTool.Web project ready to run

---

## Running the Application

### Start the Web Server

```bash
# From solution directory
cd BenchmarkTool.Web
dotnet run
```

The application will start on:
- **HTTPS**: https://localhost:5001
- **HTTP**: http://localhost:5000

### Open in Browser

Navigate to: **https://localhost:5001/Benchmark**

---

## Test Scenarios

### ? Test 1: Basic Functionality (List vs Array)

**Description**: Test the default sample code that compares List and Array performance.

**Steps**:
1. Load the Benchmark page
2. Sample code should be pre-loaded:
   - Method A: `List<int>` with 1000 items
   - Method B: `int[]` array with 1000 items
3. Click **"Run Benchmark"**
4. Observe the progress bar updating
5. Wait for completion (should take 1-2 minutes)

**Expected Result**:
- ? Progress updates: 10%, 20%, 30%... 100%
- ? Success message
- ? Benchmark table showing:
  - Method names
  - Mean execution time
  - Error margin
  - StdDev
  - Rank
  - Memory allocated
- ? "View Raw Output" section available

**Sample Result Table**:
```
| Method  | Mean      | Error    | StdDev   | Rank | Gen0   | Allocated |
|---------|-----------|----------|----------|------|--------|-----------|
| MethodA | 2.xxx ?s  | 0.xxx ?s | 0.xxx ?s | 1    | 0.xxxx | xxx B     |
| MethodB | 1.xxx ?s  | 0.xxx ?s | 0.xxx ?s | 2    | -      | xxx B     |
```

---

### ? Test 2: Compilation Error (Syntax Error)

**Description**: Test error handling for syntax errors.

**Steps**:
1. Edit Method A code:
```csharp
var list = new List<int>()  // Missing semicolon
```
2. Click **"Run Benchmark"**

**Expected Result**:
- ? Error alert box appears
- ? Message: "Code validation failed. Please check your code for errors."
- ? Compilation errors list showing:
  - Line number
  - Column number
  - Error message (e.g., "Expected `;`")
  - Error code (e.g., CS1002)

---

### ? Test 3: Semantic Error (Undefined Variable)

**Description**: Test error handling for semantic errors.

**Steps**:
1. Edit Method A code:
```csharp
myUndefinedList.Add(5);
```
2. Click **"Run Benchmark"**

**Expected Result**:
- ? Error alert box
- ? Compilation error: "The name 'myUndefinedList' does not exist in the current context"
- ? Error code: CS0103
- ? Line and column information

---

### ? Test 4: Complex Code (String Operations)

**Description**: Test with more complex code.

**Method A**:
```csharp
var result = "";
for (int i = 0; i < 100; i++)
{
    result += i.ToString();
}
```

**Method B**:
```csharp
var sb = new StringBuilder();
for (int i = 0; i < 100; i++)
{
    sb.Append(i.ToString());
}
var result = sb.ToString();
```

**Expected Result**:
- ? Both methods compile successfully
- ? Benchmark shows StringBuilder is much faster
- ? Memory allocation difference visible
- ? Method B should be ranked higher (faster)

---

### ? Test 5: LINQ Performance

**Description**: Compare LINQ methods.

**Method A**:
```csharp
var numbers = Enumerable.Range(1, 1000).ToList();
var result = numbers.Where(x => x % 2 == 0).ToList();
```

**Method B**:
```csharp
var numbers = Enumerable.Range(1, 1000).ToList();
var result = new List<int>();
for (int i = 0; i < numbers.Count; i++)
{
    if (numbers[i] % 2 == 0)
   result.Add(numbers[i]);
}
```

**Expected Result**:
- ? Both compile and run
- ? Performance comparison shown
- ? Memory diagnostics available

---

### ? Test 6: SignalR Real-Time Updates

**Description**: Verify SignalR is working.

**Steps**:
1. Open browser Developer Tools (F12)
2. Go to Console tab
3. Load Benchmark page
4. Run a benchmark

**Expected Console Output**:
```
SignalR connected
Progress: Generating benchmark code... - 10%
Progress: Validating code... - 20%
Progress: Creating temporary project... - 30%
Progress: Restoring NuGet packages... - 40%
Progress: Building project... - 50%
Progress: Running benchmarks... - 60%
...
Progress: Benchmark complete! - 100%
```

---

### ? Test 7: Reset Functionality

**Description**: Test the Reset button.

**Steps**:
1. Modify the code in both editors
2. Click **"Reset"** button

**Expected Result**:
- ? Both editors revert to original sample code
- ? List<int> example in Method A
- ? int[] array example in Method B

---

### ? Test 8: Long Running Benchmark

**Description**: Test timeout and progress updates.

**Method A**:
```csharp
for (int i = 0; i < 5000000; i++)
{
    var temp = i * 2;
}
```

**Method B**:
```csharp
for (int i = 0; i < 5000000; i++)
{
    var temp = i + i;
}
```

**Expected Result**:
- ? Progress bar updates during execution
- ? Takes longer but still completes
- ? Should complete within 5-minute timeout
- ? Results show timing differences

---

### ? Test 9: Empty Code (Edge Case)

**Description**: Test with empty methods.

**Method A**: (leave empty)
**Method B**: (leave empty)

**Expected Result**:
- ? Should compile successfully
- ? Benchmark runs very quickly
- ? Shows timing for empty methods (baseline overhead)

---

### ? Test 10: Using External Libraries

**Description**: Test code that uses common libraries.

**Method A**:
```csharp
var dict = new Dictionary<int, string>();
for (int i = 0; i < 100; i++)
{
    dict.Add(i, i.ToString());
}
```

**Method B**:
```csharp
var dict = new Dictionary<int, string>();
for (int i = 0; i < 100; i++)
{
dict[i] = i.ToString();
}
```

**Expected Result**:
- ? Compiles (Dictionary is in System.Collections.Generic)
- ? Shows performance difference between Add() and indexer

---

## Checklist: Features to Verify

### UI Components
- [ ] Monaco editors load with sample code
- [ ] Syntax highlighting works (keywords colored)
- [ ] Line numbers visible
- [ ] Dark theme applied
- [ ] Reset button works
- [ ] Run Benchmark button clickable

### Progress Reporting
- [ ] Progress bar appears when benchmark starts
- [ ] Percentage updates from 0% to 100%
- [ ] Status messages update at each stage
- [ ] Progress bar hides when complete

### Results Display
- [ ] Success state shows table
- [ ] Table includes: Method, Mean, Error, StdDev, Rank, Memory
- [ ] Numbers formatted correctly (?s, ns, etc.)
- [ ] Raw output collapsible section available
- [ ] Execution time displayed

### Error Handling
- [ ] Syntax errors caught and displayed
- [ ] Semantic errors caught and displayed
- [ ] Line/column numbers shown
- [ ] Error codes (CS####) displayed
- [ ] User-friendly error messages

### Real-Time Communication
- [ ] SignalR connects (check console)
- [ ] Progress updates received
- [ ] No JavaScript errors in console
- [ ] Connection resilient (doesn't drop)

---

## Performance Expectations

| Scenario | Expected Time |
|----------|---------------|
| Simple code (List vs Array) | 1-2 minutes |
| Complex code (StringBuilder) | 1-3 minutes |
| Empty methods | 30-60 seconds |
| Long running (5M iterations) | 2-4 minutes |

**Note**: First run takes longer due to NuGet restore and compilation.

---

## Troubleshooting

### Issue: Benchmark times out after 5 minutes

**Solution**: 
- Reduce iteration count in your code
- Timeout is configurable in `BenchmarkRunnerService.cs`

### Issue: "dotnet not found" error

**Solution**:
- Ensure .NET 10.0 SDK is installed
- Check PATH environment variable
- Restart terminal/IDE

### Issue: SignalR not connecting

**Solution**:
- Check browser console for errors
- Verify `BenchmarkHub` is mapped in `Program.cs`
- Hard refresh page (Ctrl+Shift+R)

### Issue: Compilation errors not showing details

**Solution**:
- Check Raw Output section
- Errors logged to `_logger` (check console output)
- Verify Roslyn is working: `dotnet --list-sdks`

### Issue: Temp files not cleaning up

**Solution**:
- Check `%TEMP%` directory
- Cleanup happens after 30 seconds
- Manual cleanup: Delete `BenchmarkTool_*` folders in temp

---

## Advanced Testing

### Test with Concurrent Requests (Phase 4)

**Current Behavior**: 
- No queuing system yet
- Multiple simultaneous benchmarks may conflict
- **Recommendation**: Wait for one to complete before starting another

### Test with Large Code

**Scenario**: 
- 1000+ lines of code
- Multiple classes/methods

**Expected**:
- May hit compilation limits
- Consider refactoring into smaller methods

---

## Sample Test Results

### List vs Array (Expected)
```
Method  | Mean     | Error   | StdDev  | Rank | Allocated
MethodA | 2.543 ?s | 0.05 ?s | 0.04 ?s | 2    | 4024 B
MethodB | 1.234 ?s | 0.02 ?s | 0.02 ?s | 1 | 4000 B
```
**Conclusion**: Array is faster, slightly less memory

### String Concatenation vs StringBuilder (Expected)
```
Method  | Mean      | Error    | StdDev| Rank | Allocated
MethodA | 45.32 ?s  | 0.89 ?s  | 0.83 ?s  | 2    | 50000 B
MethodB | 3.21 ?s   | 0.06 ?s  | 0.05 ?s  | 1    | 2040 B
```
**Conclusion**: StringBuilder is MUCH faster and uses less memory

---

## Success Criteria

All tests should pass with:
- ? No unhandled exceptions
- ? Clear error messages for invalid code
- ? Accurate benchmark results
- ? Real-time progress updates
- ? Formatted results display
- ? No memory leaks (temp files cleaned)

---

## Reporting Issues

If you encounter issues:

1. **Check Browser Console** (F12 ? Console)
2. **Check Application Logs** (Terminal where `dotnet run` is executing)
3. **Note the Error Details**:
   - What code was being tested?
   - At what stage did it fail?
   - What error message appeared?
4. **Check Temp Directory**: `%TEMP%` or `/tmp`
5. **Try Again**: Some issues are transient

---

## Next Steps

Once all tests pass:
- ? **Phase 3 Complete!**
- ?? Ready for **Phase 4: Testing & Optimization**
- Consider adding:
  - Unit tests
  - Integration tests
  - Performance tests
  - Load tests

---

**Happy Testing!** ??

If everything works as expected, you have a fully functional C# benchmarking tool! ??

---

**Last Updated**: October 24, 2025  
**Status**: Phase 3 Testing Ready ?
