// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

namespace NLog.UnitTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.CSharp;
    using NLog.Config;
    using NLog.UnitTests.Mocks;
    using Xunit;

    public sealed class ConfigFileLocatorTests : NLogTestBase, IDisposable
    {
        private readonly string _tempDirectory;

        public ConfigFileLocatorTests()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
        }

        void IDisposable.Dispose()
        {
            if (Directory.Exists(_tempDirectory))
                Directory.Delete(_tempDirectory, true);
        }

        [Fact]
        public void GetCandidateConfigTest()
        {
            var candidateConfigFilePaths = XmlLoggingConfiguration.GetCandidateConfigFilePaths();
            Assert.NotNull(candidateConfigFilePaths);
            var count = candidateConfigFilePaths.Count();
            Assert.NotEqual(0, count);
        }

        [Fact]
        public void GetCandidateConfigTest_list_is_readonly()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                var list = new List<string> { "c:\\global\\temp.config" };
                XmlLoggingConfiguration.SetCandidateConfigFilePaths(list);
                var candidateConfigFilePaths = XmlLoggingConfiguration.GetCandidateConfigFilePaths();
                var list2 = candidateConfigFilePaths as IList;
                list2.Add("test");
            });
        }

        [Fact]
        public void SetCandidateConfigTest()
        {
            var list = new List<string> { "c:\\global\\temp.config" };
            XmlLoggingConfiguration.SetCandidateConfigFilePaths(list);
            Assert.Single(XmlLoggingConfiguration.GetCandidateConfigFilePaths());
            //no side effects
            list.Add("c:\\global\\temp2.config");
            Assert.Single(XmlLoggingConfiguration.GetCandidateConfigFilePaths());
        }

        [Fact]
        public void ResetCandidateConfigTest()
        {
            var countBefore = XmlLoggingConfiguration.GetCandidateConfigFilePaths().Count();
            var list = new List<string> { "c:\\global\\temp.config" };
            XmlLoggingConfiguration.SetCandidateConfigFilePaths(list);
            Assert.Single(XmlLoggingConfiguration.GetCandidateConfigFilePaths());
            XmlLoggingConfiguration.ResetCandidateConfigFilePath();
            Assert.Equal(countBefore, XmlLoggingConfiguration.GetCandidateConfigFilePaths().Count());
        }

        [Theory]
        [MemberData(nameof(GetConfigFile_absolutePath_loads_testData))]
        public void GetConfigFile_absolutePath_loads(string filename, string accepts, string expected, string baseDir)
        {
            // Arrange
            var appEnvMock = new AppEnvironmentMock(f => f == accepts, f => System.Xml.XmlReader.Create(new StringReader(@"<nlog autoreload=""true""></nlog>"))) { AppDomainBaseDirectory = baseDir };
            var fileLoader = new LoggingConfigurationFileLoader(appEnvMock);
            var logFactory = new LogFactory(fileLoader);

            // Act
            var result = fileLoader.Load(logFactory, filename);

            // Assert
            Assert.Equal(expected, result?.FileNamesToWatch.First());
        }

        public static IEnumerable<object[]> GetConfigFile_absolutePath_loads_testData()
        {
            var d = Path.DirectorySeparatorChar;
            var baseDir = Path.GetTempPath();
            var dirInBaseDir = $"{baseDir}dir1";
            var rootBaseDir = Path.GetPathRoot(baseDir);
            yield return new object[] { "nlog.config", $"{rootBaseDir}nlog.config", $"{rootBaseDir}nlog.config", rootBaseDir };
            yield return new object[] { $"{baseDir}configfile", $"{baseDir}configfile", $"{baseDir}configfile", dirInBaseDir };
            yield return new object[] { "nlog.config", $"{baseDir}dir1{d}nlog.config", $"{baseDir}dir1{d}nlog.config", dirInBaseDir }; //exists
            yield return new object[] { "nlog.config", $"{baseDir}dir1{d}nlog2.config", null, dirInBaseDir }; //not existing, fallback
        }

        [Fact]
        public void LoadConfigFile_EmptyEnvironment_UseCurrentDirectory()
        {
            // Arrange
            var appEnvMock = new AppEnvironmentMock(f => true, f => null);
            var fileLoader = new LoggingConfigurationFileLoader(appEnvMock);

            // Act
            var result = fileLoader.GetDefaultCandidateConfigFilePaths().ToList();

            // Assert loading from current-directory and from nlog-assembly-directory
            if (NLog.Internal.PlatformDetector.IsWin32)
                Assert.Equal(2, result.Count);  // Case insensitive
            Assert.Equal("NLog.config", result.First(), StringComparer.OrdinalIgnoreCase);
            Assert.Contains("NLog.dll.nlog", result.Last(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void LoadConfigFile_NetCoreUnpublished_UseEntryDirectory()
        {
            // Arrange
            var tmpDir = Path.GetTempPath();
            var appEnvMock = new AppEnvironmentMock(f => true, f => null)
            {
                AppDomainBaseDirectory = Path.Combine(tmpDir, "BaseDir"),
#if NETSTANDARD
                AppDomainConfigurationFile = string.Empty,                  // NetCore style
#else
                AppDomainConfigurationFile = Path.Combine(tmpDir, "EntryDir", "Entry.exe.config"),
#endif
                CurrentProcessFilePath = Path.Combine(tmpDir, "ProcessDir", "dotnet.exe"),  // NetCore dotnet.exe
                EntryAssemblyLocation = Path.Combine(tmpDir, "EntryDir"),
                EntryAssemblyFileName = "Entry.dll"
            };

            var fileLoader = new LoggingConfigurationFileLoader(appEnvMock);

            // Act
            var result = fileLoader.GetDefaultCandidateConfigFilePaths().ToList();

            // Assert base-directory + entry-directory + nlog-assembly-directory
            AssertResult(tmpDir, "EntryDir", "EntryDir", "Entry", result);
        }

        [Fact]
        public void LoadConfigFile_NetCorePublished_UseProcessDirectory()
        {
            // Arrange
            var tmpDir = Path.GetTempPath();
            var appEnvMock = new AppEnvironmentMock(f => true, f => null)
            {
                AppDomainBaseDirectory = Path.Combine(tmpDir, "BaseDir"),
#if NETSTANDARD
                AppDomainConfigurationFile = string.Empty,                  // NetCore style
#else
                AppDomainConfigurationFile = Path.Combine(tmpDir, "ProcessDir", "Entry.exe.config"),
#endif
                CurrentProcessFilePath = Path.Combine(tmpDir, "ProcessDir", "Entry.exe"),    // NetCore published exe
                EntryAssemblyLocation = Path.Combine(tmpDir, "ProcessDir"),
                EntryAssemblyFileName = "Entry.dll"
            };

            var fileLoader = new LoggingConfigurationFileLoader(appEnvMock);

            // Act
            var result = fileLoader.GetDefaultCandidateConfigFilePaths().ToList();

            // Assert base-directory + process-directory + nlog-assembly-directory
            AssertResult(tmpDir, "ProcessDir", "ProcessDir", "Entry", result);
        }

        [Fact]
        public void LoadConfigFile_NetCoreSingleFilePublish_IgnoreTempDirectory()
        {
            // Arrange
            var tmpDir = Path.GetTempPath();
            var appEnvMock = new AppEnvironmentMock(f => true, f => null)
            {
                AppDomainBaseDirectory = Path.Combine(tmpDir, "BaseDir"),
#if NETSTANDARD
                AppDomainConfigurationFile = string.Empty,                  // NetCore style
#else
                AppDomainConfigurationFile = Path.Combine(tmpDir, "TempProcessDir", "Entry.exe.config"),
#endif
                CurrentProcessFilePath = Path.Combine(tmpDir, "ProcessDir", "Entry.exe"),    // NetCore published exe
                EntryAssemblyLocation = Path.Combine(tmpDir, "TempProcessDir"),
                UserTempFilePath = Path.Combine(tmpDir, "TempProcessDir"),
                EntryAssemblyFileName = "Entry.dll"
            };

            var fileLoader = new LoggingConfigurationFileLoader(appEnvMock);

            // Act
            var result = fileLoader.GetDefaultCandidateConfigFilePaths().ToList();

            // Assert base-directory + process-directory + nlog-assembly-directory
#if NETSTANDARD
            Assert.Equal(Path.Combine(tmpDir, "ProcessDir", "Entry.exe.nlog"), result.First(), StringComparer.OrdinalIgnoreCase);
#endif
            AssertResult(tmpDir, "TempProcessDir", "ProcessDir", "Entry", result);
        }

        [Fact]
        public void LoadConfigFile_NetCoreSingleFilePublish_IgnoreTmpDirectory()
        {
            // Arrange
            var tmpDir = "/var/tmp/";
            var appEnvMock = new AppEnvironmentMock(f => true, f => null)
            {
                AppDomainBaseDirectory = Path.Combine(tmpDir, "BaseDir"),
#if NETSTANDARD
                AppDomainConfigurationFile = string.Empty,                  // NetCore style
#else
                AppDomainConfigurationFile = Path.Combine(tmpDir, "TempProcessDir", "Entry.exe.config"),
#endif
                CurrentProcessFilePath = Path.Combine(tmpDir, "ProcessDir", "Entry.exe"),    // NetCore published exe
                EntryAssemblyLocation = Path.Combine(tmpDir, "TempProcessDir"),
                UserTempFilePath = "/tmp/",
                EntryAssemblyFileName = "Entry.dll"
            };

            var fileLoader = new LoggingConfigurationFileLoader(appEnvMock);

            // Act
            var result = fileLoader.GetDefaultCandidateConfigFilePaths().ToList();

            // Assert base-directory + process-directory + nlog-assembly-directory
#if NETSTANDARD
            Assert.Equal(Path.Combine(tmpDir, "ProcessDir", "Entry.exe.nlog"), result.First(), StringComparer.OrdinalIgnoreCase);
#endif
            AssertResult(tmpDir, "TempProcessDir", "ProcessDir", "Entry", result);
        }

        private static void AssertResult(string tmpDir, string appDir, string processDir, string appName, List<string> result)
        {
#if NETSTANDARD
            Assert.Contains(Path.Combine(tmpDir, processDir, appName + ".exe.nlog"), result, StringComparer.OrdinalIgnoreCase);
            Assert.Contains(Path.Combine(tmpDir, appDir, "Entry.dll.nlog"), result, StringComparer.OrdinalIgnoreCase);
            if (NLog.Internal.PlatformDetector.IsWin32)
            {
                if (appDir != processDir)
                    Assert.Equal(6, result.Count);  // Single File Publish on NetCore 3.1 - Case insensitive
                else
                    Assert.Equal(5, result.Count);  // Case insensitive
            }
            // Verify Single File Publish will always load "exe.nlog" before "dll.nlog"
            var priorityIndexExe = result.FindIndex(s => s.EndsWith(appName+".exe.nlog"));
            var priorityIndexDll = result.FindIndex(s => s.EndsWith(appName+".dll.nlog"));
            Assert.True(priorityIndexExe <  priorityIndexDll, $"{appName+".exe.nlog"}={priorityIndexExe} < {appName+".dll.nlog"}={priorityIndexDll}"); // Always scan for exe.nlog first
#else
            Assert.Equal(Path.Combine(tmpDir, appDir, appName + ".exe.nlog"), result.First(), StringComparer.OrdinalIgnoreCase);
            if (NLog.Internal.PlatformDetector.IsWin32)
                Assert.Equal(4, result.Count);  // Case insensitive
#endif
            Assert.Contains(Path.Combine(tmpDir, "BaseDir", "NLog.config"), result, StringComparer.OrdinalIgnoreCase);
            Assert.Contains(Path.Combine(tmpDir, appDir, "NLog.config"), result, StringComparer.OrdinalIgnoreCase);
            Assert.Contains("NLog.dll.nlog", result.Last(), StringComparison.OrdinalIgnoreCase);
        }

#if !NETSTANDARD
        private string appConfigContents = @"
<configuration>
<configSections>
    <section name='nlog' type='NLog.Config.ConfigSectionHandler, NLog' requirePermission='false' />
</configSections>

<nlog>
  <targets>
    <target name='c' type='Console' layout='AC ${message}' />
  </targets>
  <rules>
    <logger name='*' minLevel='Info' writeTo='c' />
  </rules>
</nlog>
</configuration>
";

        private string appNLogContents = @"
<nlog>
  <targets>
    <target name='c' type='Console' layout='AN ${message}' />
  </targets>
  <rules>
    <logger name='*' minLevel='Info' writeTo='c' />
  </rules>
</nlog>
";

        private string nlogConfigContents = @"
<nlog>
  <targets>
    <target name='c' type='Console' layout='NLC ${message}' />
  </targets>
  <rules>
    <logger name='*' minLevel='Info' writeTo='c' />
  </rules>
</nlog>
";

        private string nlogDllNLogContents = @"
<nlog>
  <targets>
    <target name='c' type='Console' layout='NDN ${message}' />
  </targets>
  <rules>
    <logger name='*' minLevel='Info' writeTo='c' />
  </rules>
</nlog>
";

        private string appConfigOutput = "--BEGIN--|AC InfoMsg|AC WarnMsg|AC ErrorMsg|AC FatalMsg|--END--|";
        private string appNLogOutput = "--BEGIN--|AN InfoMsg|AN WarnMsg|AN ErrorMsg|AN FatalMsg|--END--|";
        private string nlogConfigOutput = "--BEGIN--|NLC InfoMsg|NLC WarnMsg|NLC ErrorMsg|NLC FatalMsg|--END--|";
        private string nlogDllNLogOutput = "--BEGIN--|NDN InfoMsg|NDN WarnMsg|NDN ErrorMsg|NDN FatalMsg|--END--|";
        private string missingConfigOutput = "--BEGIN--|--END--|";

        [Fact]
        public void MissingConfigFileTest()
        {
            string output = RunTest();
            Assert.Equal(missingConfigOutput, output);
        }

        [Fact]
        public void NLogDotConfigTest()
        {
            File.WriteAllText(Path.Combine(_tempDirectory, "NLog.config"), nlogConfigContents);
            string output = RunTest();
            Assert.Equal(nlogConfigOutput, output);
        }

        [Fact]
        public void NLogDotDllDotNLogTest()
        {
            File.WriteAllText(Path.Combine(_tempDirectory, "NLog.dll.nlog"), nlogDllNLogContents);
            string output = RunTest();
            Assert.Equal(nlogDllNLogOutput, output);
        }

        [Fact]
        public void NLogDotDllDotNLogInDirectoryWithSpaces()
        {
            File.WriteAllText(Path.Combine(_tempDirectory, "NLog.dll.nlog"), nlogDllNLogContents);
            string output = RunTest();
            Assert.Equal(nlogDllNLogOutput, output);
        }

        [Fact]
        public void AppDotConfigTest()
        {
            File.WriteAllText(Path.Combine(_tempDirectory, "ConfigFileLocator.exe.config"), appConfigContents);
            string output = RunTest();
            Assert.Equal(appConfigOutput, output);
        }

        [Fact]
        public void AppDotNLogTest()
        {
            File.WriteAllText(Path.Combine(_tempDirectory, "ConfigFileLocator.exe.nlog"), appNLogContents);
            string output = RunTest();
            Assert.Equal(appNLogOutput, output);
        }

        [Fact]
        public void PrecedenceTest()
        {
            var precedence = new[]
                                 {
                                     new
                                         {
                                             File = "ConfigFileLocator.exe.config",
                                             Contents = appConfigContents,
                                             Output = appConfigOutput
                                         },
                                     new
                                         {
                                             File = "ConfigFileLocator.exe.nlog",
                                             Contents = appNLogContents,
                                             Output = appNLogOutput
                                         },
                                     new
                                         {
                                             File = "NLog.config",
                                             Contents = nlogConfigContents,
                                             Output = nlogConfigOutput
                                         },
                                     new
                                         {
                                             File = "NLog.dll.nlog",
                                             Contents = nlogDllNLogContents,
                                             Output = nlogDllNLogOutput
                                         },
                                 };
            // deploy all files
            foreach (var p in precedence)
            {
                File.WriteAllText(Path.Combine(_tempDirectory, p.File), p.Contents);
            }

            string output;

            // walk files in precedence order and delete config files
            foreach (var p in precedence)
            {
                output = RunTest();
                Assert.Equal(p.Output, output);
                File.Delete(Path.Combine(_tempDirectory, p.File));
            }

            output = RunTest();
            Assert.Equal(missingConfigOutput, output);
        }


        private string RunTest()
        {
            string sourceCode = @"
using System;
using System.Reflection;
using NLog;

class C1
{
    private static ILogger logger = LogManager.GetCurrentClassLogger();

    static void Main(string[] args)
    {
        Console.WriteLine(""--BEGIN--"");
        logger.Trace(""TraceMsg"");
        logger.Debug(""DebugMsg"");
        logger.Info(""InfoMsg"");
        logger.Warn(""WarnMsg"");
        logger.Error(""ErrorMsg"");
        logger.Fatal(""FatalMsg"");
        Console.WriteLine(""--END--"");
    }
}";
            var provider = new CSharpCodeProvider();
            var options = new System.CodeDom.Compiler.CompilerParameters();
            options.OutputAssembly = Path.Combine(_tempDirectory, "ConfigFileLocator.exe");
            options.GenerateExecutable = true;
            options.ReferencedAssemblies.Add(typeof(ILogger).Assembly.Location);
            options.IncludeDebugInformation = true;
            if (!File.Exists(options.OutputAssembly))
            {
                var results = provider.CompileAssemblyFromSource(options, sourceCode);
                Assert.False(results.Errors.HasWarnings);
                Assert.False(results.Errors.HasErrors);
                File.Copy(typeof(ILogger).Assembly.Location, Path.Combine(_tempDirectory, "NLog.dll"));
            }

            return RunAndRedirectOutput(options.OutputAssembly);
        }

        public static string RunAndRedirectOutput(string exeFile)
        {
            using (var proc = new Process())
            {
#if MONO
				var sb = new StringBuilder();
				sb.AppendFormat("\"{0}\" ", exeFile);
				proc.StartInfo.Arguments = sb.ToString();
                proc.StartInfo.FileName = "mono";
				proc.StartInfo.StandardOutputEncoding = Encoding.UTF8;
				proc.StartInfo.StandardErrorEncoding = Encoding.UTF8;
#else
                proc.StartInfo.FileName = exeFile;
#endif
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                proc.StartInfo.RedirectStandardInput = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                proc.WaitForExit();
                Assert.Equal(string.Empty, proc.StandardError.ReadToEnd());
                return proc.StandardOutput.ReadToEnd().Replace("\r", "").Replace("\n", "|");
            }
        }
#endif
    }
}