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
    [assembly: AssemblyTitle("NLog for .NET Framework 2.0")]
#elif NET3_5
    [assembly: AssemblyTitle("NLog for .NET Framework 3.5")]
#elif NET4_0
    [assembly: AssemblyTitle("NLog for .NET Framework 4")]
#elif MONO_2_0
    [assembly: AssemblyTitle("NLog for Mono 2.0")]
#elif NETCF2_0
    [assembly: AssemblyTitle("NLog for .NET Compact Framework 2.0")]
#elif NETCF3_5
    [assembly: AssemblyTitle("NLog for .NET Compact Framework 3.5")]
#elif SILVERLIGHT2
    [assembly: AssemblyTitle("NLog for Silverlight 2.0")]
#elif SILVERLIGHT3
    [assembly: AssemblyTitle("NLog for Silverlight 3.0")]
#elif SILVERLIGHT4
    [assembly: AssemblyTitle("NLog for Silverlight 4.0")]
#elif WINDOWS_PHONE_7
    [assembly: AssemblyTitle("NLog for Windows Phone 7")]
#elif WINDOWS_PHONE_7_1
    [assembly: AssemblyTitle("NLog for Windows Phone 7.1")]
#elif DOCUMENTATION
    [assembly: AssemblyTitle("NLog Documentation")]
#else
#error Unrecognized build target - please update AssemblyInfo.cs
#endif

[assembly: AssemblyDescription("NLog")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("NLog")]
[assembly: AssemblyCopyright("Copyright (c) 2004-2011 by Jaroslaw Kowalski")]
[assembly: AssemblyCulture("")]

[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]

[assembly: InternalsVisibleTo("NLog.UnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100ef8eab4fbdeb511eeb475e1659fe53f00ec1c1340700f1aa347bf3438455d71993b28b1efbed44c8d97a989e0cb6f01bcb5e78f0b055d311546f63de0a969e04cf04450f43834db9f909e566545a67e42822036860075a1576e90e1c43d43e023a24c22a427f85592ae56cac26f13b7ec2625cbc01f9490d60f16cfbb1bc34d9")]