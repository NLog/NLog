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

namespace NLog
{
    /// <summary>
    /// A factory of logging targets. Creates new targets based on their names.
    /// </summary>
    public sealed class TargetFactory
    {
        private static TypeDictionary _targets = new TypeDictionary();

        static TargetFactory()
        {
            foreach (Assembly a in ExtensionUtils.GetExtensionAssemblies())
            {
                AddTargetsFromAssembly(a, "");
            }
        }

        private TargetFactory(){}

        /// <summary>
        /// Removes all target information from the factory.
        /// </summary>
        public static void Clear()
        {
            _targets.Clear();
        }

        /// <summary>
        /// Removes all targets and reloads them from NLog assembly and default extension assemblies.
        /// </summary>
        public static void Reset()
        {
            Clear();
            AddDefaultTargets();
        }

        /// <summary>
        /// Scans the specified assembly for types marked with <see cref="TargetAttribute" /> and adds
        /// them to the factory. Optionally it prepends the specified text to the target names to avoid
        /// naming collisions.
        /// </summary>
        /// <param name="theAssembly">The assembly to be scanned for targets.</param>
        /// <param name="prefix">The prefix to be prepended to target names.</param>
        public static void AddTargetsFromAssembly(Assembly theAssembly, string prefix)
        {
            try
            {
                InternalLogger.Debug("AddTargetsFromAssembly('{0}')", theAssembly.FullName);
                foreach (Type t in theAssembly.GetTypes())
                {
                    TargetAttribute[]attributes = (TargetAttribute[])t.GetCustomAttributes(typeof(TargetAttribute), false);
                    if (attributes != null)
                    {
                        foreach (TargetAttribute attr in attributes)
                        {
                            if (PlatformDetector.IsSupportedOnCurrentRuntime(t))
                            {
                                AddTarget(prefix + attr.Name, t);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Failed to add targets from '" + theAssembly.FullName + "': {0}", ex);
            }
        }

        /// <summary>
        /// Adds default targets from the NLog.dll assembly.
        /// </summary>
        private static void AddDefaultTargets()
        {
            AddTargetsFromAssembly(typeof(TargetFactory).Assembly, String.Empty);
        }

        /// <summary>
        /// Registers the specified target type to the factory under a specified name.
        /// </summary>
        /// <param name="targetName">The name of the target (e.g. <code>File</code> or <code>Console</code>)</param>
        /// <param name="targetType">The type of the new target</param>
        /// <remarks>
        /// The name specified in the targetName parameter can then be used
        /// to create target.
        /// </remarks>
        public static void AddTarget(string targetName, Type targetType)
        {
            string hashKey = targetName.ToLower(CultureInfo.InvariantCulture);

            InternalLogger.Trace("Registering target {0} for type '{1}')", targetName, targetType);
            _targets[hashKey] = targetType;
        }

        /// <summary>
        /// Creates the target object based on its target name.
        /// </summary>
        /// <param name="name">The name of the target (e.g. <code>File</code> or <code>Console</code>)</param>
        /// <returns>A new instance of the Target object.</returns>
        public static Target CreateTarget(string name)
        {
            Type t = _targets[name.ToLower(CultureInfo.InvariantCulture)];
            if (t != null)
            {
                Target la = FactoryHelper.CreateInstance(t) as Target;
                if (la != null)
                    return la;
            }
            throw new ArgumentException("Target " + name + " not found.");
        }

        /// <summary>
        /// Collection of target types added to the factory.
        /// </summary>
        public static ICollection TargetTypes
        {
            get { return _targets.Values; }
        }
    }
}
