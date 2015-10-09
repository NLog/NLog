// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System.Collections;
using System.Diagnostics;
using NLog.Config;

namespace NLog.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using NLog.Common;

    /// <summary>
    /// Reflection helpers.
    /// </summary>
    internal static class ReflectionHelpers
    {
        /// <summary>
        /// Gets all usable exported types from the given assembly.
        /// </summary>
        /// <param name="assembly">Assembly to scan.</param>
        /// <returns>Usable types from the given assembly.</returns>
        /// <remarks>Types which cannot be loaded are skipped.</remarks>
        public static Type[] SafeGetTypes(this Assembly assembly)
        {
#if SILVERLIGHT
            return assembly.GetTypes();
#else
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException typeLoadException)
            {
                foreach (var ex in typeLoadException.LoaderExceptions)
                {
                    InternalLogger.Warn("Type load exception: {0}", ex);
                }

                var loadedTypes = new List<Type>();
                foreach (var t in typeLoadException.Types)
                {
                    if (t != null)
                    {
                        loadedTypes.Add(t);
                    }
                }

                return loadedTypes.ToArray();
            }
#endif
        }


        public static TAttr GetCustomAttribute<TAttr>(this Type type)
            where TAttr : Attribute
        {
#if !UAP10
            return  (TAttr)Attribute.GetCustomAttribute(type, typeof(TAttr));
#else

            var typeInfo = type.GetTypeInfo();
            return typeInfo.GetCustomAttribute<TAttr>();
#endif
        }

        public static TAttr GetCustomAttribute<TAttr>(PropertyInfo info)
    where TAttr : Attribute
        {
            return info.GetCustomAttribute<TAttr>();
        }

        public static IEnumerable<TAttr> GetCustomAttributes<TAttr>(Type type, bool inherit)
                where TAttr : Attribute
        {
#if !UAP10
            return  (TAttr[])Attribute.GetCustomAttributes(type, typeof(TAttr));
#else

            var typeInfo = type.GetTypeInfo();
            return typeInfo.GetCustomAttributes<TAttr>(inherit);
#endif
        }

        public static bool IsDefined<TAttr>(this Type type, bool inherit)
        {
#if !UAP10
            return type.IsDefined(typeof (TAttr), inherit);
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsDefined(typeof (TAttr), inherit);
#endif
        }

        public static bool IsEnum(this Type type)
        {
#if !UAP10
            return type.IsEnum;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsEnum;
#endif
        }

        public static bool IsPrimitive(this Type type)
        {
#if !UAP10
            return type.IsPrimitive;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPrimitive;
#endif
        }

        public static Assembly Assembly(this Type type)
        {
#if !UAP10
            return type.Assembly;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.Assembly;
#endif
        }


        public static Module Module(this Type type)
        {
#if !UAP10
            return type.Module;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.Module;
#endif
        }
        public static object InvokeMethod(this MethodInfo methodInfo, string methodName, object[] callParameters)
        {
#if !UAP10
           methodInfo.DeclaringType.InvokeMember( 
                MethodInfo.Name, 
                BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public | BindingFlags.OptionalParamBinding, 
                null, 
                null, 
                callParameters
#elif !SILVERLIGHT && !UAP10
                , CultureInfo.InvariantCulture
#else
            //TODO test
           return methodInfo.Invoke(methodName, callParameters);
#endif
        }

        public static Assembly Assembly(this Module module)
        {
#if !UAP10
            return module.Assembly;
#else
            //TODO check this
            var typeInfo = module.GetType().GetTypeInfo();
            return typeInfo.Assembly;
#endif
        }


        public static string CodeBase(this Assembly assembly)
        {
#if !UAP10
            return assembly.Codebase;
#else
            throw new NotImplementedException();

#endif
        }


        public static string Location(this Assembly assembly)
        {
#if !UAP10
            return assembly.Location;
#else
            throw new NotImplementedException();

#endif
        }

#if UAP10
        public static bool IsSubclassOf(this Type type, Type subtype)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsSubclassOf(subtype);

        }
#endif
    }

    public static class StackFrameExt
    {

#if UAP10
        /// <summary>
        /// Null
        /// </summary>
        /// <returns></returns>
        public static StackFrame GetFrame(this StackTrace strackTrace, int number)
        {

            //TODO
            return null;
        }
#endif
        /// <summary>
        /// 0
        /// </summary>
        /// <returns></returns>
        public static int GetFrameCount(this StackTrace strackTrace)
        {

#if !UAP10
            return strackTrace.FrameCount;
#else
            return 0;

#endif
            //TODO

        }
    }
}
