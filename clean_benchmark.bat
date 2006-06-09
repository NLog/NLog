set BENCHMARK_MODE=short

set FRAMEWORK=net-1.1
nant -t:%FRAMEWORK% release build NLog.Benchmark NLog.Benchmark-log4net-withformat

