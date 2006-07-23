// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;

using System.Text;
using System.CodeDom.Compiler;

//
// distribution - taken from some development sample
// 
// Debug - 20000
// Info - 6500
// Warn - 100
// Error - 10
// Fatal - 0
//

namespace NLog.Benchmark
{
    class Program
    {
        private static double _overhead = 0.0;
        private static double _maxmax = 0.0;

        private static string GenerateTestSourceCode(IBenchmark bench)
        {
            StringWriter sw = new StringWriter();

            sw.WriteLine("using System;");

            sw.WriteLine(bench.Header);

            sw.WriteLine("public class TheBenchmark {");
            sw.WriteLine("static TheBenchmark() {");
            sw.WriteLine("}");
            sw.WriteLine(bench.CreateSource("logger1", "nosuchlogger"));
            sw.WriteLine(bench.CreateSource("logger2", "null1"));
            sw.WriteLine(bench.CreateSource("logger3", "null2"));
            sw.WriteLine(bench.CreateSource("logger4", "file1"));
            sw.WriteLine(bench.CreateSource("logger5", "file3"));
            sw.WriteLine(bench.CreateSource("logger6", "file2"));

            sw.WriteLine("public static void Init() {");
            //sw.WriteLine("Console.WriteLine(\"Init\");");
            sw.WriteLine(bench.Init);
            sw.WriteLine("} // Init()");
            sw.WriteLine("public static void Flush() {");
            //sw.WriteLine("Console.WriteLine(\"Flushing\");");
            sw.WriteLine(bench.Flush);
            //sw.WriteLine("Console.WriteLine(\"Flushed\");");
            sw.WriteLine("} // Flush()");
            sw.WriteLine("public static void DoNothing() {");
            sw.WriteLine("} // DoNothing()");
            sw.WriteLine("public static void NoLogging() {");
            sw.WriteLine(bench.WriteUnformatted("logger1", "Debug", "Lorem Ipsum"));
            sw.WriteLine(bench.WriteUnformatted("logger1", "Info", "Lorem Ipsum"));
            sw.WriteLine(bench.WriteUnformatted("logger1", "Warn", "Lorem Ipsum"));
            sw.WriteLine(bench.WriteUnformatted("logger1", "Error", "Lorem Ipsum"));
            sw.WriteLine(bench.WriteUnformatted("logger1", "Fatal", "Lorem Ipsum"));
            sw.WriteLine("}");
            sw.WriteLine("public static void NoLoggingWithFormatting1() {");
            sw.WriteLine(bench.WriteFormatted("logger1", "Debug", "Lorem Ipsum", "1"));
            sw.WriteLine(bench.WriteFormatted("logger1", "Info", "Lorem Ipsum", "2"));
            sw.WriteLine(bench.WriteFormatted("logger1", "Warn", "Lorem Ipsum", "3"));
            sw.WriteLine(bench.WriteFormatted("logger1", "Error", "Lorem Ipsum", "4"));
            sw.WriteLine(bench.WriteFormatted("logger1", "Fatal", "Lorem Ipsum", "5"));
            sw.WriteLine("}");
            sw.WriteLine("public static void NoLoggingWithFormatting2() {");
            sw.WriteLine(bench.WriteFormatted("logger1", "Debug", "Lorem Ipsum", "1,2"));
            sw.WriteLine(bench.WriteFormatted("logger1", "Info", "Lorem Ipsum", "2,3"));
            sw.WriteLine(bench.WriteFormatted("logger1", "Warn", "Lorem Ipsum", "3,4"));
            sw.WriteLine(bench.WriteFormatted("logger1", "Error", "Lorem Ipsum", "4,5"));
            sw.WriteLine(bench.WriteFormatted("logger1", "Fatal", "Lorem Ipsum", "5,6"));
            sw.WriteLine("}");
            sw.WriteLine("public static void NoLoggingWithFormatting3() {");
            sw.WriteLine(bench.WriteFormatted("logger1", "Debug", "Lorem Ipsum", "true,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger1", "Info", "Lorem Ipsum", "1,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger1", "Warn", "Lorem Ipsum", "false,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger1", "Error", "Lorem Ipsum", "1,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger1", "Fatal", "Lorem Ipsum", "true,2,\"test\""));
            sw.WriteLine("}");
            sw.WriteLine("public static void GuardedNoLogging() {");
            sw.WriteLine(bench.GuardedWrite("logger1", "Debug", "Lorem Ipsum", "true,2,\"test\""));
            sw.WriteLine(bench.GuardedWrite("logger1", "Info", "Lorem Ipsum", "1,2,\"test\""));
            sw.WriteLine(bench.GuardedWrite("logger1", "Warn", "Lorem Ipsum", "false,2,\"test\""));
            sw.WriteLine(bench.GuardedWrite("logger1", "Error", "Lorem Ipsum", "1,2,\"test\""));
            sw.WriteLine(bench.GuardedWrite("logger1", "Fatal", "Lorem Ipsum", "true,2,\"test\""));
            sw.WriteLine("}");
            sw.WriteLine("public static void NullLoggingWithoutFormatting() {");
            sw.WriteLine(bench.WriteUnformatted("logger2", "Debug", "Lorem Ipsum"));
            sw.WriteLine(bench.WriteUnformatted("logger2", "Info", "Lorem Ipsum"));
            sw.WriteLine(bench.WriteUnformatted("logger2", "Warn", "Lorem Ipsum"));
            sw.WriteLine(bench.WriteUnformatted("logger2", "Error", "Lorem Ipsum"));
            sw.WriteLine(bench.WriteUnformatted("logger2", "Fatal", "Lorem Ipsum"));
            sw.WriteLine("}");
            sw.WriteLine("public static void NullLoggingWithFormatting1() {");
            sw.WriteLine(bench.WriteFormatted("logger2", "Debug", "Lorem Ipsum", "1"));
            sw.WriteLine(bench.WriteFormatted("logger2", "Info", "Lorem Ipsum", "2"));
            sw.WriteLine(bench.WriteFormatted("logger2", "Warn", "Lorem Ipsum", "3"));
            sw.WriteLine(bench.WriteFormatted("logger2", "Error", "Lorem Ipsum", "4"));
            sw.WriteLine(bench.WriteFormatted("logger2", "Fatal", "Lorem Ipsum", "5"));
            sw.WriteLine("}");
            sw.WriteLine("public static void NullLoggingWithFormatting3() {");
            sw.WriteLine(bench.WriteFormatted("logger2", "Debug", "Lorem Ipsum", "true,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger2", "Info", "Lorem Ipsum", "1,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger2", "Warn", "Lorem Ipsum", "false,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger2", "Error", "Lorem Ipsum", "1,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger2", "Fatal", "Lorem Ipsum", "true,2,\"test\""));
            sw.WriteLine("}");
            sw.WriteLine("public static void NoRenderingLoggingWithoutFormatting() {");
            sw.WriteLine(bench.WriteUnformatted("logger3", "Debug", "Lorem Ipsum"));
            sw.WriteLine(bench.WriteUnformatted("logger3", "Info", "Lorem Ipsum"));
            sw.WriteLine(bench.WriteUnformatted("logger3", "Warn", "Lorem Ipsum"));
            sw.WriteLine(bench.WriteUnformatted("logger3", "Error", "Lorem Ipsum"));
            sw.WriteLine(bench.WriteUnformatted("logger3", "Fatal", "Lorem Ipsum"));
            sw.WriteLine("}");
            sw.WriteLine("public static void NoRenderingLoggingWithFormatting1() {");
            sw.WriteLine(bench.WriteFormatted("logger3", "Debug", "Lorem Ipsum", "1"));
            sw.WriteLine(bench.WriteFormatted("logger3", "Info", "Lorem Ipsum", "2"));
            sw.WriteLine(bench.WriteFormatted("logger3", "Warn", "Lorem Ipsum", "3"));
            sw.WriteLine(bench.WriteFormatted("logger3", "Error", "Lorem Ipsum", "4"));
            sw.WriteLine(bench.WriteFormatted("logger3", "Fatal", "Lorem Ipsum", "5"));
            sw.WriteLine("}");
            sw.WriteLine("public static void NoRenderingLoggingWithFormatting3() {");
            sw.WriteLine(bench.WriteFormatted("logger3", "Debug", "Lorem Ipsum", "true,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger3", "Info", "Lorem Ipsum", "1,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger3", "Warn", "Lorem Ipsum", "false,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger3", "Error", "Lorem Ipsum", "1,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger3", "Fatal", "Lorem Ipsum", "true,2,\"test\""));
            sw.WriteLine("}");
            sw.WriteLine("public static void SimpleFile() {");
            sw.WriteLine(bench.WriteFormatted("logger4", "Debug", "Lorem Ipsum", "true,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger4", "Info", "Lorem Ipsum", "1,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger4", "Warn", "Lorem Ipsum", "false,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger4", "Error", "Lorem Ipsum", "1,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger4", "Fatal", "Lorem Ipsum", "true,2,\"test\""));
            sw.WriteLine("}");
            sw.WriteLine("public static void BufferedFile() {");
            sw.WriteLine(bench.WriteFormatted("logger5", "Debug", "Lorem Ipsum", "true,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger5", "Info", "Lorem Ipsum", "1,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger5", "Warn", "Lorem Ipsum", "false,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger5", "Error", "Lorem Ipsum", "1,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger5", "Fatal", "Lorem Ipsum", "true,2,\"test\""));
            sw.WriteLine("}");
            sw.WriteLine("public static void AsyncFile() {");
            sw.WriteLine(bench.WriteFormatted("logger6", "Debug", "Lorem Ipsum", "true,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger6", "Info", "Lorem Ipsum", "1,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger6", "Warn", "Lorem Ipsum", "false,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger6", "Error", "Lorem Ipsum", "1,2,\"test\""));
            sw.WriteLine(bench.WriteFormatted("logger6", "Fatal", "Lorem Ipsum", "true,2,\"test\""));
            sw.WriteLine("}");
            sw.WriteLine("}");
            return sw.ToString();
        }

