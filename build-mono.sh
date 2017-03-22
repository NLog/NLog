#!/bin/bash
set -ev

if [ "$MONO_V" = "MONO2" ]; then

  PATH=$TRAVIS_BUILD_DIR/src/.nuget:$PATH 
  mono --runtime=v4.0 src/.nuget/NuGet.exe restore src/NLog.mono2.sln
  mono --runtime=v4.0 src/.nuget/NuGet.exe install xunit.runners -Version 1.9.2 -OutputDirectory testrunner
  xbuild /p:Configuration=Release src/NLog.mono2.sln
  mono ./testrunner/xunit.runners.1.9.2/tools/xunit.console.exe      "./build/bin/Release/Mono 2.x/NLog.UnitTests.dll"
fi
if [ "$MONO_V" = "MONO4" ]; then
  nuget restore src/NLog.mono4.sln
  nuget install xunit.runners -Version 1.9.2 -OutputDirectory testrunner
  xbuild /p:Configuration=Release src/NLog.mono4.sln
  mono ./testrunner/xunit.runners.1.9.2/tools/xunit.console.clr4.exe "./build/bin/Release/Mono 4.x/NLog.UnitTests.dll"
fi