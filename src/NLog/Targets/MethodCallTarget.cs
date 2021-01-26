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

namespace NLog.Targets
{
    using System;
    using System.Reflection;
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Calls the specified static method on each log message and passes contextual parameters to it.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/MethodCall-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/MethodCall/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
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

        /// <summary>
        /// Initializes the target.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (ClassName != null && MethodName != null)
            {
                _logEventAction = null;

                var targetType = Type.GetType(ClassName);
                if (targetType != null)
                {
                    var methodInfo = targetType.GetMethod(MethodName);
                    if (methodInfo == null)
                    {
                        throw new NLogConfigurationException($"MethodCallTarget: MethodName={MethodName} not found in ClassName={ClassName} - it should be static");
                    }
                    else
                    {
                        _logEventAction = BuildLogEventAction(methodInfo);
                    }
                }
                else
                {
                    throw new NLogConfigurationException($"MethodCallTarget: failed to get type from ClassName={ClassName}");
                }
            }
            else if (_logEventAction == null)
            {
                throw new NLogConfigurationException($"MethodCallTarget: Missing configuration of ClassName and MethodName");
            }
        }

        private static Action<LogEventInfo, object[]> BuildLogEventAction(MethodInfo methodInfo)
        {
            var neededParameters = methodInfo.GetParameters().Length;
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
                }

                methodInfo.Invoke(null, parameters);
            };
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
            if (_logEventAction != null)
            {
                _logEventAction.Invoke(logEvent, parameters);
            }
            else
            {
                InternalLogger.Trace("{0}: No invoke because class/method was not found or set", this);
            }
        }
    }
}
