// 
// Copyright (c) 2004-2019 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
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
            try
            {
                return assembly.GetTypes();
            }
#if !SILVERLIGHT || WINDOWS_PHONE
            catch (ReflectionTypeLoadException typeLoadException)
            {
                foreach (var ex in typeLoadException.LoaderExceptions)
                {
                    InternalLogger.Warn(ex, "Type load exception.");
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
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "Type load exception.");
                return ArrayHelper.Empty<Type>();
            }
        }

        /// <summary>
        /// Is this a static class?
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>This is a work around, as Type doesn't have this property. 
        /// From: https://stackoverflow.com/questions/1175888/determine-if-a-type-is-static
        /// </remarks>
        public static bool IsStaticClass(this Type type)
        {
            return type.IsClass() && type.IsAbstract() && type.IsSealed();
        }

        /// <summary>
        /// Optimized delegate for calling MethodInfo
        /// </summary>
        /// <param name="target">Object instance, use null for static methods.</param>
        /// <param name="arguments">Complete list of parameters that matches the method, including optional/default parameters.</param>
        /// <returns></returns>
        public delegate object LateBoundMethod(object target, object[] arguments);

        /// <summary>
        /// Creates an optimized delegate for calling the MethodInfo using Expression-Trees
        /// </summary>
        /// <param name="methodInfo">Method to optimize</param>
        /// <returns>Optimized delegate for invoking the MethodInfo</returns>
        public static LateBoundMethod CreateLateBoundMethod(MethodInfo methodInfo)
        {
            // parameters to execute
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            // build parameter list
            var parameterExpressions = new List<Expression>();
            var paramInfos = methodInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                // (Ti)parameters[i]
                var valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));

                Type parameterType = paramInfos[i].ParameterType;
                if (parameterType.IsByRef)
                    parameterType = parameterType.GetElementType();

                var valueCast = Expression.Convert(valueObj, parameterType);

                parameterExpressions.Add(valueCast);
            }

            // non-instance for static method, or ((TInstance)instance)
            var instanceCast = methodInfo.IsStatic ? null :
                Expression.Convert(instanceParameter, methodInfo.DeclaringType);

            // static invoke or ((TInstance)instance).Method
            var methodCall = Expression.Call(instanceCast, methodInfo, parameterExpressions);

            // ((TInstance)instance).Method((T0)parameters[0], (T1)parameters[1], ...)
            if (methodCall.Type == typeof(void))
            {
                var lambda = Expression.Lambda<Action<object, object[]>>(
                        methodCall, instanceParameter, parametersParameter);

                Action<object, object[]> execute = lambda.Compile();
                return (instance, parameters) =>
                {
                    execute(instance, parameters);
                    return null;    // There is no return-type, so we return null-object
                };
            }
            else
            {
                var castMethodCall = Expression.Convert(methodCall, typeof(object));
                var lambda = Expression.Lambda<LateBoundMethod>(
                    castMethodCall, instanceParameter, parametersParameter);

                return lambda.Compile();
            }
        }

        public static bool IsEnum(this Type type)
        {
#if NETSTANDARD1_0
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        public static bool IsPrimitive(this Type type)
        {
#if NETSTANDARD1_0
            return type.GetTypeInfo().IsPrimitive;
#else
            return type.IsPrimitive;
#endif
        }

        public static bool IsValueType(this Type type)
        {
#if NETSTANDARD1_0
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }

        public static bool IsSealed(this Type type)
        {
#if NETSTANDARD1_0
            return type.GetTypeInfo().IsSealed;
#else
            return type.IsSealed;
#endif
        }

        public static bool IsAbstract(this Type type)
        {
#if NETSTANDARD1_0
            return type.GetTypeInfo().IsAbstract;
#else
            return type.IsAbstract;
#endif
        }

        public static bool IsClass(this Type type)
        {
#if NETSTANDARD1_0
            return type.GetTypeInfo().IsClass;
#else
            return type.IsClass;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if NETSTANDARD1_0
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        public static TAttr GetCustomAttribute<TAttr>(this Type type) where TAttr : Attribute
        {
#if NETSTANDARD1_0
            var typeInfo = type.GetTypeInfo();
            return typeInfo.GetCustomAttribute<TAttr>();
#else
            return (TAttr)Attribute.GetCustomAttribute(type, typeof(TAttr));
#endif
        }

        public static TAttr GetCustomAttribute<TAttr>(this PropertyInfo info)
             where TAttr : Attribute
        {
#if NETSTANDARD1_0
            return info.GetCustomAttributes(typeof(TAttr), false).FirstOrDefault() as TAttr;
#else
            return (TAttr)Attribute.GetCustomAttribute(info, typeof(TAttr));
#endif
        }

        public static TAttr GetCustomAttribute<TAttr>(this Assembly assembly)
            where TAttr : Attribute
        {
#if NETSTANDARD1_0
            return assembly.GetCustomAttributes(typeof(TAttr)).FirstOrDefault() as TAttr;
#else
            return (TAttr)Attribute.GetCustomAttribute(assembly, typeof(TAttr));
#endif
        }

        public static IEnumerable<TAttr> GetCustomAttributes<TAttr>(this Type type, bool inherit) where TAttr : Attribute
        {
#if NETSTANDARD1_0
            return type.GetTypeInfo().GetCustomAttributes<TAttr>(inherit);
#else
            return (TAttr[])type.GetCustomAttributes(typeof(TAttr), inherit);
#endif
        }

        public static Assembly GetAssembly(this Type type)
        {
#if NETSTANDARD1_0
            var typeInfo = type.GetTypeInfo();
            return typeInfo.Assembly;
#else
            return type.Assembly;
#endif
        }
    }
}
