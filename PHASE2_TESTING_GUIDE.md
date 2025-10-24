# Phase 2 Testing Guide

## ? Phase 2: Frontend Development - COMPLETED

### What Was Built

A fully functional frontend with:
- Monaco Editor for C# code editing (dark theme, syntax highlighting)
- SignalR hub for real-time communication
- Beautiful Bootstrap UI with icons and animations
- Progress tracking UI components
- Results display with error handling

### How to Test Phase 2

#### 1. Run the Application

```bash
cd BenchmarkTool.Web
dotnet run
```

The application will start on:
- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000

#### 2. Navigate Through the UI

**Homepage (`/` or `/Index`)**
- Should see welcome page with "BenchmarkTool" title
- Three feature cards explaining the tool
- "Start Benchmarking" button
- Click the button to go to benchmark page

**Benchmark Page (`/Benchmark`)**
- Two side-by-side Monaco editors with sample code
- Method A (left): List<int> example
- Method B (right): Array example
- "Run Benchmark" button
- "Reset" button

#### 3. Test Editor Features

**Monaco Editor**
? Dark theme applied
? C# syntax highlighting working
? Line numbers visible
? Auto-completion (type `var` or `for`)
? Code formatting (right-click ? Format Document)
? Find/Replace (Ctrl+F / Ctrl+H)

**Edit the Code**
- Try typing new code in either editor
- Test syntax highlighting with keywords: `public`, `class`, `void`, `async`
- Test IntelliSense by typing `List.` or `string.`

#### 4. Test UI Interactions

**Reset Button**
- Make some changes to the code
- Click "Reset" button
- Code should revert to sample List/Array example

**Run Benchmark Button** (Currently shows placeholder)
- Click "Run Benchmark"
- Page will submit the form
- Currently shows: "Benchmark service not yet implemented. Continue to Phase 3!"
- This is expected - backend services are Phase 3

#### 5. Test SignalR Connection

**Check Browser Console**
- Open Developer Tools (F12)
- Go to Console tab
- You should see: `SignalR connected`
- This confirms the hub is working

**Test Real-time Communication**
- SignalR hub is ready for Phase 3
- Listeners are set up for: `ReceiveProgress`, `ReceiveStatus`, `ReceiveError`, `ReceiveResults`

#### 6. Test Responsive Design

**Desktop View**
- Editors should be side-by-side
- Cards should be equal height
- Navigation bar spans full width

**Mobile View** (Resize browser or use DevTools device mode)
- Editors should stack vertically
- Buttons remain visible and clickable
- Navigation collapses to hamburger menu

### Expected Behavior (Phase 2)

? **Working Features:**
- Homepage loads with welcome message
- Navigation links work (Home, Benchmark)
- Monaco editors load with sample code
- Code editing works (type, delete, format)
- Reset button restores sample code
- SignalR connects successfully
- UI is responsive and looks professional

? **Not Yet Implemented (Phase 3):**
- Actual benchmark execution
- Code compilation
- Results parsing
- Progress bar updates during execution
- Real benchmark results display

### Visual Checklist

- [ ] Homepage displays correctly with icons
- [ ] "Start Benchmarking" button navigates to `/Benchmark`
- [ ] Two Monaco editors load with dark theme
- [ ] Sample code appears in both editors
- [ ] Can type and edit code in both editors
- [ ] Syntax highlighting works for C# keywords
- [ ] Line numbers are visible
- [ ] "Run Benchmark" button shows placeholder message
- [ ] "Reset" button restores original code
- [ ] Browser console shows "SignalR connected"
- [ ] No JavaScript errors in console
- [ ] UI is responsive on different screen sizes

### Common Issues & Solutions

**Monaco Editor Not Loading**
- Check browser console for CDN errors
- Ensure you have internet connection (Monaco loads from CDN)
- Try hard refresh: Ctrl+Shift+R (Windows) or Cmd+Shift+R (Mac)

**SignalR Not Connecting**
- Check browser console for connection errors
- Ensure `BenchmarkHub` is registered in `Program.cs`
- Verify SignalR script is loading from CDN

**Styles Not Applied**
- Hard refresh the page
- Check that Bootstrap Icons CDN is loading
- Verify `site.css` is being served

### Screenshots to Verify

1. **Homepage**: 
   - Large "BenchmarkTool" title with speedometer icon
   - Blue "Start Benchmarking" button
   - Three feature cards

2. **Benchmark Page**:
   - Dark navbar with "BenchmarkTool" brand
 - Two cards: blue header (Method A), green header (Method B)
   - Monaco editors with dark background and white text
   - Sample C# code visible
   - Two buttons below: blue "Run Benchmark", gray "Reset"

3. **After Clicking "Run Benchmark"**:
   - Red alert box at bottom
   - Message: "Benchmark service not yet implemented. Continue to Phase 3!"

### Next Steps

Once Phase 2 is verified working:
- ? Frontend is complete and ready
- ?? Proceed to **Phase 3: Backend Services**
- Implement: CodeGenerationService, CompilationService, BenchmarkRunnerService
- Wire up the backend to make the "Run Benchmark" button actually work

---

**Phase 2 Status**: COMPLETE ?  
**Ready for Phase 3**: YES ??  
**Last Updated**: October 24, 2025
