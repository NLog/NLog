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
    internal static class LayoutParser
    {
        internal static LayoutRenderer[] CompileLayout(ConfigurationItemFactory configurationItemFactory, SimpleStringReader sr, bool? throwConfigExceptions, bool isNested, out string text)
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
                        if (EndOfLayout(nextChar))
                        {
                            //read next char and append
                            sr.Read();
                            literalBuf.Append((char)nextChar);
                        }
                        else
                        {
                            //don't treat \ as escape char and just read it
                            literalBuf.Append('\\');
                        }
                        continue;
                    }

                    if (EndOfLayout(ch))
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
                    //stash already found layout-renderer.
                    AddLiteral(literalBuf, result);

                    LayoutRenderer newLayoutRenderer = ParseLayoutRenderer(configurationItemFactory, sr, throwConfigExceptions);
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

            AddLiteral(literalBuf, result);

            int p1 = sr.Position;

            MergeLiterals(result);
            text = sr.Substring(p0, p1);

            return result.ToArray();
        }

        /// <summary>
        /// Add <see cref="LiteralLayoutRenderer"/> to <paramref name="result"/>
        /// </summary>
        /// <param name="literalBuf"></param>
        /// <param name="result"></param>
        private static void AddLiteral(StringBuilder literalBuf, List<LayoutRenderer> result)
        {
            if (literalBuf.Length > 0)
            {
                result.Add(new LiteralLayoutRenderer(literalBuf.ToString()));
                literalBuf.Length = 0;
            }
        }

        private static bool EndOfLayout(int ch)
        {
            return ch == '}' || ch == ':';
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
                    sr.Read();

                    // issue#3193
                    if (nestLevel != 0)
                    {
                        nameBuf.Append((char)ch);
                    }
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
                // https://blogs.msdn.com/b/csharpfaq/archive/2004/03/12/what-character-escape-sequences-are-available.aspx
                if (ch == '\\')
                {
                    // skip the backslash
                    sr.Read();

                    var nextChar = (char)sr.Peek();

                    switch (nextChar)
                    {
                        case ':':
                        case '{':
                        case '}':
                        case '\'':
                        case '"':
                        case '\\':
                            sr.Read();
                            nameBuf.Append(nextChar);
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

        private static LayoutRenderer ParseLayoutRenderer(ConfigurationItemFactory configurationItemFactory, SimpleStringReader stringReader, bool? throwConfigExceptions)
        {
            int ch = stringReader.Read();
            Debug.Assert(ch == '{', "'{' expected in layout specification");

            string name = ParseLayoutRendererName(stringReader);
            var layoutRenderer = GetLayoutRenderer(configurationItemFactory, name, throwConfigExceptions);

            var wrappers = new Dictionary<Type, LayoutRenderer>();
            var orderedWrappers = new List<LayoutRenderer>();

            ch = stringReader.Read();
            while (ch != -1 && ch != '}')
            {
                string parameterName = ParseParameterName(stringReader).Trim();
                if (stringReader.Peek() == '=')
                {
                    stringReader.Read(); // skip the '='
                    PropertyInfo propertyInfo;
                    LayoutRenderer parameterTarget = layoutRenderer;

                    if (!PropertyHelper.TryGetPropertyInfo(layoutRenderer, parameterName, out propertyInfo))
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

                            if (!PropertyHelper.TryGetPropertyInfo(wrapperRenderer, parameterName, out propertyInfo))
                            {
                                propertyInfo = null;
                            }
                            else
                            {
                                parameterTarget = wrapperRenderer;
                            }
                        }
                    }

                    if (propertyInfo == null)
                    {
                        ParseParameterValue(stringReader);
                    }
                    else
                    {
                        if (typeof(Layout).IsAssignableFrom(propertyInfo.PropertyType))
                        {
                            var nestedLayout = new SimpleLayout();
                            string txt;
                            LayoutRenderer[] renderers = CompileLayout(configurationItemFactory, stringReader, throwConfigExceptions, true, out txt);

                            nestedLayout.SetRenderers(renderers, txt);
                            propertyInfo.SetValue(parameterTarget, nestedLayout, null);
                        }
                        else if (typeof(ConditionExpression).IsAssignableFrom(propertyInfo.PropertyType))
                        {
                            var conditionExpression = ConditionParser.ParseExpression(stringReader, configurationItemFactory);
                            propertyInfo.SetValue(parameterTarget, conditionExpression, null);
                        }
                        else
                        {
                            string value = ParseParameterValue(stringReader);
                            PropertyHelper.SetPropertyFromString(parameterTarget, parameterName, value, configurationItemFactory);
                        }
                    }
                }
                else
                {
                    SetDefaultPropertyValue(configurationItemFactory, layoutRenderer, parameterName);
                }

                ch = stringReader.Read();
            }

            layoutRenderer = ApplyWrappers(configurationItemFactory, layoutRenderer, orderedWrappers);

            return layoutRenderer;
        }

        private static LayoutRenderer GetLayoutRenderer(ConfigurationItemFactory configurationItemFactory, string name, bool? throwConfigExceptions)
        {
            LayoutRenderer layoutRenderer;
            try
            {
                layoutRenderer = configurationItemFactory.LayoutRenderers.CreateInstance(name);
            }
            catch (Exception ex)
            {
                if (throwConfigExceptions ?? LogManager.ThrowConfigExceptions ?? LogManager.ThrowExceptions)
                {
                    throw;  // TODO NLog 5.0 throw NLogConfigurationException. Maybe also include entire input layout-string (if not too long)
                }
                InternalLogger.Error(ex, "Error parsing layout {0} will be ignored.", name);
                // replace with empty values
                layoutRenderer = new LiteralLayoutRenderer(string.Empty);
            }
            return layoutRenderer;
        }

        private static void SetDefaultPropertyValue(ConfigurationItemFactory configurationItemFactory, LayoutRenderer layoutRenderer, string parameterName)
        {
            // what we've just read is not a parameterName, but a value
            // assign it to a default property (denoted by empty string)
            PropertyInfo propertyInfo;

            if (PropertyHelper.TryGetPropertyInfo(layoutRenderer, string.Empty, out propertyInfo))
            {
                if (typeof(SimpleLayout) == propertyInfo.PropertyType)
                {
                    propertyInfo.SetValue(layoutRenderer, new SimpleLayout(parameterName), null);
                }
                else
                {
                    string value = parameterName;
                    PropertyHelper.SetPropertyFromString(layoutRenderer, propertyInfo.Name, value, configurationItemFactory);
                }
            }
            else
            {
                InternalLogger.Warn("{0} has no default property", layoutRenderer.GetType().FullName);
            }
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
            foreach (IRenderable renderable in ObjectGraphScanner.FindReachableObjects<IRenderable>(true, lr))
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
            for (int i = 0; i + 1 < list.Count;)
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
