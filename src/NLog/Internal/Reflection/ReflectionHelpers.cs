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

using JetBrains.Annotations;

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
        public delegate object LateBoundMethod(object target, object[] arguments);

        public delegate object LateBoundMethodSingle(object target, object argument);

        /// <summary>
        /// Optimized delegate for calling a constructor
        /// </summary>
        /// <param name="arguments">Complete list of parameters that matches the constructor, including optional/default parameters. Could be null for no parameters.</param>
        public delegate object LateBoundConstructor([CanBeNull] object[] arguments);

        /// <summary>
        /// Creates an optimized delegate for calling the MethodInfo using Expression-Trees
        /// </summary>
        /// <param name="methodInfo">Method to optimize</param>
        /// <returns>Optimized delegate for invoking the MethodInfo</returns>
        public static LateBoundMethod CreateLateBoundMethod(MethodInfo methodInfo)
        {
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            var parameterExpressions = BuildParameterList(methodInfo, parametersParameter);
            var methodCall = BuildMethodCall(methodInfo, instanceParameter, parameterExpressions);

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

        /// <summary>
        /// Creates an optimized delegate for calling the MethodInfo using Expression-Trees
        /// </summary>
        /// <param name="methodInfo">Method to optimize</param>
        /// <returns>Optimized delegate for invoking the MethodInfo</returns>
        public static LateBoundMethodSingle CreateLateBoundMethodSingle(MethodInfo methodInfo)
        {
            // parameters to execute
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var parametersParameter = Expression.Parameter(typeof(object), "parameters");

            var parameterExpressions = BuildParameterListSingle(methodInfo, parametersParameter);
            var methodCall = BuildMethodCall(methodInfo, instanceParameter, parameterExpressions);

            // ((TInstance)instance).Method((T0)parameters[0], (T1)parameters[1], ...)
            if (methodCall.Type == typeof(void))
            {
                var lambda = Expression.Lambda<Action<object, object>>(
                    methodCall, instanceParameter, parametersParameter);

                Action<object, object> execute = lambda.Compile();
                return (instance, parameters) =>
                {
                    execute(instance, parameters);
                    return null;    // There is no return-type, so we return null-object
                };
            }
            else
            {
                var castMethodCall = Expression.Convert(methodCall, typeof(object));
                var lambda = Expression.Lambda<LateBoundMethodSingle>(
                    castMethodCall, instanceParameter, parametersParameter);

                return lambda.Compile();
            }
        }

        /// <summary>
        /// Creates an optimized delegate for calling the constructors using Expression-Trees
        /// </summary>
        /// <param name="constructor">Constructor to optimize</param>
        /// <returns>Optimized delegate for invoking the constructor</returns>
        public static LateBoundConstructor CreateLateBoundConstructor(ConstructorInfo constructor)
        {
            var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            // build parameter list
            var parameterExpressions = BuildParameterList(constructor, parametersParameter);

            var ctorCall = Expression.New(constructor, parameterExpressions);

            var lambda = Expression.Lambda<LateBoundConstructor>(ctorCall, parametersParameter);

            return lambda.Compile();
        }

        private static IEnumerable<Expression> BuildParameterList(MethodBase methodInfo, ParameterExpression parametersParameter)
        {
            var parameterExpressions = new List<Expression>();
            var paramInfos = methodInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                // (Ti)parameters[i]
                var valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));

                var valueCast = CreateParameterExpression(paramInfos[i], valueObj);
                parameterExpressions.Add(valueCast);
            }

            return parameterExpressions;
        }

        private static IEnumerable<Expression> BuildParameterListSingle(MethodInfo methodInfo, ParameterExpression parameterParameter)
        {
            var parameterExpressions = new List<Expression>();
            var paramInfos = methodInfo.GetParameters().Single();
            {
                // (Ti)parameters[i]
                var parameterExpression = CreateParameterExpression(paramInfos, parameterParameter);
                parameterExpressions.Add(parameterExpression);
            }
            return parameterExpressions;
        }

        private static MethodCallExpression BuildMethodCall(MethodInfo methodInfo, ParameterExpression instanceParameter, IEnumerable<Expression> parameterExpressions)
        {
            // non-instance for static method, or ((TInstance)instance)
            var instanceCast = methodInfo.IsStatic ? null : Expression.Convert(instanceParameter, methodInfo.DeclaringType);

            // static invoke or ((TInstance)instance).Method
            var methodCall = Expression.Call(instanceCast, methodInfo, parameterExpressions);
            return methodCall;
        }

        private static UnaryExpression CreateParameterExpression(ParameterInfo parameterInfo, Expression expression)
        {
            Type parameterType = parameterInfo.ParameterType;
            if (parameterType.IsByRef)
                parameterType = parameterType.GetElementType();

            var valueCast = Expression.Convert(expression, parameterType);
            return valueCast;
        }

        public static bool IsEnum(this Type type)
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return type.IsEnum;
#else
            return type.GetTypeInfo().IsEnum;            
