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

using NLog.LayoutAppenders;

namespace NLog {
    public class Layout {
        public Layout() {
            Text = String.Empty;
        }

        public Layout(string txt) {
            Text = txt;
        }

        private string _layoutText;
        private LayoutAppenderCollection _layoutAppenders;
        private int _needsStackTrace = 0;

        public string Text
        {
            get {
                return _layoutText;
            }
            set {
                _layoutText = value;
                _layoutAppenders = CompileLayout(_layoutText, out _needsStackTrace);
            }
        }

        public string GetFormattedMessage(LogEventInfo ev) {
            int size = 0;

            foreach (LayoutAppender app in _layoutAppenders) {
                int ebs = app.GetEstimatedBufferSize(ev);
                size += ebs;
            }

            StringBuilder builder = new StringBuilder(size);

            foreach (LayoutAppender app in _layoutAppenders) {
                app.Append(builder, ev);
            }

            return builder.ToString();
        }

        private static LayoutAppenderCollection CompileLayout(string s, out int needsStackTrace) {
            LayoutAppenderCollection result = new LayoutAppenderCollection();
            needsStackTrace = 0;

            int startingPos = 0;
            int pos = s.IndexOf("${", startingPos);

            while (pos >= 0) {
                if (pos != startingPos) {
                    result.Add(new LiteralLayoutAppender(s.Substring(startingPos, pos - startingPos)));
                }
                int pos2 = s.IndexOf("}", pos + 2);
                if (pos2 >= 0) {
                    startingPos = pos2 + 1;
                    string item = s.Substring(pos + 2, pos2 - pos - 2);
                    int paramPos = item.IndexOf(':');
                    string layoutAppenderName = item;
                    string layoutAppenderParams = null;
                    if (paramPos >= 0) {
                        layoutAppenderParams = layoutAppenderName.Substring(paramPos + 1);
                        layoutAppenderName = layoutAppenderName.Substring(0, paramPos);
                    }

                    LayoutAppender newLayoutAppender = LayoutAppenderFactory.CreateLayoutAppender(layoutAppenderName, layoutAppenderParams);
                    int nst = newLayoutAppender.NeedsStackTrace();
                    if (nst > needsStackTrace)
                        needsStackTrace = nst;

                    result.Add(newLayoutAppender);
                    pos = s.IndexOf("${", startingPos);
                } else {
                    break;
                }
            }
            if (startingPos != s.Length) {
                result.Add(new LiteralLayoutAppender(s.Substring(startingPos, s.Length - startingPos)));
            }

            return result;
        }

        public int NeedsStackTrace() {
            return _needsStackTrace;
        }
    }
}
