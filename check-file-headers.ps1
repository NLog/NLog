  msbuild /t:rebuild .\tools\CheckSourceCode\src\ /p:Configuration=Release /verbosity:minimal
  tools\CheckSourceCode\NLog.SourceCodeTests.exe no-interactive
