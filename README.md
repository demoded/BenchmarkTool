# BenchmarkTool

Simple web interface to run BenchmarkDotnet.
Pure vibe coded in about 8 hours

| Time     | Activity                                                                                                                                                         |
|----------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 30&nbsp;min   | first prompt with `implementation plan.md`<br>no progress bar<br>generated project failed to compile                                                              |
| 1&nbsp;hour   | silly Copilot created `namespace BenchmarkRunner` and then failed compile `var summary = BenchmarkRunner.Run<DynamicBenchmark>(config);`                                          |
| 4&nbsp;hours  | first fight for the progress bar... no luck                                                                                                                       |
| 15&nbsp;min   | next day it just fixed the progress bar in 1 go                                                                                                                  |
| 30&nbsp;min   | changed results output from Markdown to HTML                                                                                                                     |
| 2&nbsp;hours  | to add `Declarations` and `Setup`                                                                                                                               |

![BenchmarkTool Screenshot](./screenshot.png)