        delegate void RunDelegate();

        private static double TimeCode(RunDelegate init, RunDelegate run, RunDelegate flush, int count)
        {
            int unrollCount = 10;
            if (init != null)
                init();
            run();
            run();
            run();
            StopWatch sw = new StopWatch();
            sw.Start();

            for (int i = 0; i < count; ++i)
            {
                run();
                run();
                run();
                run();
                run();
                run();
                run();
                run();
                run();
                run();
            }
            if (flush != null)
                flush();
            sw.Stop();
            return sw.Nanoseconds / (count * unrollCount);
        }

        private static void TimeAndDiscardUnusual(RunDelegate init, RunDelegate run, RunDelegate flush, int count, int samples, out double min, out double max, out double avg)
        {
            double[] times = new double[samples];

            for (int i = 0; i < times.Length; ++i)
            {
                times[i] = TimeCode(init, run, flush, count) - _overhead;
            }

            Array.Sort(times);

            // discard lowest 20% and highest 20%

            int startAt = times.Length * 20 / 100;
            int endAt = times.Length * 80 / 100;

            max = times[startAt];
            min = times[startAt];
            avg = 0.0;
            int cnt = 0;

            for (int i = startAt; i < endAt; ++i)
            {
                max = Math.Max(max, times[i]);
                min = Math.Min(min, times[i]);
                avg += times[i];
                cnt++;
            }

            avg /= cnt;
        }

