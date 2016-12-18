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

using System.Collections;
using System.Linq;
#if SILVERLIGHT
using System.Windows;
#endif
using NLog.Config;

namespace NLog.Internal
{
    using System;
    using System.Collections.Generic;
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
#if SILVERLIGHT && !WINDOWS_PHONE
            return assembly.GetTypes();
#else
            try
            {
#if NETSTANDARD_1plus
                return assembly.DefinedTypes.Select(typeinfo => typeinfo.AsType()).ToArray();
#else
                return assembly.GetTypes();
#endif
            }
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
        }


        public static TAttr GetCustomAttribute<TAttr>(this Type type)
            where TAttr : Attribute
        {
#if !NETSTANDARD
            return (TAttr)Attribute.GetCustomAttribute(type, typeof(TAttr));
#else

            var typeInfo = type.GetTypeInfo();
            return typeInfo.GetCustomAttribute<TAttr>();
#endif
        }

        /// <summary>
        /// Is this a static class?
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>This is a work around, as Type doesn't have this property. 
        /// From: http://stackoverflow.com/questions/1175888/determine-if-a-type-is-static
        /// </remarks>
        public static bool IsStaticClass(this Type type)
        {
            return type.IsClass() && type.IsAbstract() && type.IsSealed();
        }

        public static TAttr GetCustomAttribute<TAttr>(PropertyInfo info)
             where TAttr : Attribute
        {
            return info.GetCustomAttributes(typeof(TAttr), false).FirstOrDefault() as TAttr;
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
                Expression.Convert(instanceParameter, methodInfo.GetReflectedType());

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

        public static IEnumerable<TAttr> GetCustomAttributes<TAttr>(Type type, bool inherit)
        where TAttr : Attribute
        {
#if !NETSTANDARD
            return (TAttr[])Attribute.GetCustomAttributes(type, typeof(TAttr));
#else

            var typeInfo = type.GetTypeInfo();
            return typeInfo.GetCustomAttributes<TAttr>(inherit);
#endif
        }

        public static bool IsDefined<TAttr>(this Type type, bool inherit)
        {
#if !NETSTANDARD
            return type.IsDefined(typeof(TAttr), inherit);
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsDefined(typeof(TAttr), inherit);
#endif
        }

        public static bool IsEnum(this Type type)
        {
#if !NETSTANDARD
            return type.IsEnum;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsEnum;
#endif
        }

        public static bool IsNestedPrivate(this Type type)
        {
#if !NETSTANDARD
            return type.IsNestedPrivate;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsNestedPrivate;
#endif
        }
        public static bool IsGenericTypeDefinition(this Type type)
        {
#if !NETSTANDARD
            return type.IsGenericTypeDefinition;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericTypeDefinition;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if !NETSTANDARD
            return type.IsGenericType;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType;
#endif
        }
        public static Type GetBaseType(this Type type)
        {
#if !NETSTANDARD
            return type.BaseType;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.BaseType;
#endif
        }

        public static bool IsPublic(this Type type)
        {
#if !NETSTANDARD
            return type.IsPublic;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPublic;
#endif
        }


        public static bool IsInterface(this Type type)
        {
#if !NETSTANDARD
            return type.IsInterface;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsInterface;
#endif
        }

        public static bool IsAbstract(this Type type)
        {
#if !NETSTANDARD
            return type.IsAbstract;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsAbstract;
#endif
        }

        public static bool IsPrimitive(this Type type)
        {
#if !NETSTANDARD
            return type.IsPrimitive;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPrimitive;
#endif
        }
        public static bool IsClass(this Type type)
        {
#if !NETSTANDARD
            return type.IsClass;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsClass;
#endif
        }
        public static bool IsSealed(this Type type)
        {
#if !NETSTANDARD
            return type.IsSealed;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsSealed;
#endif
        }



        public static Assembly GetAssembly(this Type type)
        {
#if !NETSTANDARD
            return type.Assembly;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.Assembly;
#endif
        }


        public static Module GetModule(this Type type)
        {
#if !NETSTANDARD
            return type.Module;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.Module;
#endif
        }

        public static Type GetReflectedType(this MethodInfo methodInfo)
        {
#if !NETSTANDARD
            return methodInfo.ReflectedType;
#else
#if RELEASE

#error TODO methodInfo.ReflectedType?
#endif
            return methodInfo.ReturnType;

#endif
        }


        public static object InvokeMethod(this MethodInfo methodInfo, string methodName, object[] callParameters)
        {
#if !NETSTANDARD
            return methodInfo.DeclaringType.InvokeMember(
                 methodName,
                 BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public | BindingFlags.OptionalParamBinding,
                 null,
                 null,
                 callParameters);
#elif !SILVERLIGHT && !NETSTANDARD
                , CultureInfo.InvariantCulture
#else


            var neededParameters = methodInfo.GetParameters();

            var missingParametersCount = neededParameters.Length - callParameters.Length;
            if (missingParametersCount > 0)
            {
                //optional parmeters needs to passed here with Type.Missing;
                var paramList = callParameters.ToList();
                paramList.AddRange(Enumerable.Repeat(Type.Missing, missingParametersCount));
                callParameters = paramList.ToArray();
            }
            //TODO test
            return methodInfo.Invoke(methodName, callParameters);
#endif
        }

        public static Assembly GetAssembly(this Module module)
        {
#if !NETSTANDARD
            return module.Assembly;
#else
            //TODO check this
            var typeInfo = module.GetType().GetTypeInfo();
            return typeInfo.Assembly;
#endif
        }
#if !NETSTANDARD && !WINDOWS_PHONE

        public static string GetCodeBase(this Assembly assembly)
        {

            return assembly.CodeBase;
        }

#endif

#if !NETSTANDARD && !WINDOWS_PHONE
        public static string GetLocation(this Assembly assembly)
        {
            return assembly.Location;

        }
#endif

#if NETSTANDARD
        public static bool IsSubclassOf(this Type type, Type subtype)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsSubclassOf(subtype);

        }
#endif
    }
}
