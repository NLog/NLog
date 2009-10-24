// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NLog.Common;
using NLog.Config;
using NLog.Internal;
using NLog.LayoutRenderers;
using NLog.LayoutRenderers.Wrappers;

namespace NLog.Layouts
{
    /// <summary>
    /// Parses layout strings.
    /// </summary>
    internal sealed class LayoutParser
    {
        internal static LayoutRenderer[] CompileLayout(Tokenizer sr, bool isNested, out string text)
        {
            List<LayoutRenderer> result = new List<LayoutRenderer>();
            StringBuilder literalBuf = new StringBuilder();

            int ch;

            int p0 = sr.Position;

            while ((ch = sr.Peek()) != -1)
            {
                if (isNested && (ch == '}' || ch == ':'))
                {
                    break;
                }

                sr.Read();

                if (ch == '$' && sr.Peek() == '{')
                {
                    if (literalBuf.Length > 0)
                    {
                        result.Add(new LiteralLayoutRenderer(literalBuf.ToString()));
                        literalBuf.Length = 0;
                    }

                    LayoutRenderer newLayoutRenderer = ParseLayoutRenderer(sr);
                    if (newLayoutRenderer.IsAppDomainFixed())
                    {
                        newLayoutRenderer = ConvertToLiteral(newLayoutRenderer);
                    }

                    // layout renderer
                    result.Add(newLayoutRenderer);
                }
                else
                {
                    literalBuf.Append((char)ch);
                }
            }

            if (literalBuf.Length > 0)
            {
                result.Add(new LiteralLayoutRenderer(literalBuf.ToString()));
                literalBuf.Length = 0;
            }

            int p1 = sr.Position;

            MergeLiterals(result);
            text = sr.Substring(p0, p1);

            return result.ToArray();
        }

        private static string ParseLayoutRendererName(Tokenizer sr)
        {
            int ch;

            StringBuilder nameBuf = new StringBuilder();
            while ((ch = sr.Peek()) != -1)
            {
                if (ch == ':' || ch == '}')
                {
                    break;
                }

                nameBuf.Append((char)ch);
                sr.Read();
            }

            return nameBuf.ToString();
        }

        private static string ParseParameterName(Tokenizer sr)
        {
            int ch;
            int nestLevel = 0;

            StringBuilder nameBuf = new StringBuilder();
            while ((ch = sr.Peek()) != -1)
            {
                if ((ch == '=' || ch == '}' || ch == ':') && nestLevel == 0)
                {
                    break;
                }

                if (ch == '$')
                {
                    sr.Read();
                    nameBuf.Append('$');
                    if (sr.Peek() == '{')
                    {
                        nameBuf.Append('{');
                        nestLevel++;
                        sr.Read();
                    }

                    continue;
                }

                if (ch == '}')
                {
                    nestLevel--;
                }

                if (ch == '\\')
                {
                    // skip the backslash
                    sr.Read();

                    // append next character
                    nameBuf.Append((char)sr.Read());
                    continue;
                }

                nameBuf.Append((char)ch);
                sr.Read();
            }

            return nameBuf.ToString();
        }

        private static string ParseParameterValue(Tokenizer sr)
        {
            int ch;

            StringBuilder nameBuf = new StringBuilder();
            while ((ch = sr.Peek()) != -1)
            {
                if (ch == ':' || ch == '}')
                {
                    break;
                }

                if (ch == '\\')
                {
                    // skip the backslash
                    sr.Read();

                    // append next character
                    nameBuf.Append((char)sr.Read());
                    continue;
                }

                nameBuf.Append((char)ch);
                sr.Read();
            }

            return nameBuf.ToString();
        }

