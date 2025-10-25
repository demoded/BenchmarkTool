# BenchmarkTool

Simple web interface to run BenchmarkDotnet.
Pure vibe coded in about 8 hours

| Time     | Activity                                                                                                                                                         |
|----------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 30 min   | first prompt with `implementation plan.md`<br>no progress bar<br>generated project failed to compile                                                              |
| 1 hour   | silly Copilot created `namespace BenchmarkRunner` and then failed compile `var summary = BenchmarkRunner.Run<DynamicBenchmark>(config);`                                          |
| 4 hours  | first fight for the progress bar... no luck                                                                                                                       |
| 15 min   | next day it just fixed the progress bar in 1 go                                                                                                                  |
| 30 min   | changed results output from Markdown to HTML                                                                                                                     |
| 2 hours  | to add `Declarations` and `Setup`                                                                                                                               |

![BenchmarkTool Screenshot](./screenshot.png)