        private static void TimeAndDisplay(string name, XmlTextWriter xtw, RunDelegate init, RunDelegate run, RunDelegate flush, int count, int divider)
        {
            double min, max, avg;

            TimeAndDiscardUnusual(init, run, flush, count, 10, out min, out max, out avg);
            max /= divider;
            min /= divider;
            avg /= divider;
            Console.WriteLine("{0}: min={1}ns max={2}ns avg={3}ns", name, Math.Round(min, 3), Math.Round(max, 3), Math.Round(avg, 3));
            xtw.WriteStartElement("test");
            xtw.WriteAttributeString("name", name);
            xtw.WriteAttributeString("min", Convert.ToInt32(min).ToString());
            xtw.WriteAttributeString("max", Convert.ToInt32(max).ToString());
            xtw.WriteAttributeString("avg", Convert.ToInt32(avg).ToString());
            xtw.WriteEndElement();
            _maxmax = Math.Max(_maxmax, max);
        }

        public static int Main(string[] args)
        {
            try
            {
                XmlTextWriter xtw = new XmlTextWriter("results.xml", Encoding.UTF8);
                xtw.Formatting = Formatting.Indented;

                xtw.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"Graph.xsl\"");

                xtw.WriteStartElement("results");
                // DoBenchmark(xtw, new Log4NetWithFastLoggerBenchmark());
                DoBenchmark(xtw, new Log4NetBenchmark());
                DoBenchmark(xtw, new NLogBenchmark());
                xtw.WriteStartElement("scale");
                xtw.WriteAttributeString("max", Convert.ToInt32(_maxmax).ToString());
                xtw.WriteEndElement();
                xtw.WriteEndElement();
                xtw.Close();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex);
                return 1;
            }
        }

        private static void DoBenchmark(XmlTextWriter xtw, IBenchmark b)
        {
            Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();

            Console.WriteLine("Benchmark: {0}", b.Name);

            if (File.Exists("BenchmarkAssembly." + b.Name + ".dll"))
                File.Delete("BenchmarkAssembly." + b.Name + ".dll");

            CompilerParameters options = new CompilerParameters();
            options.OutputAssembly = "BenchmarkAssembly." + b.Name + ".dll";
            options.GenerateInMemory = true;
            options.GenerateExecutable = false;
            foreach (string s in b.References)
                options.ReferencedAssemblies.Add(s);
            options.CompilerOptions = "/optimize+";
            //options.IncludeDebugInformation = true;

            string sourceCode = GenerateTestSourceCode(b);
            
            using (StreamWriter sw = File.CreateText("BenchmarkAssembly." + b.Name + ".cs"))
            {
                sw.Write(sourceCode);
            }

            CompilerResults results = provider.CreateCompiler().CompileAssemblyFromSource(options, sourceCode);
            foreach (CompilerError ce in results.Errors)
            {
                Console.WriteLine("ERROR in line {0}: {1}", ce.Line, ce.ErrorText);
            }
            if (results.Errors.Count > 0)
            {
                Console.WriteLine("Errors in generated code for " + b.Name + " Ignoring.");
                return;
            }

            //Console.WriteLine("Compiled to assembly: {0}", results.CompiledAssembly.FullName);
            xtw.WriteStartElement("framework");
            xtw.WriteAttributeString("name", b.Name);

            Type t = results.CompiledAssembly.GetType("TheBenchmark");

            double min, max, avg;

            TimeAndDiscardUnusual(null, (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "DoNothing"), null, 100000, 10, out min, out max, out avg);
            _overhead = min;

            Console.WriteLine("overhead: {0}", _overhead);

            RunDelegate init = (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "Init");
            RunDelegate flush = (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "Flush");

            init();

            TimeAndDisplay("Guarded no logging", xtw, null, (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "GuardedNoLogging"), null, 100000, 5);
            TimeAndDisplay("Unguarded no logging", xtw, null, (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "NoLogging"), null, 100000, 5);
            TimeAndDisplay("Unguarded no logging with formatting 1", xtw, null, (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "NoLoggingWithFormatting1"), null, 10000, 5);
            TimeAndDisplay("Unguarded no logging with formatting 2", xtw, null, (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "NoLoggingWithFormatting2"), null, 10000, 5);
            TimeAndDisplay("Unguarded no logging with formatting 3", xtw, null, (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "NoLoggingWithFormatting3"), null, 10000, 5);
            TimeAndDisplay("Null target without rendering", xtw, init, (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "NoRenderingLoggingWithoutFormatting"), flush, 10000, 5);
            TimeAndDisplay("Null target without rendering 1", xtw, init, (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "NoRenderingLoggingWithFormatting1"), flush, 10000, 5);
            TimeAndDisplay("Null target without rendering 3", xtw, init, (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "NoRenderingLoggingWithFormatting3"), flush, 10000, 5);
            TimeAndDisplay("Null target with rendering", xtw, init, (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "NullLoggingWithoutFormatting"), flush, 1000, 5);
            TimeAndDisplay("Null target with rendering 1", xtw, init, (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "NullLoggingWithFormatting1"), flush, 1000, 5);
            TimeAndDisplay("Null target with rendering 3", xtw, init, (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "NullLoggingWithFormatting3"), flush, 1000, 5);
            TimeAndDisplay("Simple file", xtw, init, (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "SimpleFile"), flush, 10, 5);
            //TimeAndDisplay("Buffered file", xtw, init, (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "BufferedFile"), flush, 100, 5);
            //TimeAndDisplay("Asynchronous File without a flush", xtw, init, (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "AsyncFile"), null, 100, 5);
            //flush();
            //TimeAndDisplay("Asynchronous File with a flush", xtw, init, (RunDelegate)Delegate.CreateDelegate(typeof(RunDelegate), t, "AsyncFile"), flush, 5000, 5);

            xtw.WriteEndElement();
        }
    }

}
