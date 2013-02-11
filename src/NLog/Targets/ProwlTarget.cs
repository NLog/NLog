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

#if !NET_CF && !SILVERLIGHT
using System;
using System.Net;
using System.Text;
using NLog.Config;
using NLog.Layouts;

namespace NLog.Targets
{
    /// <summary>
    /// Writes log message to prowl pn service.
    /// </summary>
    /// <seealso href="http://nlog-project.org/wiki/Prowl_target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Prowl/Simple/NLog.config" />
    /// <p>
    /// The above examples assume just one target and a single rule. 
    /// More configuration options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Prowl/Simple/Example.cs" />
    /// </example>
    [Target("Prowl")]
    public sealed class ProwlTarget : TargetWithLayout
    {
        private struct ProwlNotification
        {
            private const int DESCRIPTION_MAX_LENGTH = 10000;
            private const int EVENT_MAX_LENGTH = 1024;

            private const string EX_MSG_DESCRIPTION_EXCEEDS_MAX_LENGTH =
                "Provided pn description exceeds the maximum allowed length [{0}]; unable to proceed.";

            private const string EX_MSG_DESCRIPTION_NOT_PROVIDED =
                "Notification description not provided; unable to proceed.";

            private const string EX_MSG_EVENT_EXCEEDS_MAX_LENGTH =
                "Provided pn event exceeds the maximum allowed length [{0}]; unable to proceed.";

            private const string EX_MSG_EVENT_NOT_PROVIDED =
                "Notification event not provided; unable to proceed.";

            internal void Validate()
            {
                if (String.IsNullOrEmpty(Description))
                    throw new InvalidOperationException(EX_MSG_DESCRIPTION_NOT_PROVIDED);

                if (Description.Length > DESCRIPTION_MAX_LENGTH)
                    throw new InvalidOperationException(String.Format(EX_MSG_DESCRIPTION_EXCEEDS_MAX_LENGTH,
                                                                      DESCRIPTION_MAX_LENGTH));

                if (String.IsNullOrEmpty(Event))
                    throw new InvalidOperationException(EX_MSG_EVENT_NOT_PROVIDED);

                if (Event.Length > EVENT_MAX_LENGTH)
                    throw new InvalidOperationException(String.Format(EX_MSG_EVENT_EXCEEDS_MAX_LENGTH, EVENT_MAX_LENGTH));
            }

            internal string Event { get; set; }
            internal string Description { get; set; }

            internal ProwlNotificationPriority Priority { get; set; }
        }


        internal enum ProwlNotificationPriority : sbyte
        {
            VeryLow = -2,
            Moderate = -1,
            Normal = 0,
            High = 1,
            Emergency = 2
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProwlTarget"/> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public ProwlTarget()
        {
        }



        /// <summary>
        /// Gets or sets a comma-separated list of prowl api keys to send notifications to
        /// </summary>
        /// <remarks>
        /// To write to a private queue on a local machine use <c>.\private$\QueueName</c>.
        /// For other available queue names, consult MSMQ documentation.
        /// </remarks>
        /// <docgen category='Prowl Options' order='10' />
        [RequiredParameter]
        public Layout ApiKeys { get; set; }

        /// <summary>
        /// Gets or sets the event name to associate with each message.
        /// </summary>
        /// <docgen category='Prowl Options' order='10' />
        [RequiredParameter]
        public Layout EventLayout { get; set; }

        /// <summary>
        /// Gets or sets the description name to associate with each message.
        /// </summary>
        /// <docgen category='Prowl Options' order='10' />
        [RequiredParameter]
        public Layout DescriptionLayout { get; set; }

        /// <summary>
        /// Gets or sets the application name to associate with each message.
        /// </summary>
        /// <docgen category='Prowl Options' order='10' />
        [RequiredParameter]
        public Layout ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets an optional provider key to access the prowl api with
        /// </summary>
        /// <docgen category='Prowl Options' order='10' />
        public string ProviderKey { get; set; }

        private const string POST_NOTIFICATION_BASE_METHOD =
            "add?apikey={0}&application={1}&description={2}&event={3}&priority={4}";

        private const int API_KEYCHAIN_MAX_LENGTH = 204;
        private const int APPLICATION_NAME_MAX_LENGTH = 256;

        private const string DEFAULT_BASE_URL = @"https://api.prowlapp.com/publicapi/";

        private const string POST_NOTIFICATION_PROVIDER_PARAMETER = "&provider={0}";
        private const string REQUEST_CONTENT_TYPE = "application/x-www-form-urlencoded";
        private const string REQUEST_METHOD_TYPE = "POST";


