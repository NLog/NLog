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

namespace NLog.Config
{
    using System;

    /// <summary>
    /// Marks the layout or layout renderer as thread independent - it producing correct results 
    /// regardless of the thread it's running on. 
    ///
    /// Without this attribute everything is rendered on the main thread.
    /// </summary>
    /// <remarks>
    /// If this attribute is set on a layout, it could be rendered on the another thread. 
    /// This could be more efficient as it's skipped when not needed.
    /// 
    /// If context like <c>HttpContext.Current</c> is needed, which is only available on the main thread, this attribute should not be applied.
    ///
    /// See the AsyncTargetWrapper and BufferTargetWrapper with the <see cref="NLog.Targets.Target.PrecalculateVolatileLayouts"/> , using <see cref="NLog.Layouts.Layout.Precalculate"/>
    /// 
    /// Apply this attribute when:
    /// - The result can we rendered in another thread. Delaying this could be more efficient. And/Or,
    /// - The result should not be precalculated, for example the target sends some extra context information. 
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ThreadAgnosticAttribute : Attribute
    {
    }
}
