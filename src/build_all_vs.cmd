@echo off
"%VS90COMNTOOLS%..\ide\devenv.com" NLog.sln /rebuild Debug || exit /b 1
"%VS90COMNTOOLS%..\ide\devenv.com" NLogC.sln /rebuild Debug || exit /b 1
"%VS90COMNTOOLS%..\ide\devenv.com" ..\tests\NLogTests.sln /rebuild Debug || exit /b 1
"%VS90COMNTOOLS%..\ide\devenv.com" ..\tests\NLogBinaryCompatTests.sln /rebuild Debug || exit /b 1

