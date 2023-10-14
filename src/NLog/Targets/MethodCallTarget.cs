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

namespace NLog.Targets
{
    using System;
    using System.Reflection;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Calls the specified static method on each log message and passes contextual parameters to it.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/nlog/nlog/wiki/MethodCall-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/MethodCall-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="https://github.com/NLog/NLog/wiki/Configuration-file">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/MethodCall/NLog.config" />
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/MethodCall/Simple/Example.cs" />
    /// </example>
    [Target("MethodCall")]
    public sealed class MethodCallTarget : MethodCallTargetBase
    {
        /// <summary>
        /// Gets or sets the class name.
        /// </summary>
        /// <docgen category='Invocation Options' order='10' />
        public string ClassName { get; set; }

        /// <summary>
        /// Gets or sets the method name. The method must be public and static.
        /// 
        /// Use the AssemblyQualifiedName , https://msdn.microsoft.com/en-us/library/system.type.assemblyqualifiedname(v=vs.110).aspx
        /// e.g. 
        /// </summary>
        /// <docgen category='Invocation Options' order='10' />
        public string MethodName { get; set; }

        Action<LogEventInfo, object[]> _logEventAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodCallTarget" /> class.
        /// </summary>
        public MethodCallTarget() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodCallTarget" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        public MethodCallTarget(string name) : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodCallTarget" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <param name="logEventAction">Method to call on logevent.</param>
        public MethodCallTarget(string name, Action<LogEventInfo, object[]> logEventAction) : this()
        {
            Name = name;
            _logEventAction = logEventAction;
        }

        /// <inheritdoc/>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (!string.IsNullOrEmpty(ClassName) && !string.IsNullOrEmpty(MethodName))
            {
                _logEventAction = BuildLogEventAction(ClassName, MethodName);
            }
            else if (_logEventAction is null)
            {
                throw new NLogConfigurationException($"MethodCallTarget: Missing configuration of ClassName and MethodName");
            }
        }

        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming - Allow method lookup from config", "IL2075")]
        private static Action<LogEventInfo, object[]> BuildLogEventAction(string className, string methodName)
        {
            var targetType = PropertyTypeConverter.ConvertToType(className.Trim(), false);
            if (targetType is null)
            {
                throw new NLogConfigurationException($"MethodCallTarget: failed to get type from ClassName={className}");
            }
            else
            {
                var methodInfo = targetType.GetMethod(methodName);
                if (methodInfo is null)
                {
                    throw new NLogConfigurationException($"MethodCallTarget: MethodName={methodName} not found in ClassName={className} - and must be static method");
                }
                else if (!methodInfo.IsStatic)
                {
                    throw new NLogConfigurationException($"MethodCallTarget: MethodName={methodName} found in ClassName={className} - but not static method");
                }
                else
                {
                    return BuildLogEventAction(methodInfo);
                }
            }
        }

        private static Action<LogEventInfo, object[]> BuildLogEventAction(MethodInfo methodInfo)
        {
            var neededParameters = methodInfo.GetParameters().Length;

            ReflectionHelpers.LateBoundMethod lateBoundMethod = null;
            return (logEvent, parameters) =>
            {
                var missingParameters = neededParameters - parameters.Length;
                if (missingParameters > 0)
                {
                    //fill missing parameters with Type.Missing
                    var newParams = new object[neededParameters];
                    for (int i = 0; i < parameters.Length; ++i)
                        newParams[i] = parameters[i];
                    for (int i = parameters.Length; i < neededParameters; ++i)
                        newParams[i] = Type.Missing;
                    parameters = newParams;
                    methodInfo.Invoke(null, parameters);
                }
                else if (parameters.Length != neededParameters && neededParameters != 0)
                {
                    methodInfo.Invoke(null, parameters);
                }
                else
                {
                    parameters = neededParameters == 0 ? ArrayHelper.Empty<object>() : parameters;
                    if (lateBoundMethod is null)
                        lateBoundMethod = CreateFastInvoke(methodInfo, parameters) ?? CreateNormalInvoke(methodInfo, parameters);
                    else
                        lateBoundMethod.Invoke(null, parameters);
                }
            };
        }

        private static ReflectionHelpers.LateBoundMethod CreateFastInvoke(MethodInfo methodInfo, object[] parameters)
        {
            try
            {
                var lateBoundMethod = ReflectionHelpers.CreateLateBoundMethod(methodInfo);
                lateBoundMethod.Invoke(null, parameters);
                return lateBoundMethod;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "MethodCallTarget: Failed to create expression method {0} - {1}", methodInfo.Name,  ex.Message);
                return null;
            }
        }

        private static ReflectionHelpers.LateBoundMethod CreateNormalInvoke(MethodInfo methodInfo, object[] parameters)
        {
            ReflectionHelpers.LateBoundMethod reflectionMethod = (target, args) => methodInfo.Invoke(null, args);

            try
            {
                reflectionMethod.Invoke(null, parameters);
                return reflectionMethod;
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "MethodCallTarget: Failed to invoke reflection method {0} - {1}", methodInfo.Name, ex.Message);
                return reflectionMethod;
            }
        }

        /// <summary>
        /// Calls the specified Method.
        /// </summary>
        /// <param name="parameters">Method parameters.</param>
        /// <param name="logEvent">The logging event.</param>
        protected override void DoInvoke(object[] parameters, AsyncLogEventInfo logEvent)
        {
            try
            {
                ExecuteLogMethod(parameters, logEvent.LogEvent);
                logEvent.Continuation(null);
            }
            catch (Exception ex)
            {
                if (ExceptionMustBeRethrown(ex))
                {
                    throw;
                }

                logEvent.Continuation(ex);
            }
        }

        /// <summary>
        /// Calls the specified Method. 
        /// </summary>
        /// <param name="parameters">Method parameters.</param>
        protected override void DoInvoke(object[] parameters)
        {
            ExecuteLogMethod(parameters, null);
        }

        private void ExecuteLogMethod(object[] parameters, LogEventInfo logEvent)
        {
            if (_logEventAction is null)
            {
                InternalLogger.Debug("{0}: No invoke because class/method was not found or set", this);
            }
            else
            {
                try
                {
                    _logEventAction.Invoke(logEvent, parameters);
                }
                catch (TargetInvocationException ex)
                {
                    InternalLogger.Warn("{0}: Failed to invoke method - {1}", this, ex.Message);
                    throw ex.InnerException;
                }
            }
        }
    }
}
