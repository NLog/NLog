// 
// Copyright (c) 2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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
using System.Text;
using System.Collections;
using System.Reflection;
using System.Globalization;

using NLog.Internal;

namespace NLog.LayoutAppenders
{
    public sealed class LayoutAppenderFactory
    {
        private static TypeDictionary _appenders = new TypeDictionary();

        static LayoutAppenderFactory()
        {
			Clear();
            AddDefaultLayoutAppenders();
        }

		private LayoutAppenderFactory()
		{
		}

        public static void Clear() {
			_appenders.Clear();
        }

        public static void AddLayoutAppendersFromAssembly(Assembly theAssembly, string prefix) {
            InternalLogger.Debug("AddLayoutAppendersFromAssembly('{0}')", theAssembly.FullName);
            foreach (Type t in theAssembly.GetTypes()) {
                LayoutAppenderAttribute[] attributes = (LayoutAppenderAttribute[])t.GetCustomAttributes(typeof(LayoutAppenderAttribute), false);
                if (attributes != null) {
                    foreach (LayoutAppenderAttribute attr in attributes) {
                        AddLayoutAppender(prefix + attr.FormatString, t);
                    }
                }
            }
        }
        private static void AddDefaultLayoutAppenders() {
            AddLayoutAppendersFromAssembly(typeof(LayoutAppenderFactory).Assembly, String.Empty);
        }

        private static LayoutAppender CreateUnknownLayoutAppender(string name, string parameters) {
            return new LiteralLayoutAppender("[unknown layout appender:" + name + ":" + parameters + "]");
        }

        public static void AddLayoutAppender(string name, Type t) {
            InternalLogger.Debug("AddLayoutAppender('{0}','{1}')", name, t.FullName);
            _appenders[name] = t;
        }

        public static void ApplyLayoutAppenderParameters(LayoutAppender appender, string parameterString) {
            int pos = 0;
            Type appenderType = appender.GetType();

            while (pos < parameterString.Length) {
                int nameStartPos = pos;
                while (pos < parameterString.Length && Char.IsWhiteSpace(parameterString[pos])) {
                    pos++;
                }
                while (pos < parameterString.Length && parameterString[pos] != '=') {
                    pos++;
                }
                int nameEndPos = pos;
                if (nameStartPos == nameEndPos)
                    break;

                pos++;

                // we've got a name - now get a value
                //

                int valueStartPos = pos;
                StringBuilder valueBuf = new StringBuilder();
                while (pos < parameterString.Length) {
                    if (parameterString[pos] == '\\')
                    {
                        valueBuf.Append(parameterString[pos + 1]);
                        pos += 2;
                    } else if (parameterString[pos] == ':') {
                        pos++;
                        break;
                    } else {
                        valueBuf.Append(parameterString[pos]);
                        pos++;
                    }
                }

                string name = parameterString.Substring(nameStartPos, nameEndPos - nameStartPos);
                string value= valueBuf.ToString();

                PropertyHelper.SetPropertyFromString(appender, name, value);
            }
        }

        public static LayoutAppender CreateLayoutAppender(string name, string parameters) {
            Type t = _appenders[name];
            if (t != null) {
                object o = Activator.CreateInstance(t);
                if (o is LayoutAppender) {
                    LayoutAppender la = (LayoutAppender)o;

                    if (parameters != null && parameters.Length > 0)
                    {
                        ApplyLayoutAppenderParameters(la, parameters);
                    }

                    return la;
                } else {
                    return CreateUnknownLayoutAppender(name, parameters);
                }
            }
            return CreateUnknownLayoutAppender(name, parameters);
        }
    }
}
