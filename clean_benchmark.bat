set BENCHMARK_MODE=short

set FRAMEWORK=net-1.1
nant -t:%FRAMEWORK% release build NLog.Benchmark NLog.Benchmark-log4net NLog.Benchmark-log4net-withformat
rem ant -t:net-2.0 release build NLog.Benchmark
c:\apps\sysinternals\sync.exe
pushd build\%FRAMEWORK%\bin
start /abovenormal /wait NLog.Benchmark.exe NLog.results.xml %BENCHMARK_MODE%
popd
pushd build\%FRAMEWORK%\bin
start /abovenormal /wait NLog.Benchmark-log4net.exe log4net.results.xml %BENCHMARK_MODE%
popd
pushd build\%FRAMEWORK%\bin
start /abovenormal /wait NLog.Benchmark-log4net-withformat.exe log4net.results-withformat.xml %BENCHMARK_MODE%
popd

