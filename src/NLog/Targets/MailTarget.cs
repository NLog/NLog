//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

#if NETFRAMEWORK
    using System.Configuration;
    using System.Net.Configuration;
#endif

    /// <summary>
    /// Sends log messages by email using SMTP protocol.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/nlog/nlog/wiki/Mail-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/Mail-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="https://github.com/NLog/NLog/wiki/Configuration-file">configuration file</a>,
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Mail/Simple/NLog.config" />
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Mail/Simple/Example.cs" />
    /// <p>
    /// Mail target works best when used with BufferingWrapper target
    /// which lets you send multiple log messages in single mail
    /// </p>
    /// <p>
    /// To set up the buffered mail target in the <a href="https://github.com/NLog/NLog/wiki/Configuration-file">configuration file</a>,
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Mail/Buffered/NLog.config" />
    /// <p>
    /// To set up the buffered mail target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Mail/Buffered/Example.cs" />
    /// </example>
    [Target("Mail")]
    [Target("Email")]
    [Target("Smtp")]
    [Target("SmtpClient")]
    public class MailTarget : TargetWithLayoutHeaderAndFooter
    {
        private const string RequiredPropertyIsEmptyFormat = "After the processing of the MailTarget's '{0}' property it appears to be empty. The email message will not be sent.";

        private Layout _from;

        /// <summary>
        /// Initializes a new instance of the <see cref="MailTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        public MailTarget()
        {
            Body = "${message}${newline}";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MailTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        /// <param name="name">Name of the target.</param>
        public MailTarget(string name) : this()
        {
            Name = name;
        }

#if NETFRAMEWORK
        private SmtpSection _currentailSettings;

        /// <summary>
        /// Gets the mailSettings/smtp configuration from app.config in cases when we need those configuration.
        /// E.g when UseSystemNetMailSettings is enabled and we need to read the From attribute from system.net/mailSettings/smtp
        /// </summary>
        /// <remarks>Internal for mocking</remarks>
        internal SmtpSection SmtpSection
        {
            get
            {
                if (_currentailSettings is null)
                {
                    try
                    {
                        _currentailSettings = System.Configuration.ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;
                    }
                    catch (Exception ex)
                    {
                        InternalLogger.Warn(ex, "{0}: Reading 'From' from .config failed.", this);

                        if (LogManager.ThrowExceptions)
                            throw;

                        _currentailSettings = new SmtpSection();
                    }
                }

                return _currentailSettings;
            }
            set => _currentailSettings = value;
        }
#endif

        /// <summary>
        /// Gets or sets sender's email address (e.g. joe@domain.com).
        /// </summary>
        /// <docgen category='Message Options' order='10' />
        [RequiredParameter]
        public Layout From
        {
            get
            {
#if NETFRAMEWORK

                // In contrary to other settings, System.Net.Mail.SmtpClient doesn't read the 'From' attribute from the system.net/mailSettings/smtp section in the config file.
                // Thus, when UseSystemNetMailSettings is enabled we have to read the configuration section of system.net/mailSettings/smtp to initialize the 'From' address.
                // It will do so only if the 'From' attribute in system.net/mailSettings/smtp is not empty.

                //only use from config when not set in current
                if (UseSystemNetMailSettings && _from is null)
                {
                    var from = SmtpSection.From;
                    return from;
                }
#endif
                return _from;
            }
            set { _from = value; }
        }

        /// <summary>
        /// Gets or sets recipients' email addresses separated by semicolons (e.g. john@domain.com;jane@domain.com).
        /// </summary>
        /// <docgen category='Message Options' order='11' />
        [RequiredParameter]
        public Layout To { get; set; }

        /// <summary>
        /// Gets or sets CC email addresses separated by semicolons (e.g. john@domain.com;jane@domain.com).
        /// </summary>
        /// <docgen category='Message Options' order='12' />
        public Layout CC { get; set; }

        /// <summary>
        /// Gets or sets BCC email addresses separated by semicolons (e.g. john@domain.com;jane@domain.com).
        /// </summary>
        /// <docgen category='Message Options' order='13' />
        public Layout Bcc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to add new lines between log entries.
        /// </summary>
        /// <value>A value of <c>true</c> if new lines should be added; otherwise, <c>false</c>.</value>
        /// <docgen category='Message Options' order='99' />
        public bool AddNewLines { get; set; }

        /// <summary>
        /// Gets or sets the mail subject.
        /// </summary>
        /// <docgen category='Message Options' order='5' />
        [RequiredParameter]
        public Layout Subject { get; set; } = "Message from NLog on ${machinename}";

        /// <summary>
        /// Gets or sets mail message body (repeated for each log message send in one mail).
        /// </summary>
        /// <remarks>Alias for the <c>Layout</c> property.</remarks>
        /// <docgen category='Message Options' order='6' />
        public Layout Body
        {
            get => Layout;
            set => Layout = value;
        }

        /// <summary>
        /// Gets or sets encoding to be used for sending e-mail.
        /// </summary>
        /// <docgen category='Message Options' order='20' />
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Gets or sets a value indicating whether to send message as HTML instead of plain text.
        /// </summary>
        /// <docgen category='Message Options' order='11' />
        public bool Html { get; set; }

        /// <summary>
        /// Gets or sets SMTP Server to be used for sending.
        /// </summary>
        /// <docgen category='SMTP Options' order='10' />
        public Layout SmtpServer { get; set; }

        /// <summary>
        /// Gets or sets SMTP Authentication mode.
        /// </summary>
        /// <docgen category='SMTP Options' order='11' />
        public SmtpAuthenticationMode SmtpAuthentication { get; set; } = SmtpAuthenticationMode.None;

        /// <summary>
        /// Gets or sets the username used to connect to SMTP server (used when SmtpAuthentication is set to "basic").
        /// </summary>
        /// <docgen category='SMTP Options' order='12' />
        public Layout SmtpUserName { get; set; }

        /// <summary>
        /// Gets or sets the password used to authenticate against SMTP server (used when SmtpAuthentication is set to "basic").
        /// </summary>
        /// <docgen category='SMTP Options' order='13' />
        public Layout SmtpPassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL (secure sockets layer) should be used when communicating with SMTP server.
        /// </summary>
        /// <docgen category='SMTP Options' order='14' />.
        public bool EnableSsl { get; set; }

        /// <summary>
        /// Gets or sets the port number that SMTP Server is listening on.
        /// </summary>
        /// <docgen category='SMTP Options' order='15' />
        public Layout<int> SmtpPort { get; set; } = 25;

        /// <summary>
        /// Gets or sets a value indicating whether the default Settings from System.Net.MailSettings should be used.
        /// </summary>
        /// <docgen category='SMTP Options' order='16' />
        public bool UseSystemNetMailSettings { get; set; }

        /// <summary>
        /// Specifies how outgoing email messages will be handled.
        /// </summary>
        /// <docgen category='SMTP Options' order='18' />
        public SmtpDeliveryMethod DeliveryMethod { get; set; } = SmtpDeliveryMethod.Network;

        /// <summary>
        /// Gets or sets the folder where applications save mail messages to be processed by the local SMTP server.
        /// </summary>
        /// <docgen category='SMTP Options' order='17' />
        public Layout PickupDirectoryLocation { get; set; }

        /// <summary>
        /// Gets or sets the priority used for sending mails.
        /// </summary>
        /// <docgen category='Message Options' order='100' />
        public Layout<MailPriority> Priority { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether NewLine characters in the body should be replaced with <br/> tags.
        /// </summary>
        /// <remarks>Only happens when <see cref="Html"/> is set to true.</remarks>
        /// <docgen category='Message Options' order='100' />
        public bool ReplaceNewlineWithBrTagInHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the SMTP client timeout.
        /// </summary>
        /// <remarks>Warning: zero is not infinite waiting</remarks>
        /// <docgen category='SMTP Options' order='100' />
        public Layout<int> Timeout { get; set; } = 10000;

        /// <summary>
        /// Gets the array of email headers that are transmitted with this email message
        /// </summary>
        /// <docgen category='Message Options' order='100' />
        [ArrayParameter(typeof(MethodCallParameter), "mailheader")]
        public IList<MethodCallParameter> MailHeaders { get; } = new List<MethodCallParameter>();

        internal virtual ISmtpClient CreateSmtpClient()
        {
            return new MySmtpClient();
        }

        /// <inheritdoc/>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            Write((IList<AsyncLogEventInfo>)new[] { logEvent });
        }

        /// <inheritdoc/>
        protected override void Write(IList<AsyncLogEventInfo> logEvents)
        {
            if (logEvents.Count == 1)
            {
                ProcessSingleMailMessage(logEvents);
            }
            else
            {
                var buckets = logEvents.GroupBy(l => GetSmtpSettingsKey(l.LogEvent));
                foreach (var bucket in buckets)
                {
                    var eventInfos = bucket;
                    ProcessSingleMailMessage(eventInfos);
                }
            }
        }

        /// <inheritdoc/>
        protected override void InitializeTarget()
        {
            CheckRequiredParameters();

            base.InitializeTarget();
        }

        /// <summary>
        /// Create mail and send with SMTP
        /// </summary>
        /// <param name="events">event printed in the body of the event</param>
        private void ProcessSingleMailMessage(IEnumerable<AsyncLogEventInfo> events)
        {
            try
            {
                LogEventInfo firstEvent = events.FirstOrDefault().LogEvent;
                LogEventInfo lastEvent = events.LastOrDefault().LogEvent;
                if (firstEvent is null || lastEvent is null)
                {
                    throw new NLogRuntimeException("We need at least one event.");
                }

                // unbuffered case, create a local buffer, append header, body and footer
                var bodyBuffer = CreateBodyBuffer(events, firstEvent, lastEvent);

                using (var msg = CreateMailMessage(lastEvent, bodyBuffer.ToString()))
                {
                    using (ISmtpClient client = CreateSmtpClient())
                    {
                        if (!UseSystemNetMailSettings)
                        {
                            ConfigureMailClient(lastEvent, client);
                        }

                        if (client.EnableSsl)
                            InternalLogger.Debug("{0}: Sending mail to {1} using {2}:{3} (ssl=true)", this, msg.To, client.Host, client.Port);
                        else
                            InternalLogger.Debug("{0}: Sending mail to {1} using {2}:{3} (ssl=false)", this, msg.To, client.Host, client.Port);

                        InternalLogger.Trace("{0}:   Subject: '{1}'", this, msg.Subject);
                        InternalLogger.Trace("{0}:   From: '{1}'", this, msg.From);

                        client.Send(msg);

                        foreach (var ev in events)
                        {
                            ev.Continuation(null);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                //always log
                InternalLogger.Error(exception, "{0}: Error sending mail.", this);

                if (LogManager.ThrowExceptions)
                    throw;

                foreach (var ev in events)
                {
                    ev.Continuation(exception);
                }
            }
        }

        /// <summary>
        /// Create buffer for body
        /// </summary>
        /// <param name="events">all events</param>
        /// <param name="firstEvent">first event for header</param>
        /// <param name="lastEvent">last event for footer</param>
        /// <returns></returns>
        private StringBuilder CreateBodyBuffer(IEnumerable<AsyncLogEventInfo> events, LogEventInfo firstEvent, LogEventInfo lastEvent)
        {
            var bodyBuffer = new StringBuilder();
            if (Header != null)
            {
                bodyBuffer.Append(RenderLogEvent(Header, firstEvent));
                if (AddNewLines)
                {
                    bodyBuffer.Append('\n');
                }
            }

            foreach (AsyncLogEventInfo eventInfo in events)
            {
                bodyBuffer.Append(RenderLogEvent(Layout, eventInfo.LogEvent));
                if (AddNewLines)
                {
                    bodyBuffer.Append('\n');
                }
            }

            if (Footer != null)
            {
                bodyBuffer.Append(RenderLogEvent(Footer, lastEvent));
                if (AddNewLines)
                {
                    bodyBuffer.Append('\n');
                }
            }
            return bodyBuffer;
        }

        /// <summary>
        /// Set properties of <paramref name="client"/>
        /// </summary>
        /// <param name="lastEvent">last event for username/password</param>
        /// <param name="client">client to set properties on</param>
        /// <remarks>Configure not at <see cref="InitializeTarget"/>, as the properties could have layout renderers.</remarks>
        internal void ConfigureMailClient(LogEventInfo lastEvent, ISmtpClient client)
        {
            CheckRequiredParameters();

            if (DeliveryMethod == SmtpDeliveryMethod.Network)
            {
                var smtpServer = RenderLogEvent(SmtpServer, lastEvent);
                if (string.IsNullOrEmpty(smtpServer))
                {
                    throw new NLogRuntimeException(string.Format(RequiredPropertyIsEmptyFormat, "SmtpServer"));
                }

                var smtpPort = RenderLogEvent(SmtpPort, lastEvent, 25);
                if (smtpPort <= 0)
                {
                    throw new NLogRuntimeException(string.Format(RequiredPropertyIsEmptyFormat, "SmtpPort"));
                }

                client.Host = smtpServer;
                client.Port = smtpPort;
                client.EnableSsl = EnableSsl;

                if (SmtpAuthentication == SmtpAuthenticationMode.Ntlm)
                {
                    InternalLogger.Trace("{0}:   Using NTLM authentication.", this);
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;
                }
                else if (SmtpAuthentication == SmtpAuthenticationMode.Basic)
                {
                    string username = RenderLogEvent(SmtpUserName, lastEvent);
                    string password = RenderLogEvent(SmtpPassword, lastEvent);

                    InternalLogger.Trace("{0}:   Using basic authentication: Username='{1}' Password='{2}'", this, username, new string('*', password.Length));
                    client.Credentials = new NetworkCredential(username, password);
                }
            }

            if (DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory)
            {
                var pickupDirectoryLocation = RenderLogEvent(PickupDirectoryLocation, lastEvent);
                if (string.IsNullOrEmpty(pickupDirectoryLocation))
                {
                    throw new NLogRuntimeException(string.Format(RequiredPropertyIsEmptyFormat, "PickupDirectoryLocation"));
                }

                client.PickupDirectoryLocation = ConvertDirectoryLocation(pickupDirectoryLocation);
            }

            // In case DeliveryMethod = PickupDirectoryFromIis we will not require Host nor PickupDirectoryLocation
            client.DeliveryMethod = DeliveryMethod;
            client.Timeout = RenderLogEvent(Timeout, lastEvent, 10000);
        }

        /// <summary>
        /// Handle <paramref name="pickupDirectoryLocation"/> if it is a virtual directory.
        /// </summary>
        /// <param name="pickupDirectoryLocation"></param>
        /// <returns></returns>
        internal static string ConvertDirectoryLocation(string pickupDirectoryLocation)
        {
            const string virtualPathPrefix = "~/";
            if (!pickupDirectoryLocation.StartsWith(virtualPathPrefix, StringComparison.Ordinal))
            {
                return pickupDirectoryLocation;
            }

            // Support for Virtual Paths
            var root = AppDomain.CurrentDomain.BaseDirectory;
            var directory = pickupDirectoryLocation.Substring(virtualPathPrefix.Length).Replace('/', Path.DirectorySeparatorChar);
            var pickupRoot = Path.Combine(root, directory);
            return pickupRoot;
        }

        private void CheckRequiredParameters()
        {
            if (!UseSystemNetMailSettings && DeliveryMethod == SmtpDeliveryMethod.Network && SmtpServer is null)
            {
                throw new NLogConfigurationException($"The MailTarget's '{nameof(SmtpServer)}' properties are not set - but needed because useSystemNetMailSettings=false and DeliveryMethod=Network. The email message will not be sent.");
            }

            if (!UseSystemNetMailSettings && DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory && PickupDirectoryLocation is null)
            {
                throw new NLogConfigurationException($"The MailTarget's '{nameof(PickupDirectoryLocation)}' properties are not set - but needed because useSystemNetMailSettings=false and DeliveryMethod=SpecifiedPickupDirectory. The email message will not be sent.");
            }
        }

        /// <summary>
        /// Create key for grouping. Needed for multiple events in one mail message
        /// </summary>
        /// <param name="logEvent">event for rendering layouts   </param>
        /// <returns>string to group on</returns>
        private string GetSmtpSettingsKey(LogEventInfo logEvent)
        {
            return $@"{RenderLogEvent(From, logEvent)}
{RenderLogEvent(To, logEvent)}
{RenderLogEvent(CC, logEvent)}
{RenderLogEvent(Bcc, logEvent)}
{RenderLogEvent(SmtpServer, logEvent)}
{RenderLogEvent(SmtpPassword, logEvent)}
{RenderLogEvent(SmtpUserName, logEvent)}
{RenderLogEvent(PickupDirectoryLocation, logEvent)}";
        }

        /// <summary>
        /// Create the mail message with the addresses, properties and body.
        /// </summary>
        private MailMessage CreateMailMessage(LogEventInfo lastEvent, string body)
        {
            var msg = new MailMessage();

            var renderedFrom = RenderLogEvent(From, lastEvent);

            if (string.IsNullOrEmpty(renderedFrom))
            {
                throw new NLogRuntimeException(string.Format(RequiredPropertyIsEmptyFormat, "From"));
            }
            msg.From = new MailAddress(renderedFrom);

            var addedTo = AddAddresses(msg.To, To, lastEvent);
            var addedCc = AddAddresses(msg.CC, CC, lastEvent);
            var addedBcc = AddAddresses(msg.Bcc, Bcc, lastEvent);

            if (!addedTo && !addedCc && !addedBcc)
            {
                throw new NLogRuntimeException(string.Format(RequiredPropertyIsEmptyFormat, "To/Cc/Bcc"));
            }

            msg.Subject = (RenderLogEvent(Subject, lastEvent) ?? string.Empty).Trim();
            msg.BodyEncoding = Encoding;
            msg.IsBodyHtml = Html;

            if (Priority != null)
            {
                msg.Priority = RenderLogEvent(Priority, lastEvent, MailPriority.Normal);
            }

            msg.Body = body;
            if (msg.IsBodyHtml && ReplaceNewlineWithBrTagInHtml && msg.Body != null)
                msg.Body = msg.Body.Replace(Environment.NewLine, "<br/>");

            if (MailHeaders?.Count > 0)
            {
                for (int i = 0; i < MailHeaders.Count; i++)
                {
                    string headerValue = RenderLogEvent(MailHeaders[i].Layout, lastEvent);
                    if (headerValue is null)
                        continue;

                    msg.Headers.Add(MailHeaders[i].Name, headerValue);
                }
            }

            return msg;
        }

        /// <summary>
        /// Render  <paramref name="layout"/> and add the addresses to <paramref name="mailAddressCollection"/>
        /// </summary>
        /// <param name="mailAddressCollection">Addresses appended to this list</param>
        /// <param name="layout">layout with addresses, ; separated</param>
        /// <param name="logEvent">event for rendering the <paramref name="layout"/></param>
        /// <returns>added a address?</returns>
        private bool AddAddresses(MailAddressCollection mailAddressCollection, Layout layout, LogEventInfo logEvent)
        {
            var added = false;
            var mailAddresses = RenderLogEvent(layout, logEvent);

            if (!string.IsNullOrEmpty(mailAddresses))
            {
                foreach (string mail in mailAddresses.Split(';'))
                {
                    var mailAddress = mail.Trim();
                    if (string.IsNullOrEmpty(mailAddress))
                        continue;

                    mailAddressCollection.Add(mailAddress);
                    added = true;
                }
            }

            return added;
        }
    }
}
