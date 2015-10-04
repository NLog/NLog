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

using JetBrains.Annotations;

#if !SILVERLIGHT

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using NLog.Common;
    using NLog.Config;
    using NLog.Internal;
    using NLog.Layouts;

    /// <summary>
    /// Sends log messages by email using SMTP protocol.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/Mail-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Mail/Simple/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Mail/Simple/Example.cs" />
    /// <p>
    /// Mail target works best when used with BufferingWrapper target
    /// which lets you send multiple log messages in single mail
    /// </p>
    /// <p>
    /// To set up the buffered mail target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Mail/Buffered/NLog.config" />
    /// <p>
    /// To set up the buffered mail target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Mail/Buffered/Example.cs" />
    /// </example>
    [Target("Mail")]
    public class MailTarget : TargetWithLayoutHeaderAndFooter
    {
        private const string RequiredPropertyIsEmptyFormat = "After the processing of the MailTarget's '{0}' property it appears to be empty. The email message will not be sent.";

        /// <summary>
        /// Initializes a new instance of the <see cref="MailTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "This one is safe.")]
        public MailTarget()
        {
            this.Body = "${message}${newline}";
            this.Subject = "Message from NLog on ${machinename}";
            this.Encoding = Encoding.UTF8;
            this.SmtpPort = 25;
            this.SmtpAuthentication = SmtpAuthenticationMode.None;
            this.Timeout = 10000;
        }

        /// <summary>
        /// Gets or sets sender's email address (e.g. joe@domain.com).
        /// </summary>
        /// <docgen category='Message Options' order='10' />
        [RequiredParameter]
        public Layout From { get; set; }

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
        /// <docgen category='Layout Options' order='99' />
        public bool AddNewLines { get; set; }

        /// <summary>
        /// Gets or sets the mail subject.
        /// </summary>
        /// <docgen category='Message Options' order='5' />
        [DefaultValue("Message from NLog on ${machinename}")]
        [RequiredParameter]
        public Layout Subject { get; set; }

        /// <summary>
        /// Gets or sets mail message body (repeated for each log message send in one mail).
        /// </summary>
        /// <remarks>Alias for the <c>Layout</c> property.</remarks>
        /// <docgen category='Message Options' order='6' />
        [DefaultValue("${message}${newline}")]
        public Layout Body
        {
            get { return this.Layout; }
            set { this.Layout = value; }
        }

        /// <summary>
        /// Gets or sets encoding to be used for sending e-mail.
        /// </summary>
        /// <docgen category='Layout Options' order='20' />
        [DefaultValue("UTF8")]
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to send message as HTML instead of plain text.
        /// </summary>
        /// <docgen category='Layout Options' order='11' />
        [DefaultValue(false)]
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
        [DefaultValue("None")]
        public SmtpAuthenticationMode SmtpAuthentication { get; set; }

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
        [DefaultValue(false)]
        public bool EnableSsl { get; set; }

        /// <summary>
        /// Gets or sets the port number that SMTP Server is listening on.
        /// </summary>
        /// <docgen category='SMTP Options' order='15' />
        [DefaultValue(25)]
        public int SmtpPort { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the default Settings from System.Net.MailSettings should be used.
        /// </summary>
        /// <docgen category='SMTP Options' order='16' />
        [DefaultValue(false)]
        public bool UseSystemNetMailSettings { get; set; }

        /// <summary>
        /// Gets or sets the folder where applications save mail messages to be processed by the local SMTP server.
        /// </summary>
        /// <docgen category='SMTP Options' order='17' />
        [DefaultValue(null)]
        public string PickupDirectoryLocation { get; set; }

        /// <summary>
        /// Gets or sets the priority used for sending mails.
        /// </summary>
        public Layout Priority { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether NewLine characters in the body should be replaced with <br/> tags.
        /// </summary>
        /// <remarks>Only happens when <see cref="Html"/> is set to true.</remarks>
        [DefaultValue(false)]
        public bool ReplaceNewlineWithBrTagInHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the SMTP client timeout.
        /// </summary>
        /// <remarks>Warning: zero is not infinit waiting</remarks>
        [DefaultValue(10000)]
        public int Timeout { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "This is a factory method.")]
        internal virtual ISmtpClient CreateSmtpClient()
        {
            return new MySmtpClient();
        }

        /// <summary>
        /// Renders the logging event message and adds it to the internal ArrayList of log messages.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(AsyncLogEventInfo logEvent)
        {
            this.Write(new[] { logEvent });
        }

        /// <summary>
        /// Renders an array logging events.
        /// </summary>
        /// <param name="logEvents">Array of logging events.</param>
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            foreach (var bucket in logEvents.BucketSort(c => this.GetSmtpSettingsKey(c.LogEvent)))
            {
                var eventInfos = bucket.Value;
                this.ProcessSingleMailMessage(eventInfos);
            }
        }

        /// <summary>
        /// Initializes the target. Can be used by inheriting classes
        /// to initialize logging.
        /// </summary>
        protected override void InitializeTarget()
        {

            CheckRequiredParameters();

            base.InitializeTarget();
        }

        /// <summary>
        /// Create mail and send with SMTP
        /// </summary>
        /// <param name="events">event printed in the body of the event</param>
        private void ProcessSingleMailMessage([NotNull] List<AsyncLogEventInfo> events)
        {

            try
            {
                if (events.Count == 0)
                {
                    throw new NLogRuntimeException("We need at least one event.");
                }

                LogEventInfo firstEvent = events[0].LogEvent;
                LogEventInfo lastEvent = events[events.Count - 1].LogEvent;

                // unbuffered case, create a local buffer, append header, body and footer
                var bodyBuffer = CreateBodyBuffer(events, firstEvent, lastEvent);

                using (var msg = CreateMailMessage(lastEvent, bodyBuffer.ToString()))
                {
                    using (ISmtpClient client = this.CreateSmtpClient())
                    {
                        if (!UseSystemNetMailSettings)
                            ConfigureMailClient(lastEvent, client);

                        InternalLogger.Debug("Sending mail to {0} using {1}:{2} (ssl={3})", msg.To, client.Host, client.Port, client.EnableSsl);
                        InternalLogger.Trace("  Subject: '{0}'", msg.Subject);
                        InternalLogger.Trace("  From: '{0}'", msg.From.ToString());

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
                InternalLogger.Error(exception.ToString());

                if (exception.MustBeRethrown())
                {
                    throw;
                }


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
            if (this.Header != null)
            {
                bodyBuffer.Append(this.Header.Render(firstEvent));
                if (this.AddNewLines)
                {
                    bodyBuffer.Append("\n");
                }
            }

            foreach (AsyncLogEventInfo eventInfo in events)
            {
                bodyBuffer.Append(this.Layout.Render(eventInfo.LogEvent));
                if (this.AddNewLines)
                {
                    bodyBuffer.Append("\n");
                }
            }

            if (this.Footer != null)
            {
                bodyBuffer.Append(this.Footer.Render(lastEvent));
                if (this.AddNewLines)
                {
                    bodyBuffer.Append("\n");
                }
            }
            return bodyBuffer;
        }

        /// <summary>
        /// Set properties of <paramref name="client"/>
        /// </summary>
        /// <param name="lastEvent">last event for username/password</param>
        /// <param name="client">client to set properties on</param>
        private void ConfigureMailClient(LogEventInfo lastEvent, ISmtpClient client)
        {

            CheckRequiredParameters();


            var renderedSmtpServer = this.SmtpServer.Render(lastEvent);
            if (string.IsNullOrEmpty(renderedSmtpServer) && string.IsNullOrEmpty(this.PickupDirectoryLocation))
            {

                throw new NLogRuntimeException(string.Format(RequiredPropertyIsEmptyFormat,
                    string.IsNullOrEmpty(renderedSmtpServer) ? "SmtpServer" : "PickupDirectoryLocation"));
            }




            client.Host = renderedSmtpServer;
            client.Port = this.SmtpPort;
            client.EnableSsl = this.EnableSsl;
            client.Timeout = this.Timeout;
            client.PickupDirectoryLocation = this.PickupDirectoryLocation;

            if (this.SmtpAuthentication == SmtpAuthenticationMode.Ntlm)
            {
                InternalLogger.Trace("  Using NTLM authentication.");
                client.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
            else if (this.SmtpAuthentication == SmtpAuthenticationMode.Basic)
            {
                string username = this.SmtpUserName.Render(lastEvent);
                string password = this.SmtpPassword.Render(lastEvent);

                InternalLogger.Trace("  Using basic authentication: Username='{0}' Password='{1}'", username, new string('*', password.Length));
                client.Credentials = new NetworkCredential(username, password);
            }
        }

        private void CheckRequiredParameters()
        {
            if (!this.UseSystemNetMailSettings && this.SmtpServer == null && string.IsNullOrEmpty(this.PickupDirectoryLocation))
            {
                throw new NLogConfigurationException(
                    string.Format("The MailTarget's '{0}' and '{1}' properties are not set - but needed because useSystemNetMailSettings=false. The email message will not be sent.", "SmtpServer", "PickupDirectoryLocation"));
            }
        }

        /// <summary>
        /// Create key for grouping. Needed for multiple events in one mailmessage
        /// </summary>
        /// <param name="logEvent">event for rendering layouts   </param>  
        ///<returns>string to group on</returns>
        private string GetSmtpSettingsKey(LogEventInfo logEvent)
        {
            var sb = new StringBuilder();

            AppendLayout(sb, logEvent, this.From);
            AppendLayout(sb, logEvent, this.To);
            AppendLayout(sb, logEvent, this.CC);
            AppendLayout(sb, logEvent, this.Bcc);
            AppendLayout(sb, logEvent, this.SmtpServer);
            AppendLayout(sb, logEvent, this.SmtpPassword);
            AppendLayout(sb, logEvent, this.SmtpUserName);


            return sb.ToString();
        }

        /// <summary>
        /// Append rendered layout to the stringbuilder
        /// </summary>
        /// <param name="sb">append to this</param>
        /// <param name="logEvent">event for rendering <paramref name="layout"/></param>
        /// <param name="layout">append if not <c>null</c></param>
        private static void AppendLayout(StringBuilder sb, LogEventInfo logEvent, Layout layout)
        {
            sb.Append("|");
            if (layout != null)
                sb.Append(layout.Render(logEvent));
        }

        /// <summary>
        /// Create the mailmessage with the addresses, properties and body.
        /// </summary>
        private MailMessage CreateMailMessage(LogEventInfo lastEvent, string body)
        {

            var msg = new MailMessage();
            var renderedFrom = this.From == null ? null : this.From.Render(lastEvent);
            if (string.IsNullOrEmpty(renderedFrom))
            {
                throw new NLogRuntimeException(RequiredPropertyIsEmptyFormat, "From");
            }
            msg.From = new MailAddress(renderedFrom);

            var addedTo = AddAddresses(msg.To, this.To, lastEvent);
            var addedCc = AddAddresses(msg.CC, this.CC, lastEvent);
            var addedBcc = AddAddresses(msg.Bcc, this.Bcc, lastEvent);

            if (!addedTo && !addedCc && !addedBcc)
            {
                throw new NLogRuntimeException(RequiredPropertyIsEmptyFormat, "To/Cc/Bcc");
            }

            msg.Subject = this.Subject == null ? string.Empty : this.Subject.Render(lastEvent).Trim();
            msg.BodyEncoding = this.Encoding;
            msg.IsBodyHtml = this.Html;

            if (this.Priority != null)
            {
                var renderedPriority = this.Priority.Render(lastEvent);
                try
                {

                    msg.Priority = (MailPriority)Enum.Parse(typeof(MailPriority), renderedPriority, true);
                }
                catch
                {
                    InternalLogger.Warn("Could not convert '{0}' to MailPriority, valid values are Low, Normal and High. Using normal priority as fallback.");
                    msg.Priority = MailPriority.Normal;
                }
            }
            msg.Body = body;
            if (msg.IsBodyHtml && ReplaceNewlineWithBrTagInHtml && msg.Body != null)
                msg.Body = msg.Body.Replace(EnvironmentHelper.NewLine, "<br/>");
            return msg;
        }

        /// <summary>
        /// Render  <paramref name="layout"/> and add the addresses to <paramref name="mailAddressCollection"/>
        /// </summary>
        /// <param name="mailAddressCollection">Addresses appended to this list</param>
        /// <param name="layout">layout with addresses, ; separated</param>
        /// <param name="logEvent">event for rendering the <paramref name="layout"/></param>
        /// <returns>added a address?</returns>
        private static bool AddAddresses(MailAddressCollection mailAddressCollection, Layout layout, LogEventInfo logEvent)
        {
            var added = false;
            if (layout != null)
            {
                foreach (string mail in layout.Render(logEvent).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mailAddressCollection.Add(mail);
                    added = true;
                }
            }

            return added;
        }
    }
}

#endif