        private const string EX_MSG_API_KEYCHAIN_EXCEEDS_MAX_LENGTH =
            "Provided Prowl API keychain exceeds the maximum allowed length [{0}]; unable to proceed.";

        private const string EX_MSG_API_KEYCHAIN_NOT_PROVIDED =
            "Prowl API keychain not provided; unable to proceed.";

        private const string EX_MSG_APPLICATION_NAME_EXCEEDS_MAX_LENGTH =
            "Provided Prowl application name exceeds the maximum allowed length [{0}]; unable to proceed.";

        private const string EX_MSG_APPLICATION_NAME_NOT_PROVIDED =
            "Prowl application name not provided; unable to proceed.";


        private void Validate(string apiKey, string appName)
        {
            if (String.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException(EX_MSG_API_KEYCHAIN_NOT_PROVIDED);

            if (apiKey.Length > API_KEYCHAIN_MAX_LENGTH)
                throw new InvalidOperationException(String.Format(EX_MSG_API_KEYCHAIN_EXCEEDS_MAX_LENGTH,
                                                                  API_KEYCHAIN_MAX_LENGTH));

            if (String.IsNullOrEmpty(appName))
                throw new InvalidOperationException(EX_MSG_APPLICATION_NAME_NOT_PROVIDED);

            if (appName.Length > APPLICATION_NAME_MAX_LENGTH)
                throw new InvalidOperationException(String.Format(EX_MSG_APPLICATION_NAME_EXCEEDS_MAX_LENGTH,
                                                                  APPLICATION_NAME_MAX_LENGTH));
        }

        /// <summary>
        /// Writes the specified logging event to a queue specified in the Queue 
        /// parameter.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (this.ApiKeys == null)
            {
                return;
            }

            var apiKeys = this.ApiKeys.Render(logEvent).Split(',', ';');

            var pn = new ProwlNotification
                         {
                             Event = this.EventLayout.Render(logEvent),
                             Description = this.DescriptionLayout.Render(logEvent),
                             Priority = TranslatePriority(logEvent.Level),
                         };

            pn.Validate();

            var appName = this.ApplicationName.Render(logEvent);

            foreach (var ak in apiKeys)
            {
                Validate(ak, appName);
                PostNotification(pn, ak, appName);
            }

        }

        private ProwlNotificationPriority TranslatePriority(LogLevel level)
        {
            if (level == LogLevel.Fatal)
                return ProwlNotificationPriority.Emergency;
            if (level == LogLevel.Error)
                return ProwlNotificationPriority.High;
            if (level == LogLevel.Warn)
                return ProwlNotificationPriority.Normal;
            if (level == LogLevel.Info)
                return ProwlNotificationPriority.Normal;
            if (level == LogLevel.Debug)
                return ProwlNotificationPriority.Moderate;
            if (level == LogLevel.Trace)
                return ProwlNotificationPriority.VeryLow;

            return ProwlNotificationPriority.Normal;
        }


        private void PostNotification(ProwlNotification pn, string apiKey, string appName)
        {
            var updateRequest =
                HttpWebRequest.Create(BuildNotificationRequestUrl(pn, apiKey, appName)) as HttpWebRequest;

            updateRequest.ContentLength = 0;
            updateRequest.ContentType = REQUEST_CONTENT_TYPE;
            updateRequest.Method = REQUEST_METHOD_TYPE;

            var postResponse = default(WebResponse);

            try
            {
                postResponse = updateRequest.GetResponse();
            }
            finally
            {
                if (postResponse != null)
                    postResponse.Close();
            }
        }

        private string BuildNotificationRequestUrl(ProwlNotification pn, string apiKey, string appName)
        {
            var prowlUrlSb = new StringBuilder(DEFAULT_BASE_URL);
            prowlUrlSb.AppendFormat(
                POST_NOTIFICATION_BASE_METHOD,
                Uri.EscapeDataString(apiKey),
                Uri.EscapeDataString(appName),
                Uri.EscapeDataString(pn.Description),
                Uri.EscapeDataString(pn.Event),
                ((sbyte) (pn.Priority)));

            if (!String.IsNullOrEmpty(ProviderKey))
                prowlUrlSb.AppendFormat(
                    POST_NOTIFICATION_PROVIDER_PARAMETER,
                    Uri.EscapeDataString(ProviderKey));

            return prowlUrlSb.ToString();
        }
    }
}

#endif