// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !NETSTANDARD

namespace NLog.UnitTests
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.CSharp;
    using Xunit;
    using NLog.Config;
    using NLog.Layouts;
    using NLog.LayoutRenderers;

    public class ConfigFileLocatorTests : NLogTestBase, IDisposable
    {
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
                                             File = "NLog.config",
                                             Contents = nlogConfigContents,
                                             Output = nlogConfigOutput
                                         },
                                     new
                                         {
                                             File = "ConfigFileLocator.exe.nlog",
                                             Contents = appNLogContents,
                                             Output = appNLogOutput
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

        [Fact]
        void FuncLayoutRendererRegisterTest1()
        {
            LayoutRenderer.Register("the-answer", (info) => "42");
            Layout l = "${the-answer}";
            var result = l.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("42", result);

        }
        [Fact]
        void FuncLayoutRendererRegisterTest1WithXML()
        {
            LayoutRenderer.Register("the-answer", (info) => "42");

            LogManager.Configuration = CreateConfigurationFromString(@"
<nlog throwExceptions='true'>
            
                <targets>
                    <target name='debug' type='Debug' layout= '${the-answer}' /></targets>
                <rules>
                    <logger name='*' minlevel='Debug' writeTo='debug' />
                </rules>
            </nlog>");

            var logger = LogManager.GetCurrentClassLogger();
            logger.Debug("test1");
            AssertDebugLastMessage("debug", "42");

        }

        [Fact]
        void FuncLayoutRendererRegisterTest2()
        {
            LayoutRenderer.Register("message-length", (info) => info.Message.Length);
            Layout l = "${message-length}";
            var result = l.Render(LogEventInfo.Create(LogLevel.Error, "logger-adhoc", "1234567890"));
            Assert.Equal("10", result);

        }

        [Fact]
        public void GetCandidateConfigTest()
        {
            Assert.NotNull(XmlLoggingConfiguration.GetCandidateConfigFilePaths());
            var candidateConfigFilePaths = XmlLoggingConfiguration.GetCandidateConfigFilePaths();
            var count = candidateConfigFilePaths.Count();
            Assert.True(count > 0);

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
            var options = new CompilerParameters();
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
    }
}

#endif