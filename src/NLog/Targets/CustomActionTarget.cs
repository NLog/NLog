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


namespace NLog.Targets
{
    using System;
    using NLog.Common;
    using System.Collections.Generic;
    using NLog.Internal;

    /// <summary>
    /// Allows the user to register one or more custom actions to be performed when a logging event is triggered
    /// </summary>
    [Target("CustomAction")]
    public class CustomActionTarget : TargetWithLayout
    {
        static CustomActionTarget()
        {
            ActionProviders = new HashSet<IActionProvider>();
        }

        /// <summary>
        /// Holds the registered instances of <see cref="IActionProvider"/> on which to perform actions.
        /// </summary>
        protected static HashSet<IActionProvider> ActionProviders { get; private set; }

        private static object sync = new object();

        /// <summary>
        /// Registers an instance of <see cref="IActionProvider"/>
        /// </summary>
        /// <param name="actionProvider">The instance of <see cref="IActionProvider"/> to register.</param>
        /// <returns>True if the action provider was just registered, false if it was already registered.</returns>
        public static bool Register(IActionProvider actionProvider)
        {
            lock (sync)
                return ActionProviders.Add(actionProvider);
        }

        /// <summary>
        /// Unregisters an instance of <see cref="IActionProvider"/>
        /// </summary>
        /// <param name="actionProvider">The instance of <see cref="IActionProvider"/> to unregister.</param>
        /// <returns>True if the action provider was just unregistered, false if it was not found.</returns>
        public static bool Unregister(IActionProvider actionProvider)
        {
            lock (sync)
                return ActionProviders.Remove(actionProvider);
        }

        /// <summary>
        /// Deletes all registered items from the set
        /// </summary>
        public static void ClearRegistrations()
        {
            lock (sync)
                ActionProviders.Clear();
        }

        /// <summary>
        /// Performs the actions on the registered action prividers using an event to be logged.
        /// </summary>
        /// <param name="logEvent">The logging event with which to perform the action.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            Exception firstException = null;
            lock (sync)
            {
                foreach (IActionProvider actionProvider in ActionProviders)
                {
                    Exception ex = InvokeAction(actionProvider, logEvent);
                    if (ex != null && firstException == null)
                        firstException = ex;
                }
            }
            if (firstException != null)
                throw firstException;
        }

        private Exception InvokeAction(IActionProvider actionProvider, LogEventInfo logEvent)
        {
            Exception exception = null;
            try
            {
                actionProvider.Action(this, logEvent);
            }
            catch (Exception ex)
            {
                if (ex.MustBeRethrown())
                {
                    throw;
                }
                InternalLogger.Error("Error when performing action on type {0} : {1}", actionProvider.GetType(), ex);
                exception = ex;
            }
            return exception;
        }
    }
}

