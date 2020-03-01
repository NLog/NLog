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

using NLog.Common;

namespace NLog.Layouts
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class LayoutExtensions
    {
        /// <summary>
        /// Get the value, or if <paramref name="l"/>is <c>null</c>, the <paramref name="defaultValue"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="l">The item to render the value from</param>
        /// <param name="logEvent">Log event needed for rendering</param>
        /// <param name="defaultValue">The default value when <paramref name="l"/>is <c>null</c></param>
        /// <returns></returns>
        public static T ToValueOrDefault<T>(this IToValue<T> l, AsyncLogEventInfo logEvent, T defaultValue = default(T))
        {
            return ToValueOrDefault(l, logEvent.LogEvent, defaultValue);
        }

        /// <summary>
        /// Get the value, or if <paramref name="l"/>is <c>null</c>, the <paramref name="defaultValue"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="l">The item to render the value from</param>
        /// <param name="logEvent">Log event needed for rendering</param>
        /// <param name="defaultValue">The default value when <paramref name="l"/>is <c>null</c></param>
        /// <returns></returns>
        public static T ToValueOrDefault<T>(this IToValue<T> l, LogEventInfo logEvent, T defaultValue = default(T))
        {
            if (l == null)
            {
                return defaultValue;
            }

            return l.ToValue(logEvent);
        }
    }
}