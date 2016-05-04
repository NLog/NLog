// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Layouts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using NLog.Common;
    using NLog.Conditions;
    using NLog.Config;
    using NLog.Internal;
    using NLog.LayoutRenderers;
    using NLog.LayoutRenderers.Wrappers;

    /// <summary>
    /// Parses layout strings.
    /// </summary>
    internal sealed class LayoutParser
    {
        internal static LayoutRenderer[] CompileLayout(ConfigurationItemFactory configurationItemFactory, SimpleStringReader sr, bool isNested, out string text)
        {
            var result = new List<LayoutRenderer>();
            var literalBuf = new StringBuilder();

            int ch;

            int p0 = sr.Position;

            while ((ch = sr.Peek()) != -1)
            {
                if (isNested)
                {
                    //possible escape char `\` 
                    if (ch == '\\')
                    {
                        sr.Read();
                        var nextChar = sr.Peek();

                        //escape chars
                        if (nextChar == '}' || nextChar == ':')
                        {
                            //read next char and append
                            sr.Read();
                            literalBuf.Append((char)nextChar);
                        }
                        else
                        {
                            //dont treat \ as escape char and just read it
                            literalBuf.Append('\\');
                        }
                        continue;
                    }

                    if (ch == '}' || ch == ':')
                    {
                        //end of innerlayout. 
                        // `}` is when double nested inner layout. 
                        // `:` when single nested layout
                        break;
                    }
                }

                sr.Read();

                //detect `${` (new layout-renderer)
                if (ch == '$' && sr.Peek() == '{')
                {
                    //stach already found layout-renderer.
                    if (literalBuf.Length > 0)
                    {
                        result.Add(new LiteralLayoutRenderer(literalBuf.ToString()));
                        literalBuf.Length = 0;
                    }

                    LayoutRenderer newLayoutRenderer = ParseLayoutRenderer(configurationItemFactory, sr);
                    if (CanBeConvertedToLiteral(newLayoutRenderer))
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

        private static string ParseLayoutRendererName(SimpleStringReader sr)
        {
            int ch;

            var nameBuf = new StringBuilder();
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

        private static string ParseParameterName(SimpleStringReader sr)
        {
            int ch;
            int nestLevel = 0;

            var nameBuf = new StringBuilder();
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

        private static string ParseParameterValue(SimpleStringReader sr)
        {
            int ch;

            var nameBuf = new StringBuilder();
            while ((ch = sr.Peek()) != -1)
            {
                if (ch == ':' || ch == '}')
                {
                    break;
                }

                // Code in this condition was replaced
                // to support escape codes e.g. '\r' '\n' '\u003a',
                // which can not be used directly as they are used as tokens by the parser
                // All escape codes listed in the following link were included
                // in addition to "\{", "\}", "\:" which are NLog specific:
                // http://blogs.msdn.com/b/csharpfaq/archive/2004/03/12/what-character-escape-sequences-are-available.aspx
                if (ch == '\\')
                {
                    // skip the backslash
                    sr.Read();

                    var nextChar = (char)sr.Peek();

                    switch (nextChar)
                    {
                        case ':':
                            sr.Read();
                            nameBuf.Append(':');
                            break;
                        case '{':
                            sr.Read();
                            nameBuf.Append('{');
                            break;
                        case '}':
                            sr.Read();
                            nameBuf.Append('}');
                            break;

                        case '\'':
                            sr.Read();
                            nameBuf.Append('\'');
                            break;
                        case '"':
                            sr.Read();
                            nameBuf.Append('"');
                            break;
                        case '\\':
                            sr.Read();
                            nameBuf.Append('\\');
                            break;
                        case '0':
                            sr.Read();
                            nameBuf.Append('\0');
                            break;
                        case 'a':
                            sr.Read();
                            nameBuf.Append('\a');
                            break;
                        case 'b':
                            sr.Read();
                            nameBuf.Append('\b');
                            break;
                        case 'f':
                            sr.Read();
                            nameBuf.Append('\f');
                            break;
                        case 'n':
                            sr.Read();
                            nameBuf.Append('\n');
                            break;
                        case 'r':
                            sr.Read();
                            nameBuf.Append('\r');
                            break;
                        case 't':
                            sr.Read();
                            nameBuf.Append('\t');
                            break;
                        case 'u':
                            sr.Read();
                            var uChar = GetUnicode(sr, 4); // 4 digits
                            nameBuf.Append(uChar);
                            break;
                        case 'U':
                            sr.Read();
                            var UChar = GetUnicode(sr, 8); // 8 digits
                            nameBuf.Append(UChar);
                            break;
                        case 'x':
                            sr.Read();
                            var xChar = GetUnicode(sr, 4); // 1-4 digits
                            nameBuf.Append(xChar);
                            break;
                        case 'v':
                            sr.Read();
                            nameBuf.Append('\v');
                            break;
                    }

                    continue;
                }

                nameBuf.Append((char)ch);
                sr.Read();
            }

            return nameBuf.ToString();
        }

        private static char GetUnicode(SimpleStringReader sr, int maxDigits)
        {
            int code = 0;

            for (int cnt = 0; cnt < maxDigits; cnt++)
            {
                var digitCode = sr.Peek();
                if (digitCode >= (int)'0' && digitCode <= (int)'9')
                    digitCode = digitCode - (int)'0';
                else if (digitCode >= (int)'a' && digitCode <= (int)'f')
                    digitCode = digitCode - (int)'a' + 10;
                else if (digitCode >= (int)'A' && digitCode <= (int)'F')
                    digitCode = digitCode - (int)'A' + 10;
                else
                    break;

                sr.Read();
                code = code * 16 + digitCode;
            }

            return (char)code;
        }

        private static LayoutRenderer ParseLayoutRenderer(ConfigurationItemFactory configurationItemFactory, SimpleStringReader sr)
        {
            int ch = sr.Read();
            Debug.Assert(ch == '{', "'{' expected in layout specification");

            string name = ParseLayoutRendererName(sr);
            LayoutRenderer lr = configurationItemFactory.LayoutRenderers.CreateInstance(name);

            var wrappers = new Dictionary<Type, LayoutRenderer>();
            var orderedWrappers = new List<LayoutRenderer>();

            ch = sr.Read();
            while (ch != -1 && ch != '}')
            {
                string parameterName = ParseParameterName(sr).Trim();
                if (sr.Peek() == '=')
                {
                    sr.Read(); // skip the '='
                    PropertyInfo pi;
                    LayoutRenderer parameterTarget = lr;

                    if (!PropertyHelper.TryGetPropertyInfo(lr, parameterName, out pi))
                    {
                        Type wrapperType;

                        if (configurationItemFactory.AmbientProperties.TryGetDefinition(parameterName, out wrapperType))
                        {
                            LayoutRenderer wrapperRenderer;

                            if (!wrappers.TryGetValue(wrapperType, out wrapperRenderer))
                            {
                                wrapperRenderer = configurationItemFactory.AmbientProperties.CreateInstance(parameterName);
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
                            var nestedLayout = new SimpleLayout();
                            string txt;
                            LayoutRenderer[] renderers = CompileLayout(configurationItemFactory, sr, true, out txt);

                            nestedLayout.SetRenderers(renderers, txt);
                            pi.SetValue(parameterTarget, nestedLayout, null);
                        }
                        else if (typeof(ConditionExpression).IsAssignableFrom(pi.PropertyType))
                        {
                            var conditionExpression = ConditionParser.ParseExpression(sr, configurationItemFactory);
                            pi.SetValue(parameterTarget, conditionExpression, null);
                        }
                        else
                        {
                            string value = ParseParameterValue(sr);
                            PropertyHelper.SetPropertyFromString(parameterTarget, parameterName, value, configurationItemFactory);
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
                            PropertyHelper.SetPropertyFromString(lr, pi.Name, value, configurationItemFactory);
                        }
                    }
                    else
                    {
                        InternalLogger.Warn("{0} has no default property", lr.GetType().FullName);
                    }
                }

                ch = sr.Read();
            }

            lr = ApplyWrappers(configurationItemFactory, lr, orderedWrappers);

            return lr;
        }

        private static LayoutRenderer ApplyWrappers(ConfigurationItemFactory configurationItemFactory, LayoutRenderer lr, List<LayoutRenderer> orderedWrappers)
        {
            for (int i = orderedWrappers.Count - 1; i >= 0; --i)
            {
                var newRenderer = (WrapperLayoutRendererBase)orderedWrappers[i];
                InternalLogger.Trace("Wrapping {0} with {1}", lr.GetType().Name, newRenderer.GetType().Name);
                if (CanBeConvertedToLiteral(lr))
                {
                    lr = ConvertToLiteral(lr);
                }

                newRenderer.Inner = new SimpleLayout(new[] { lr }, string.Empty, configurationItemFactory);
                lr = newRenderer;
            }

            return lr;
        }

        private static bool CanBeConvertedToLiteral(LayoutRenderer lr)
        {
            foreach (IRenderable renderable in ObjectGraphScanner.FindReachableObjects<IRenderable>(lr))
            {
                if (renderable.GetType() == typeof(SimpleLayout))
                {
                    continue;
                }

                if (!renderable.GetType().IsDefined(typeof(AppDomainFixedOutputAttribute), false))
                {
                    return false;
                }
            }

            return true;
        }

        private static void MergeLiterals(List<LayoutRenderer> list)
        {
            for (int i = 0; i + 1 < list.Count; )
            {
                var lr1 = list[i] as LiteralLayoutRenderer;
                var lr2 = list[i + 1] as LiteralLayoutRenderer;
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
            return new LiteralLayoutRenderer(renderer.Render(LogEventInfo.CreateNullEvent()));
        }
    }
}
