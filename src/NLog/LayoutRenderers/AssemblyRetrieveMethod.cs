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


namespace NLog.LayoutRenderers
{
    /// <summary>
    /// 
    /// </summary>
    public enum AssemblyRetrieveMethod {
        /// <summary>
        /// Specifies that we want to get the assembly name
        /// of default executable.
        /// </summary>
        EntryAssembly,
#if !SILVERLIGHT && !SILVERLIGHT5
        /// <summary>
        /// Specifies that we want the assembly name
        /// for the assembly that contains the currently
        /// executing code
        /// </summary>
        ExecutingAssembly,
        /// <summary>
        /// Specifies that we want the assembly name
        /// for the assembly that invoked the current executing method
        /// </summary>
        CallingAssembly,
#endif
        /// <summary>
        /// Specifies that we want to use a pre-defined assembly name
        /// This should only be used for unit-testing as unit tests
        /// may not be able to retrieve an assembly name.
        /// 
        /// The value for this is: "ExampleAssembly, Version=1.0.0.0, Culture=en, PublicKeyToken=a5d015c7d5a0b012"
        /// </summary>
        TestAssembly,
        /// <summary>
        /// Specifies that we want to use a null assembly name.
        /// This is only useful for unit testing purposes.
        /// </summary>
        EmptyAssembly
    }
}