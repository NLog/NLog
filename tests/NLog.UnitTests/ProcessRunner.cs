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

#if !NETSTANDARD

namespace NLog.UnitTests
{

    using System;
    using System.CodeDom.Compiler;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using Microsoft.CSharp;
    using Xunit;

    static class ProcessRunner
    {
        static ProcessRunner()
        {
            string sourceCode = @"
using System;
using System.Reflection;

class C1
{
    static int Main(string[] args)
    {
        try
        {
            if (args.Length < 3)
                throw new Exception(""Usage: Runner.exe \""AssemblyName\"" \""ClassName\"" \""MethodName\"" \""Parameter1...N\"""");

            string assemblyName = args[0];
            string className = args[1];
            string methodName = args[2];
            object[] arguments = new object[args.Length - 3];
            for (int i = 0; i < arguments.Length; ++i)
                arguments[i] = args[3 + i];

            Assembly assembly = Assembly.Load(assemblyName);
            Type type = assembly.GetType(className);
            if (type == null)
                throw new Exception(className + "" not found in "" + assemblyName);
            MethodInfo method = type.GetMethod(methodName);
            if (method == null)
                throw new Exception(methodName + "" not found in "" + type);
            object targetObject = null;
            if (!method.IsStatic)
                targetObject = Activator.CreateInstance(type);
            method.Invoke(targetObject, arguments);
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return 1;
        }
    }
}";
            CSharpCodeProvider provider = new CSharpCodeProvider();
            var options = new CompilerParameters();
            options.OutputAssembly = "Runner.exe";
            options.GenerateExecutable = true;
            options.IncludeDebugInformation = true;
            // To allow debugging the generated Runner.exe we need to keep files.
            // See https://stackoverflow.com/questions/875723/how-to-debug-break-in-codedom-compiled-code
            options.TempFiles = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true);
            var results = provider.CompileAssemblyFromSource(options, sourceCode);
            Assert.False(results.Errors.HasWarnings);
            Assert.False(results.Errors.HasErrors);
        }

        public static Process SpawnMethod(Type type, string methodName, params string[] p)
        {
            string assemblyName = type.Assembly.FullName;
            string typename = type.FullName;
            StringBuilder sb = new StringBuilder();
#if MONO
            sb.AppendFormat("\"{0}\" ", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Runner.exe"));
#endif
            sb.AppendFormat("\"{0}\" \"{1}\" \"{2}\"", assemblyName, typename, methodName);
            foreach (string s in p)
            {
                sb.Append(" ");
                sb.Append("\"");
                sb.Append(s);
                sb.Append("\"");
            }

            Process proc = new Process();
            proc.StartInfo.Arguments = sb.ToString();
#if MONO
            proc.StartInfo.FileName = "mono";
#else
            proc.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Runner.exe");
#endif
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.CreateNoWindow = true;
            // Hint:
            // In case we wanna redirect stdout we should drain the redirected pipe continously.
            // Otherwise Runner.exe's console buffer is full rather fast, leading to a lock within Console.Write(Line).
            proc.Start();
            return proc;
        }
    }
}

#endif