#endif
        }

        public static bool IsPrimitive(this Type type)
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return type.IsPrimitive;
#else
            return type.GetTypeInfo().IsPrimitive;
#endif
        }

        public static bool IsValueType(this Type type)
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return type.IsValueType;
#else
            return type.GetTypeInfo().IsValueType;
#endif
        }

        public static bool IsSealed(this Type type)
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return type.IsSealed;
#else
            return type.GetTypeInfo().IsSealed;
#endif
        }

        public static bool IsAbstract(this Type type)
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return type.IsAbstract;
#else
            return type.GetTypeInfo().IsAbstract;
#endif
        }

        public static bool IsClass(this Type type)
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return type.IsClass;
#else
            return type.GetTypeInfo().IsClass;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return type.IsGenericType;
#else
            return type.GetTypeInfo().IsGenericType;
#endif
        }

        [CanBeNull]
        public static TAttr GetFirstCustomAttribute<TAttr>(this Type type) where TAttr : Attribute
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return Attribute.GetCustomAttributes(type, typeof(TAttr)).FirstOrDefault() as TAttr;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.GetCustomAttributes<TAttr>().FirstOrDefault();
#endif
        }

        [CanBeNull]
        public static TAttr GetFirstCustomAttribute<TAttr>(this PropertyInfo info)
             where TAttr : Attribute
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return Attribute.GetCustomAttributes(info, typeof(TAttr)).FirstOrDefault() as TAttr;
#else
            return info.GetCustomAttributes(typeof(TAttr), false).FirstOrDefault() as TAttr;            
#endif
        }

        [CanBeNull]
        public static TAttr GetFirstCustomAttribute<TAttr>(this Assembly assembly)
            where TAttr : Attribute
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return Attribute.GetCustomAttributes(assembly, typeof(TAttr)).FirstOrDefault() as TAttr;
#else
            return assembly.GetCustomAttributes(typeof(TAttr)).FirstOrDefault() as TAttr;       
#endif
        }

        public static IEnumerable<TAttr> GetCustomAttributes<TAttr>(this Type type, bool inherit) where TAttr : Attribute
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return (TAttr[])type.GetCustomAttributes(typeof(TAttr), inherit);
#else
            return type.GetTypeInfo().GetCustomAttributes<TAttr>(inherit);       
#endif
        }

        public static Assembly GetAssembly(this Type type)
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return type.Assembly;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.Assembly;            
#endif
        }

        public static bool IsValidPublicProperty(this PropertyInfo p)
        {
            return p.CanRead && p.GetIndexParameters().Length == 0 && p.GetGetMethod() != null;
        }

        public static object GetPropertyValue(this PropertyInfo p, object instance)
        {
#if !NET35 && !NET40
            return p.GetValue(instance);
#else
            return p.GetGetMethod().Invoke(instance, null);
#endif
        }
    }
}
