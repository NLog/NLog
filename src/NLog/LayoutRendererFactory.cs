// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Globalization;

using NLog.Internal;
using NLog.LayoutRenderers;
using NLog.Config;

namespace NLog
{
    /// <summary>
    /// A factory for layout renderers.  Creates new layout renderers based on their names.
    /// </summary>
    public sealed class LayoutRendererFactory
    {
        private static TypeDictionary _targets = new TypeDictionary();

        static LayoutRendererFactory()
        {
            foreach (Assembly a in ExtensionUtils.GetExtensionAssemblies())
            {
                AddLayoutRenderersFromAssembly(a, "");
            }
        }

        private LayoutRendererFactory(){}

        /// <summary>
        /// Removes all layout renderer information from the factory.
        /// </summary>
        public static void Clear()
        {
            _targets.Clear();
        }

        /// <summary>
        /// Scans the specified assembly for types marked with <see cref="LayoutRendererAttribute" /> and adds
        /// them to the factory. Optionally it prepends the specified text to layout renderer names to avoid
        /// naming collisions.
        /// </summary>
        /// <param name="theAssembly">The assembly to be scanned for layout renderers.</param>
        /// <param name="prefix">The prefix to be prepended to layout renderer names.</param>
        public static void AddLayoutRenderersFromAssembly(Assembly theAssembly, string prefix)
        {
            try
            {
                InternalLogger.Debug("AddLayoutRenderersFromAssembly('{0}')", theAssembly.FullName);
                foreach (Type t in theAssembly.GetTypes())
                {
                    LayoutRendererAttribute[]attributes = (LayoutRendererAttribute[])t.GetCustomAttributes(typeof(LayoutRendererAttribute), false);
                    if (attributes != null)
                    {
                        foreach (LayoutRendererAttribute attr in attributes)
                        {
                            if (PlatformDetector.IsSupportedOnCurrentRuntime(t))
                            {
                                AddLayoutRenderer(prefix + attr.FormatString, t);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Failed to add layout renderers from '" + theAssembly.FullName + "': {0}", ex);
            }
        }

        private static LayoutRenderer CreateUnknownLayoutRenderer(string name, string parameters)
        {
            return new LiteralLayoutRenderer("");
        }

        /// <summary>
        /// Registers the specified layout renderer type to the factory under a specified name.
        /// </summary>
        /// <param name="name">The name of the layout renderer (e.g. <code>logger</code>, <code>message</code> or <code>aspnet-request</code>)</param>
        /// <param name="t">The type of the new layout renderer</param>
        /// <remarks>
        /// The name specified in the name parameter can then be used
        /// to create layout renderers.
        /// </remarks>
        public static void AddLayoutRenderer(string name, Type t)
        {
            InternalLogger.Trace("Registering layout renderer {0} for type '{1}')", name, t.FullName);
            _targets[name.ToLower()] = t;
        }

        private static void ApplyLayoutRendererParameters(LayoutRenderer target, string parameterString)
        {
            int pos = 0;

            while (pos < parameterString.Length)
            {
                int nameStartPos = pos;
                while (pos < parameterString.Length && Char.IsWhiteSpace(parameterString[pos]))
                {
                    pos++;
                }
                while (pos < parameterString.Length && parameterString[pos] != '=')
                {
                    pos++;
                }
                int nameEndPos = pos;
                if (nameStartPos == nameEndPos)
                    break;

                pos++;

                // we've got a name - now get a value
                //

                StringBuilder valueBuf = new StringBuilder();
                while (pos < parameterString.Length)
                {
                    if (parameterString[pos] == '\\')
                    {
                        valueBuf.Append(parameterString[pos + 1]);
                        pos += 2;
                    }
                    else if (parameterString[pos] == ':')
                    {
                        pos++;
                        break;
                    }
                    else
                    {
                        valueBuf.Append(parameterString[pos]);
                        pos++;
                    }
                }

                string name = parameterString.Substring(nameStartPos, nameEndPos - nameStartPos);
                string value = valueBuf.ToString();

                PropertyHelper.SetPropertyFromString(target, name, value, null);
            }
        }

        /// <summary>
        /// Creates the layout renderer object based on its layout renderer name and sets its properties from parameters string.
        /// </summary>
        /// <param name="name">The name of the layout renderer (e.g. <code>message</code> or <code>aspnet-request</code>)</param>
        /// <param name="parameters">Parameters to the layout renderer.</param>
        /// <returns>A new instance of the <see cref="LayoutRenderer"/> object.</returns>
        public static LayoutRenderer CreateLayoutRenderer(string name, string parameters)
        {
            Type t = _targets[name.ToLower()];
            if (t != null)
            {
                LayoutRenderer la = FactoryHelper.CreateInstance(t) as LayoutRenderer;
                if (la != null)
                {

                    if (parameters != null && parameters.Length > 0)
                    {
                        ApplyLayoutRendererParameters(la, parameters);
                    }

                    return la;
                }
                else
                {
                    return CreateUnknownLayoutRenderer(name, parameters);
                }
            }
            return CreateUnknownLayoutRenderer(name, parameters);
        }
    }
}