        private static LayoutRenderer ParseLayoutRenderer(Tokenizer sr)
        {
            int ch;

            ch = sr.Read();
            if (ch != '{')
            {
                throw new NLogConfigurationException("'{' expected in layout specification");
            }

            string name = ParseLayoutRendererName(sr);
            LayoutRenderer lr = NLogFactories.Default.LayoutRendererFactory.CreateInstance(name);

            Dictionary<Type, LayoutRenderer> wrappers = new Dictionary<Type, LayoutRenderer>();
            List<LayoutRenderer> orderedWrappers = new List<LayoutRenderer>();

            ch = sr.Read();
            while (ch != -1 && ch != '}')
            {
                string parameterName = ParseParameterName(sr).Trim();
                if (sr.Peek() == '=')
                {
                    sr.Read(); // skip the '='
                    PropertyInfo pi = null;
                    LayoutRenderer parameterTarget = lr;

                    if (!PropertyHelper.TryGetPropertyInfo(lr, parameterName, out pi))
                    {
                        Type wrapperType;

                        if (NLogFactories.Default.AmbientPropertyFactory.TryGetDefinition(parameterName, out wrapperType))
                        {
                            LayoutRenderer wrapperRenderer;

                            if (!wrappers.TryGetValue(wrapperType, out wrapperRenderer))
                            {
                                wrapperRenderer = NLogFactories.Default.AmbientPropertyFactory.CreateInstance(parameterName);
                                wrappers[wrapperType] = wrapperRenderer;
                                orderedWrappers.Add(wrapperRenderer);
                            }

                            if (!PropertyHelper.TryGetPropertyInfo(wrapperRenderer, parameterName, out pi))
                            {
                                pi = null;
                            }
                            else
                            {
                                parameterTarget = wrapperRenderer;
                            }
                        }
                    }

                    if (pi == null)
                    {
                        ParseParameterValue(sr);
                    }
                    else
                    {
                        if (typeof(Layout).IsAssignableFrom(pi.PropertyType))
                        {
                            SimpleLayout nestedLayout = new SimpleLayout();
                            string txt;
                            LayoutRenderer[] renderers = CompileLayout(sr, true, out txt);

                            nestedLayout.SetRenderers(renderers, txt);
                            pi.SetValue(parameterTarget, nestedLayout, null);
                        }
                        else
                        {
                            string value = ParseParameterValue(sr);
                            PropertyHelper.SetPropertyFromString(parameterTarget, parameterName, value, null);
                        }
                    }
                }
                else
                {
                    // what we've just read is not a parameterName, but a value
                    // assign it to a default property (denoted by empty string)
                    PropertyInfo pi;

                    if (PropertyHelper.TryGetPropertyInfo(lr, string.Empty, out pi))
                    {
                        if (typeof(SimpleLayout) == pi.PropertyType)
                        {
                            pi.SetValue(lr, new SimpleLayout(parameterName), null);
                        }
                        else
                        {
                            string value = parameterName;
                            PropertyHelper.SetPropertyFromString(lr, pi.Name, value, null);
                        }
                    }
                    else
                    {
                        InternalLogger.Warn("{0} has no default property", lr.GetType().FullName);
                    }
                }

                ch = sr.Read();
            }

            for (int i = orderedWrappers.Count - 1; i >= 0; --i)
            {
                WrapperLayoutRendererBase newRenderer = (WrapperLayoutRendererBase)orderedWrappers[i];
                InternalLogger.Trace("Wrapping {0} with {1}", lr.GetType().Name, newRenderer.GetType().Name);
                if (lr.IsAppDomainFixed())
                {
                    lr = ConvertToLiteral(lr);
                }

                newRenderer.Inner = new SimpleLayout(new LayoutRenderer[] { lr }, string.Empty);
                lr = newRenderer;
            }

            return lr;
        }

        private static void MergeLiterals(List<LayoutRenderer> list)
        {
            for (int i = 0; i + 1 < list.Count;)
            {
                LiteralLayoutRenderer lr1 = list[i] as LiteralLayoutRenderer;
                LiteralLayoutRenderer lr2 = list[i + 1] as LiteralLayoutRenderer;
                if (lr1 != null && lr2 != null)
                {
                    lr1.Text += lr2.Text;
                    list.RemoveAt(i + 1);
                }
                else
                {
                    i++;
                }
            }
        }

        private static LayoutRenderer ConvertToLiteral(LayoutRenderer renderer)
        {
            StringBuilder sb = new StringBuilder();
            renderer.Append(sb, LogEventInfo.CreateNullEvent());
            return new LiteralLayoutRenderer(sb.ToString());
        }

        /// <summary>
        /// Simple character tokenizer.
        /// </summary>
        internal class Tokenizer
        {
            private readonly string text;

            /// <summary>
            /// Initializes a new instance of the <see cref="Tokenizer" /> class.
            /// </summary>
            /// <param name="text">The text to be tokenized.</param>
            public Tokenizer(string text)
            {
                this.text = text;
                this.Position = 0;
            }

            internal int Position { get; private set; }

            internal int Peek()
            {
                if (this.Position < this.text.Length)
                {
                    return this.text[this.Position];
                }
                else
                {
                    return -1;
                }
            }

            internal int Read()
            {
                if (this.Position < this.text.Length)
                {
                    return this.text[this.Position++];
                }
                else
                {
                    return -1;
                }
            }

            internal string Substring(int p0, int p1)
            {
                return this.text.Substring(p0, p1 - p0);
            }
        }
    }
}
