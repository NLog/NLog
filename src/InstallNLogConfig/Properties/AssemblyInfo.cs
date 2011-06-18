// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET2_0
    [assembly: AssemblyTitle("NLog configuration installer for .NET Framework 2.0")]
#elif NET3_5
    [assembly: AssemblyTitle("NLog configuration installer for .NET Framework 3.5")]
#elif NET4_0
    [assembly: AssemblyTitle("NLog configuration installer for .NET Framework 4")]
#elif MONO_2_0
    [assembly: AssemblyTitle("NLog configuration installer for Mono 2.0")]
#elif NETCF2_0
    [assembly: AssemblyTitle("NLog configuration installer for .NET Compact Framework 2.0")]
#elif NETCF3_5
    [assembly: AssemblyTitle("NLog configuration installer for .NET Compact Framework 3.5")]
#elif SILVERLIGHT2
    [assembly: AssemblyTitle("NLog configuration installer for Silverlight 2.0")]
#elif SILVERLIGHT3
    [assembly: AssemblyTitle("NLog configuration installer for Silverlight 3.0")]
#elif SILVERLIGHT4
    [assembly: AssemblyTitle("NLog configuration installer for Silverlight 4.0")]
#elif DOCUMENTATION
    [assembly: AssemblyTitle("NLog Documentation")]
#else
#error Unrecognized build target - please update AssemblyInfo.cs
#endif

[assembly: AssemblyDescription("NLog configuration installer")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("NLog")]
[assembly: AssemblyCopyright("Copyright (c) 2004-2010 by Jaroslaw Kowalski")]
[assembly: AssemblyCulture("")]

[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]