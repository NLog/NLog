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
 

using System;
using System.Globalization;
using NLog.Common;
using NLog.Layouts;

namespace NLog.Internal
{
    internal static class LayoutHelpers
    {
        
        /// <summary>
        /// Render the event info as parse as <c>short</c>
        /// </summary>
        /// <param name="layout">current layout</param>
        /// <param name="logEvent"></param>
        /// <param name="defaultValue">default value when the render </param>
        /// <param name="layoutName">layout name for log message to internal log when logging fails</param>
        /// <returns></returns>
        public static short RenderShort(this Layout layout, LogEventInfo logEvent, short defaultValue, string layoutName)
        {
            if (layout == null)
            {
                InternalLogger.Debug(layoutName + " is null so default value of " + defaultValue);
                return defaultValue;
            }
            if (logEvent == null)
            {
                InternalLogger.Debug(layoutName + ": logEvent is null so default value of " + defaultValue);
                return defaultValue;
            }

            var rendered = layout.Render(logEvent);
            short result;
  
            // NumberStyles.Integer is default of Convert.ToInt16
            // CultureInfo.InvariantCulture is backwards compatible.
            if (!short.TryParse(rendered, NumberStyles.Integer, CultureInfo.InvariantCulture, out result)) 
            {
                InternalLogger.Warn(layoutName + ": parse of value '" + rendered + "' failed, return " + defaultValue);
                return defaultValue;
            }
            return result;
        }
        
        /// <summary>
        /// Render the event info as parse as <c>int</c>
        /// </summary>
        /// <param name="layout">current layout</param>
        /// <param name="logEvent"></param>
        /// <param name="defaultValue">default value when the render </param>
        /// <param name="layoutName">layout name for log message to internal log when logging fails</param>
        /// <returns></returns>
        public static int RenderInt(this Layout layout, LogEventInfo logEvent, int defaultValue, string layoutName)
        {
            if (layout == null)
            {
                InternalLogger.Debug(layoutName + " is null so default value of " + defaultValue);
                return defaultValue;
            }
            if (logEvent == null)
            {
                InternalLogger.Debug(layoutName + ": logEvent is null so default value of " + defaultValue);
                return defaultValue;
            }

            var rendered = layout.Render(logEvent);
            int result;
  
            // NumberStyles.Integer is default of Convert.ToInt16
            // CultureInfo.InvariantCulture is backwards compatible.
            if (!int.TryParse(rendered, NumberStyles.Integer, CultureInfo.InvariantCulture, out result)) 
            {
                InternalLogger.Warn(layoutName + ": parse of value '" + rendered + "' failed, return " + defaultValue);
                return defaultValue;
            }
            return result;
        }
        
        /// <summary>
        /// Render the event info as parse as <c>bool</c>
        /// </summary>
        /// <param name="layout">current layout</param>
        /// <param name="logEvent"></param>
        /// <param name="defaultValue">default value when the render </param>
        /// <param name="layoutName">layout name for log message to internal log when logging fails</param>
        /// <returns></returns>
        public static bool RenderBool(this Layout layout, LogEventInfo logEvent, bool defaultValue, string layoutName)
        {
            if (layout == null)
            {
                InternalLogger.Debug(layoutName + " is null so default value of " + defaultValue);
                return defaultValue;
            }
            if (logEvent == null)
            {
                InternalLogger.Debug(layoutName + ": logEvent is null so default value of " + defaultValue);
                return defaultValue;
            }

            var rendered = layout.Render(logEvent);
            bool result;
  
            if (!bool.TryParse(rendered, out result)) 
            {
                InternalLogger.Warn(layoutName + ": parse of value '" + rendered + "' failed, return " + defaultValue);
                return defaultValue;
            }
            return result;
        }
    }
}
