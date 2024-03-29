// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Config
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using NLog.Common;
    using NLog.Conditions;
    using NLog.Internal;

    /// <summary>
    /// Factory for locating methods.
    /// </summary>
    internal sealed class MethodFactory :
#pragma warning disable CS0618 // Type or member is obsolete
        INamedItemFactory<MethodInfo, MethodInfo>,
#pragma warning restore CS0618 // Type or member is obsolete
        IFactory
    {
        private readonly MethodFactory _globalDefaultFactory;
        private readonly Dictionary<string, MethodDetails> _nameToMethodDetails = new Dictionary<string, MethodDetails>(StringComparer.OrdinalIgnoreCase);

        struct MethodDetails
        {
            public readonly MethodInfo MethodInfo;
            public readonly Func<LogEventInfo, object> NoParameters;
            public readonly Func<LogEventInfo, object, object> OneParameter;
            public readonly Func<LogEventInfo, object, object, object> TwoParameters;
            public readonly Func<LogEventInfo, object, object, object, object> ThreeParameters;
            public readonly Func<object[], object> ManyParameters;
            public readonly int ManyParameterMinCount;
            public readonly int ManyParameterMaxCount;
            public readonly bool ManyParameterWithLogEvent;

            public MethodDetails(
                MethodInfo methodInfo,
                Func<LogEventInfo, object> noParameters,
                Func<LogEventInfo, object, object> oneParameter,
                Func<LogEventInfo, object, object, object> twoParameters,
                Func<LogEventInfo, object, object, object, object> threeParameters,
                Func<object[], object> manyParameters,
                int manyParameterMinCount,
                int manyParameterMaxCount,
                bool manyParameterWithLogEvent)
            {
                MethodInfo = methodInfo;
                NoParameters = noParameters;
                OneParameter = oneParameter;
                TwoParameters = twoParameters;
                ThreeParameters = threeParameters;
                ManyParameters = manyParameters;
                ManyParameterMinCount = manyParameterMinCount;
                ManyParameterMaxCount = manyParameterMaxCount;
                ManyParameterWithLogEvent = manyParameterWithLogEvent;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodFactory"/> class.
        /// </summary>
        public MethodFactory(MethodFactory globalDefaultFactory)
        {
            _globalDefaultFactory = globalDefaultFactory;
        }

        /// <summary>
        /// Scans the assembly for classes marked with expected class <see cref="Attribute"/>
        /// and methods marked with expected <see cref="NameBaseAttribute"/> and adds them 
        /// to the factory.
        /// </summary>
        /// <param name="types">The types to scan.</param>
        /// <param name="assemblyName">The assembly name for the type.</param>
        /// <param name="itemNamePrefix">The item name prefix.</param>
        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        [UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2072")]
        [UnconditionalSuppressMessage("Trimming - Ignore since obsolete", "IL2062")]
        public void ScanTypes(Type[] types, string assemblyName, string itemNamePrefix)
        {
            foreach (Type t in types)
            {
                try
                {
                    if (t.IsClass())
                    {
                        RegisterType(t, itemNamePrefix);
                    }
                }
                catch (Exception exception)
                {
                    InternalLogger.Error(exception, "Failed to add type '{0}'.", t.FullName);

                    if (exception.MustBeRethrown())
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Registers the type.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="itemNamePrefix">The item name prefix.</param>
        void IFactory.RegisterType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)] Type type, string itemNamePrefix)
        {
            RegisterType(type, itemNamePrefix);
        }

        /// <summary>
        /// Registers the type.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="itemNamePrefix">The item name prefix.</param>
        private void RegisterType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type, string itemNamePrefix)
        {
            var extractedMethods = ExtractClassMethods<ConditionMethodsAttribute, ConditionMethodAttribute>(type);
            if (extractedMethods?.Count > 0)
            {
                for (int i = 0; i < extractedMethods.Count; ++i)
                {
                    string methodName = string.IsNullOrEmpty(itemNamePrefix) ? extractedMethods[i].Key : itemNamePrefix + extractedMethods[i].Key;
                    RegisterDefinition(methodName, extractedMethods[i].Value);
                }
            }
        }

        /// <summary>
        /// Scans a type for relevant methods with their symbolic names
        /// </summary>
        /// <typeparam name="TClassAttributeType">Include types that are marked with this attribute</typeparam>
        /// <typeparam name="TMethodAttributeType">Include methods that are marked with this attribute</typeparam>
        /// <param name="type">Class Type to scan</param>
        /// <returns>Collection of methods with their symbolic names</returns>
        private static IList<KeyValuePair<string, MethodInfo>> ExtractClassMethods<TClassAttributeType, TMethodAttributeType>([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type) 
            where TClassAttributeType : Attribute
            where TMethodAttributeType : NameBaseAttribute
        {
            if (!type.IsDefined(typeof(TClassAttributeType), false))
                return ArrayHelper.Empty<KeyValuePair<string, MethodInfo>>();

            var conditionMethods = new List<KeyValuePair<string, MethodInfo>>();
            foreach (MethodInfo mi in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                var methodAttributes = (TMethodAttributeType[])mi.GetCustomAttributes(typeof(TMethodAttributeType), false);
                foreach (var attr in methodAttributes)
                {
                    conditionMethods.Add(new KeyValuePair<string, MethodInfo>(attr.Name, mi));
                }
            }

            return conditionMethods;
        }

        /// <summary>
        /// Clears contents of the factory.
        /// </summary>
        public void Clear()
        {
            lock (_nameToMethodDetails)
            {
                _nameToMethodDetails.Clear();
            }
        }

        /// <summary>
        /// Registers the definition of a single method.
        /// </summary>
        /// <param name="itemName">The method name.</param>
        /// <param name="itemDefinition">The method info.</param>
        [Obsolete("Instead use RegisterType<T>, as dynamic Assembly loading will be moved out. Marked obsolete with NLog v5.2")]
        void INamedItemFactory<MethodInfo, MethodInfo>.RegisterDefinition(string itemName, MethodInfo itemDefinition)
        {
            RegisterDefinition(itemName, itemDefinition);
        }

        internal void RegisterDefinition(string methodName, MethodInfo methodInfo)
        {
            object[] defaultMethodParameters = ResolveDefaultMethodParameters(methodInfo, out var manyParameterMinCount, out var manyParameterMaxCount, out var includeLogEvent);

            if (manyParameterMaxCount > 0)
                RegisterManyParameters(methodName, (inputArgs) => InvokeMethodInfo(methodInfo, ResolveMethodParameters(defaultMethodParameters, inputArgs)), manyParameterMinCount, manyParameterMaxCount, includeLogEvent, methodInfo);

            if (manyParameterMinCount == 0)
            {
                if (!includeLogEvent)
                    RegisterNoParameters(methodName, (logEvent) => InvokeMethodInfo(methodInfo, defaultMethodParameters), methodInfo);
                else
                    RegisterNoParameters(methodName, (logEvent) => InvokeMethodInfo(methodInfo, ResolveMethodParameters(defaultMethodParameters, logEvent)), methodInfo);
            }
            if (manyParameterMinCount <= 1 && manyParameterMaxCount >= 1)
            {
                if (!includeLogEvent)
                    RegisterOneParameter(methodName, (logEvent, arg1) => InvokeMethodInfo(methodInfo, ResolveMethodParameters(defaultMethodParameters, arg1)), methodInfo);
                else
                    RegisterOneParameter(methodName, (logEvent, arg1) => InvokeMethodInfo(methodInfo, ResolveMethodParameters(defaultMethodParameters, logEvent, arg1)), methodInfo);
            }
            if (manyParameterMinCount <= 2 && manyParameterMaxCount >= 2)
            {
                if (!includeLogEvent)
                    RegisterTwoParameters(methodName, (logEvent, arg1, arg2) => InvokeMethodInfo(methodInfo, ResolveMethodParameters(defaultMethodParameters, arg1, arg2)), methodInfo);
                else
                    RegisterTwoParameters(methodName, (logEvent, arg1, arg2) => InvokeMethodInfo(methodInfo, ResolveMethodParameters(defaultMethodParameters, logEvent, arg1, arg2)), methodInfo);
            }
            if (manyParameterMinCount <= 3 && manyParameterMaxCount >= 3)
            {
                if (!includeLogEvent)
                    RegisterThreeParameters(methodName, (logEvent, arg1, arg2, arg3) => InvokeMethodInfo(methodInfo, ResolveMethodParameters(defaultMethodParameters, arg1, arg2, arg3)), methodInfo);
                else
                    RegisterThreeParameters(methodName, (logEvent, arg1, arg2, arg3) => InvokeMethodInfo(methodInfo, ResolveMethodParameters(defaultMethodParameters, logEvent, arg1, arg2, arg3)), methodInfo);
            }
        }

        private static object InvokeMethodInfo(MethodInfo methodInfo, object[] methodArgs)
        {
            try
            {
                return methodInfo.Invoke(null, methodArgs);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is null)
                    throw;

                throw ex.InnerException;
            }
        }

        private static object[] ResolveDefaultMethodParameters(MethodInfo methodInfo, out int manyParameterMinCount, out int manyParameterMaxCount, out bool includeLogEvent)
        {
            var methodParameters = methodInfo.GetParameters();

            manyParameterMinCount = 0;
            manyParameterMaxCount = methodParameters.Length;
            var defaultMethodParameters = new object[methodParameters.Length];
            for (int i = 0; i < defaultMethodParameters.Length; ++i)
            {
                if (methodParameters[i].IsOptional)
                    defaultMethodParameters[i] = methodParameters[i].DefaultValue;
                else
                    ++manyParameterMinCount;
            }
            includeLogEvent = methodParameters.Length > 0 && methodParameters[0].ParameterType == typeof(LogEventInfo);
            if (includeLogEvent)
            {
                --manyParameterMaxCount;
                if (manyParameterMinCount > 0)
                    --manyParameterMinCount;
            }
            return defaultMethodParameters;
        }

        private static object[] ResolveMethodParameters(object[] defaultMethodParameters, object[] inputParameters)
        {
            if (defaultMethodParameters.Length == inputParameters.Length)
                return inputParameters;

            object[] methodParameters = new object[defaultMethodParameters.Length];
            for (int i = 0; i < inputParameters.Length; ++i)
                methodParameters[i] = inputParameters[i];
            for (int i = inputParameters.Length; i < defaultMethodParameters.Length; ++i)
                methodParameters[i] = defaultMethodParameters[i];
            return methodParameters;
        }

        private static object[] ResolveMethodParameters(object[] defaultMethodParameters, object inputParameterArg1)
        {
            object[] methodParameters = new object[defaultMethodParameters.Length];
            methodParameters[0] = inputParameterArg1;
            for (int i = 1; i < defaultMethodParameters.Length; ++i)
                methodParameters[i] = defaultMethodParameters[i];
            return methodParameters;
        }

        private static object[] ResolveMethodParameters(object[] defaultMethodParameters, object inputParameterArg1, object inputParameterArg2)
        {
            object[] methodParameters = new object[defaultMethodParameters.Length];
            methodParameters[0] = inputParameterArg1;
            methodParameters[1] = inputParameterArg2;
            for (int i = 2; i < defaultMethodParameters.Length; ++i)
                methodParameters[i] = defaultMethodParameters[i];
            return methodParameters;
        }

        private static object[] ResolveMethodParameters(object[] defaultMethodParameters, object inputParameterArg1, object inputParameterArg2, object inputParameterArg3)
        {
            object[] methodParameters = new object[defaultMethodParameters.Length];
            methodParameters[0] = inputParameterArg1;
            methodParameters[1] = inputParameterArg2;
            methodParameters[2] = inputParameterArg3;
            for (int i = 3; i < defaultMethodParameters.Length; ++i)
                methodParameters[i] = defaultMethodParameters[i];
            return methodParameters;
        }

        private static object[] ResolveMethodParameters(object[] defaultMethodParameters, object inputParameterArg1, object inputParameterArg2, object inputParameterArg3, object inputParameterArg4)
        {
            object[] methodParameters = new object[defaultMethodParameters.Length];
            methodParameters[0] = inputParameterArg1;
            methodParameters[1] = inputParameterArg2;
            methodParameters[2] = inputParameterArg3;
            methodParameters[3] = inputParameterArg4;
            for (int i = 4; i < defaultMethodParameters.Length; ++i)
                methodParameters[i] = defaultMethodParameters[i];
            return methodParameters;
        }

        /// <summary>
        /// Tries to retrieve method by name.
        /// </summary>
        /// <param name="itemName">The method name.</param>
        /// <param name="result">The result.</param>
        /// <returns>A value of <c>true</c> if the method was found, <c>false</c> otherwise.</returns>
        [Obsolete("Use TryCreateInstance instead. Marked obsolete with NLog v5.2")]
        bool INamedItemFactory<MethodInfo, MethodInfo>.TryCreateInstance(string itemName, out MethodInfo result)
        {
            return TryGetDefinition(itemName, out result);
        }

        /// <summary>
        /// Retrieves method by name.
        /// </summary>
        /// <param name="itemName">Method name.</param>
        /// <returns>MethodInfo object.</returns>
        [Obsolete("Use TryCreateInstance instead. Marked obsolete with NLog v5.2")]
        MethodInfo INamedItemFactory<MethodInfo, MethodInfo>.CreateInstance(string itemName)
        {
            if (TryGetDefinition(itemName, out MethodInfo result))
            {
                return result;
            }

            throw new NLogConfigurationException($"Unknown function: '{itemName}'");
        }

        /// <summary>
        /// Tries to get method definition.
        /// </summary>
        /// <param name="itemName">The method name.</param>
        /// <param name="result">The result.</param>
        /// <returns>A value of <c>true</c> if the method was found, <c>false</c> otherwise.</returns>
        [Obsolete("Use TryCreateInstance instead. Marked obsolete with NLog v5.2")]
        public bool TryGetDefinition(string itemName, out MethodInfo result)
        {
            lock (_nameToMethodDetails)
            {
                if (_nameToMethodDetails.TryGetValue(itemName, out var methodDetails))
                {
                    result = methodDetails.MethodInfo;
                    return result != null;
                }
            }

            result = null;
            return _globalDefaultFactory?.TryGetDefinition(itemName, out result) ?? false;
        }

        public void RegisterNoParameters(string methodName, Func<LogEventInfo, object> noParameters, MethodInfo legacyMethodInfo = null)
        {
            lock (_nameToMethodDetails)
            {
                _nameToMethodDetails.TryGetValue(methodName, out var methodDetails);
                legacyMethodInfo = legacyMethodInfo ?? methodDetails.MethodInfo ?? noParameters.GetDelegateInfo();
                _nameToMethodDetails[methodName] = new MethodDetails(legacyMethodInfo, noParameters, methodDetails.OneParameter, methodDetails.TwoParameters, methodDetails.ThreeParameters, methodDetails.ManyParameters, methodDetails.ManyParameterMinCount, methodDetails.ManyParameterMaxCount, methodDetails.ManyParameterWithLogEvent);
            }
        }

        public void RegisterOneParameter(string methodName, Func<LogEventInfo, object, object> oneParameter, MethodInfo legacyMethodInfo = null)
        {
            lock (_nameToMethodDetails)
            {
                _nameToMethodDetails.TryGetValue(methodName, out var methodDetails);
                legacyMethodInfo = legacyMethodInfo ?? methodDetails.MethodInfo ?? oneParameter.GetDelegateInfo();
                _nameToMethodDetails[methodName] = new MethodDetails(legacyMethodInfo, methodDetails.NoParameters, oneParameter, methodDetails.TwoParameters, methodDetails.ThreeParameters, methodDetails.ManyParameters, methodDetails.ManyParameterMinCount, methodDetails.ManyParameterMaxCount, methodDetails.ManyParameterWithLogEvent);
            }
        }

        public void RegisterTwoParameters(string methodName, Func<LogEventInfo, object, object, object> twoParameters, MethodInfo legacyMethodInfo = null)
        {
            lock (_nameToMethodDetails)
            {
                _nameToMethodDetails.TryGetValue(methodName, out var methodDetails);
                legacyMethodInfo = legacyMethodInfo ?? methodDetails.MethodInfo ?? twoParameters.GetDelegateInfo();
                _nameToMethodDetails[methodName] = new MethodDetails(legacyMethodInfo, methodDetails.NoParameters, methodDetails.OneParameter, twoParameters, methodDetails.ThreeParameters, methodDetails.ManyParameters, methodDetails.ManyParameterMinCount, methodDetails.ManyParameterMaxCount, methodDetails.ManyParameterWithLogEvent);
            }
        }

        public void RegisterThreeParameters(string methodName, Func<LogEventInfo, object, object, object, object> threeParameters, MethodInfo legacyMethodInfo = null)
        {
            lock (_nameToMethodDetails)
            {
                _nameToMethodDetails.TryGetValue(methodName, out var methodDetails);
                legacyMethodInfo = legacyMethodInfo ?? methodDetails.MethodInfo ?? threeParameters.GetDelegateInfo();
                _nameToMethodDetails[methodName] = new MethodDetails(legacyMethodInfo, methodDetails.NoParameters, methodDetails.OneParameter, methodDetails.TwoParameters, threeParameters, methodDetails.ManyParameters, methodDetails.ManyParameterMinCount, methodDetails.ManyParameterMaxCount, methodDetails.ManyParameterWithLogEvent);
            }
        }

        public void RegisterManyParameters(string methodName, Func<object[], object> manyParameters, int manyParameterMinCount, int manyParameterMaxCount, bool manyParameterWithLogEvent, MethodInfo legacyMethodInfo = null)
        {
            lock (_nameToMethodDetails)
            {
                _nameToMethodDetails.TryGetValue(methodName, out var methodDetails);
                legacyMethodInfo = legacyMethodInfo ?? methodDetails.MethodInfo ?? manyParameters.GetDelegateInfo();
                _nameToMethodDetails[methodName] = new MethodDetails(legacyMethodInfo, methodDetails.NoParameters, methodDetails.OneParameter, methodDetails.TwoParameters, methodDetails.ThreeParameters, manyParameters, manyParameterMinCount, manyParameterMaxCount, manyParameterWithLogEvent);
            }
        }

        public Func<LogEventInfo, object> TryCreateInstanceWithNoParameters(string methodName)
        {
            lock (_nameToMethodDetails)
            {
                if (_nameToMethodDetails.TryGetValue(methodName, out var methodDetails))
                    return methodDetails.NoParameters;
                else
                    return null;
            }
        }

        public Func<LogEventInfo, object, object> TryCreateInstanceWithOneParameter(string methodName)
        {
            lock (_nameToMethodDetails)
            {
                if (_nameToMethodDetails.TryGetValue(methodName, out var methodDetails))
                    return methodDetails.OneParameter;
                else
                    return null;
            }
        }

        public Func<LogEventInfo, object, object, object> TryCreateInstanceWithTwoParameters(string methodName)
        {
            lock (_nameToMethodDetails)
            {
                if (_nameToMethodDetails.TryGetValue(methodName, out var methodDetails))
                    return methodDetails.TwoParameters;
                else
                    return null;
            }
        }

        public Func<LogEventInfo, object, object, object, object> TryCreateInstanceWithThreeParameters(string methodName)
        {
            lock (_nameToMethodDetails)
            {
                if (_nameToMethodDetails.TryGetValue(methodName, out var methodDetails))
                    return methodDetails.ThreeParameters;
                else
                    return null;
            }
        }

        public Func<object[], object> TryCreateInstanceWithManyParameters(string methodName, out int manyParameterMinCount, out int manyParameterMaxCount, out bool manyParameterWithLogEvent)
        {
            lock (_nameToMethodDetails)
            {
                if (_nameToMethodDetails.TryGetValue(methodName, out var methodDetails))
                {
                    if (methodDetails.ManyParameters != null)
                    {
                        manyParameterMaxCount = methodDetails.ManyParameterMaxCount;
                        manyParameterMinCount = methodDetails.ManyParameterMinCount;
                        manyParameterWithLogEvent = methodDetails.ManyParameterWithLogEvent;
                        return methodDetails.ManyParameters;
                    }
                    else if (methodDetails.ThreeParameters != null)
                    {
                        manyParameterMaxCount = 3;
                        manyParameterMinCount = methodDetails.TwoParameters is null ? 3 : 2;
                        manyParameterWithLogEvent = true;
                        return new Func<object[], object>(args => methodDetails.ThreeParameters((LogEventInfo)args[0], args[1], args[2], args[3]));
                    }
                    else if (methodDetails.TwoParameters != null)
                    {
                        manyParameterMaxCount = 2;
                        manyParameterMinCount = methodDetails.OneParameter is null ? 2 : 1;
                        manyParameterWithLogEvent = true;
                        return new Func<object[], object>(args => methodDetails.TwoParameters((LogEventInfo)args[0], args[1], args[2]));
                    }
                    else if (methodDetails.OneParameter != null)
                    {
                        manyParameterMaxCount = 1;
                        manyParameterMinCount = methodDetails.NoParameters is null ? 1 : 0;
                        manyParameterWithLogEvent = true;
                        return new Func<object[], object>(args => methodDetails.OneParameter((LogEventInfo)args[0], args[1]));
                    }
                    else if (methodDetails.NoParameters != null)
                    {
                        manyParameterMaxCount = 0;
                        manyParameterMinCount = 0;
                        manyParameterWithLogEvent = true;
                        return new Func<object[], object>(args => methodDetails.NoParameters((LogEventInfo)args[0]));
                    }
                }
                manyParameterMinCount = 0;
                manyParameterMaxCount = 0;
                manyParameterWithLogEvent = false;
                return null;
            }
        }
    }
}
