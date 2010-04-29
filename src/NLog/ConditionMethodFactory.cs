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
using System.Reflection;
using System.Globalization;

using NLog.Internal;
using NLog.Config;

namespace NLog
{
    /// <summary>
    /// A factory of condition methods. Retrieves condition MethodInfos based on their names.
    /// </summary>
    public sealed class ConditionMethodFactory
    {
        private static MethodInfoDictionary _conditionMethods = new MethodInfoDictionary();

        static ConditionMethodFactory()
        {
            foreach (Assembly a in ExtensionUtils.GetExtensionAssemblies())
            {
                AddConditionMethodsFromAssembly(a, "");
            }
        }

        private ConditionMethodFactory(){}

        /// <summary>
        /// Removes all condition method information from the factory.
        /// </summary>
        public static void Clear()
        {
            _conditionMethods.Clear();
        }

        /// <summary>
        /// Scans the specified assembly for types marked with <see cref="ConditionMethodsAttribute" /> and adds
        /// them to the factory. Optionally it prepends the specified text to layout renderer names to avoid
        /// naming collisions.
        /// </summary>
        /// <param name="theAssembly">The assembly to be scanned for layout renderers.</param>
        /// <param name="prefix">The prefix to be prepended to layout renderer names.</param>
        public static void AddConditionMethodsFromAssembly(Assembly theAssembly, string prefix)
        {
            try
            {
                InternalLogger.Debug("AddLogEventConditionsFromAssembly('{0}')", theAssembly.FullName);
                foreach (Type t in theAssembly.GetTypes())
                {
                    if (t.IsDefined(typeof(ConditionMethodsAttribute), false))
                    {
                        if (PlatformDetector.IsSupportedOnCurrentRuntime(t))
                        {
                            foreach (MethodInfo mi in t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                            {
                                ConditionMethodAttribute[] malist = (ConditionMethodAttribute[])mi.GetCustomAttributes(typeof(ConditionMethodAttribute), false);

                                foreach (ConditionMethodAttribute ma in malist)
                                {
                                    if (PlatformDetector.IsSupportedOnCurrentRuntime(mi))
                                    {
                                        AddConditionMethod(ma.Name, mi);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Failed to add LogEventConditions from '" + theAssembly.FullName + "': {0}", ex);
            }

        }

        /// <summary>
        /// Registers the specified condition method under a specified name.
        /// </summary>
        /// <param name="name">Name of the condition method.</param>
        /// <param name="mi"><see cref="MethodInfo" /> of the method to be registered.</param>
        public static void AddConditionMethod(string name, MethodInfo mi)
        {
            InternalLogger.Trace("AddConditionMethod('{0}','{1}')", name, mi);
            _conditionMethods[name.ToLower(CultureInfo.InvariantCulture)] = mi;
        }

        /// <summary>
        /// Returns the <see cref="MethodInfo"/> for the specified condition method.
        /// </summary>
        /// <param name="name">Name of the condition method.</param>
        /// <returns><see cref="MethodInfo"/> object that represents the condition method.</returns>
        public static MethodInfo CreateConditionMethod(string name)
        {
            return _conditionMethods[name.ToLower(CultureInfo.InvariantCulture)];
        }
    }
}
