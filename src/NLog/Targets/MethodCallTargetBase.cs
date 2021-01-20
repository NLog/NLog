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
    using System.Collections.Generic;
    using System.Globalization;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// The base class for all targets which call methods (local or remote). 
    /// Manages parameters and type coercion.
    /// </summary>
    public abstract class MethodCallTargetBase : Target
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodCallTargetBase" /> class.
        /// </summary>
        protected MethodCallTargetBase()
        {
            Parameters = new List<MethodCallParameter>();
        }

        /// <summary>
        /// Gets the array of parameters to be passed.
        /// </summary>
        /// <docgen category='Parameter Options' order='10' />
        [ArrayParameter(typeof(MethodCallParameter), "parameter")]
        public IList<MethodCallParameter> Parameters { get; private set; }

        private IPropertyTypeConverter PropertyTypeConverter
        {
            get => _propertyTypeConverter ?? (_propertyTypeConverter = ResolveService<IPropertyTypeConverter>());
            set => _propertyTypeConverter = value;
        }
        private IPropertyTypeConverter _propertyTypeConverter;

        /// <inheritdoc/>
        protected override void CloseTarget()
        {
            PropertyTypeConverter = null;
            base.CloseTarget();
        }

        /// <summary>
        /// Prepares an array of parameters to be passed based on the logging event and calls DoInvoke().
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            object[] parameters = Parameters.Count > 0 ? new object[Parameters.Count] : ArrayHelper.Empty<object>();
            for (int i = 0; i < parameters.Length; ++i)
            {
                try
                {
                    parameters[i] = GetParameterValue(logEvent.LogEvent, Parameters[i]);
                }
                catch (Exception ex)
                {
                    if (ex.MustBeRethrownImmediately())
                        throw;

                    Common.InternalLogger.Warn(ex, "{0}: Failed to get parameter value {1}", this, Parameters[i].Name);
                    throw;
                }
            }

            DoInvoke(parameters, logEvent);
        }

        private object GetParameterValue(LogEventInfo logEvent, MethodCallParameter param)
        {
            var parameterType = param.ParameterType ?? typeof(string);

            var parameterValue = RenderLogEvent(param.Layout, logEvent) ?? string.Empty;
            if (parameterType == typeof(string) || parameterType == typeof(object))
                return parameterValue;

            if (string.IsNullOrEmpty(parameterValue) && parameterType.IsValueType())
            {
                return Activator.CreateInstance(param.ParameterType);
            }

            return PropertyTypeConverter.Convert(parameterValue, parameterType, null, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Calls the target DoInvoke method, and handles AsyncContinuation callback
        /// </summary>
        /// <param name="parameters">Method call parameters.</param>
        /// <param name="logEvent">The logging event.</param>
        protected virtual void DoInvoke(object[] parameters, AsyncLogEventInfo logEvent)
        {
            DoInvoke(parameters, logEvent.Continuation);
        }

        /// <summary>
        /// Calls the target DoInvoke method, and handles AsyncContinuation callback
        /// </summary>
        /// <param name="parameters">Method call parameters.</param>
        /// <param name="continuation">The continuation.</param>
        protected virtual void DoInvoke(object[] parameters, AsyncContinuation continuation)
        {
            try
            {
                DoInvoke(parameters);
                continuation(null);
            }
            catch (Exception ex)
            {
                if (ExceptionMustBeRethrown(ex))
                {
                    throw;
                }

                continuation(ex);
            }
        }

        /// <summary>
        /// Calls the target method. Must be implemented in concrete classes.
        /// </summary>
        /// <param name="parameters">Method call parameters.</param>
        protected abstract void DoInvoke(object[] parameters);
    }
}
