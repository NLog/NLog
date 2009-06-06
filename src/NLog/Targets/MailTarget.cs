// 
// Copyright (c) 2004-2009 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Text;
using NLog.Layouts;

namespace NLog.Targets
{
    /// <summary>
    /// Sends logging messages by email using SMTP protocol.
    /// </summary>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/Mail/Simple/NLog.config" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/Mail/Simple/Example.cs" />
    /// <p>
    /// Mail target works best when used with BufferingWrapper target
    /// which lets you send multiple logging messages in single mail
    /// </p>
    /// <p>
    /// To set up the buffered mail target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" src="examples/targets/Configuration File/Mail/Buffered/NLog.config" />
    /// <p>
    /// To set up the buffered mail target programmatically use code like this:
    /// </p>
    /// <code lang="C#" src="examples/targets/Configuration API/Mail/Buffered/Example.cs" />
    /// </example>
    [Target("Mail")]
    public class MailTarget : TargetWithLayoutHeaderAndFooter
    {
        /// <summary>
        /// Initializes a new instance of the MailTarget class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public MailTarget()
        {
            this.Body = "${message}${newline}";
            this.Subject = "Message from NLog on ${machinename}";
            this.Encoding = Encoding.UTF8;
            this.SmtpPort = 25;
            this.SmtpAuthentication = SmtpAuthenticationMode.None;
        }

        /// <summary>
        /// SMTP authentication modes.
        /// </summary>
        public enum SmtpAuthenticationMode
        {
            /// <summary>
            /// No authentication.
            /// </summary>
            None,

            /// <summary>
            /// Basic - username and password.
            /// </summary>
            Basic,

            /// <summary>
            /// NTLM Authentication.
            /// </summary>
            Ntlm,
        }

        /// <summary>
        /// Gets or sets sender's email address (e.g. joe@domain.com).
        /// </summary>
        public Layout From { get; set; }

        /// <summary>
        /// Gets or sets recipients' email addresses separated by semicolons (e.g. john@domain.com;jane@domain.com).
        /// </summary>
        public Layout To { get; set; }

        /// <summary>
        /// Gets or sets CC email addresses separated by semicolons (e.g. john@domain.com;jane@domain.com).
        /// </summary>
        public Layout CC { get; set; }

        /// <summary>
        /// Gets or sets BCC email addresses separated by semicolons (e.g. john@domain.com;jane@domain.com).
        /// </summary>
        public Layout Bcc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to add new lines between log entries.
        /// </summary>
        /// <value>A value of <c>true</c> if new lines should be added; otherwise, <c>false</c>.</value>
        public bool AddNewLines { get; set; }

        /// <summary>
        /// Gets or sets the mail subject.
        /// </summary>
        [DefaultValue("Message from NLog on ${machinename}")]
        public Layout Subject { get; set; }

        /// <summary>
        /// Gets or sets mail message body (repeated for each log message send in one mail).
        /// </summary>
        /// <remarks>Alias for <see cref="Layout"/> property.</remarks>
        [DefaultValue("${message}")]
        public Layout Body
        {
            get { return this.Layout; }
            set { this.Layout = value; }
        }

        /// <summary>
        /// Gets or sets encoding to be used for sending e-mail.
        /// </summary>
        [DefaultValue("UTF8")]
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to send message as HTML instead of plain text.
        /// </summary>
        [DefaultValue(false)]
        public bool Html { get; set; }
        
        /// <summary>
        /// Gets or sets SMTP Server to be used for sending.
        /// </summary>
        public Layout SmtpServer { get; set; }

        /// <summary>
        /// Gets or sets SMTP Authentication mode.
        /// </summary>
        [DefaultValue("None")]
        public SmtpAuthenticationMode SmtpAuthentication { get; set; }

        /// <summary>
        /// Gets or sets the username used to connect to SMTP server (used when SmtpAuthentication is set to "basic").
        /// </summary>
        public Layout SmtpUsername { get; set; }

        /// <summary>
        /// Gets or sets the password used to authenticate against SMTP server (used when SmtpAuthentication is set to "basic").
        /// </summary>
        public Layout SmtpPassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL (secure sockets layer) should be used when communicating with SMTP server.
        /// </summary>
        [DefaultValue(false)]
        public bool EnableSsl { get; set; }

        /// <summary>
        /// Gets or sets the port number that SMTP Server is listening on.
        /// </summary>
        [DefaultValue(25)]
        public int SmtpPort { get; set; }

        /// <summary>
        /// Adds all layouts used by this target to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(ICollection<Layout> layouts)
        {
            base.PopulateLayouts(layouts);
            if (this.From != null)
            {
                this.From.PopulateLayouts(layouts);
            }

            if (this.To != null)
            {
                this.To.PopulateLayouts(layouts);
            }

            if (this.CC != null)
            {
                this.CC.PopulateLayouts(layouts);
            }

            if (this.Bcc != null)
            {
                this.Bcc.PopulateLayouts(layouts);
            }

            if (this.Subject != null)
            {
                this.Subject.PopulateLayouts(layouts);
            }
        }

        /// <summary>
        /// Renders the logging event message and adds it to the internal ArrayList of log messages.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected internal override void Write(LogEventInfo logEvent)
        {
            this.Write(new LogEventInfo[1] { logEvent });
        }

        /// <summary>
        /// Renders an array logging events.
        /// </summary>
        /// <param name="events">Array of logging events.</param>
        protected internal override void Write(LogEventInfo[] events)
        {
            if (events == null)
            {
                return;
            }

            if (events.Length == 0)
            {
                return;
            }

            if (this.From == null || this.To == null || this.Subject == null)
            {
                return;
            }

            LogEventInfo lastEvent = events[events.Length - 1];
            string bodyText;

            // unbuffered case, create a local buffer, append header, body and footer
            StringBuilder bodyBuffer = new StringBuilder();
            if (Header != null)
            {
                bodyBuffer.Append(Header.GetFormattedMessage(lastEvent));
                if (this.AddNewLines)
                {
                    bodyBuffer.Append("\n");
                }
            }

            for (int i = 0; i < events.Length; ++i)
            {
                bodyBuffer.Append(this.Layout.GetFormattedMessage(events[i]));
                if (this.AddNewLines)
                {
                    bodyBuffer.Append("\n");
                }
            }

            if (Footer != null)
            {
                bodyBuffer.Append(Footer.GetFormattedMessage(lastEvent));
                if (this.AddNewLines)
                {
                    bodyBuffer.Append("\n");
                }
            }

            bodyText = bodyBuffer.ToString();

            MailMessage msg = new MailMessage();
            this.SetupMailMessage(msg, lastEvent);
            msg.Body = bodyText;
            SmtpClient client = new SmtpClient(this.SmtpServer.GetFormattedMessage(lastEvent), this.SmtpPort);
            if (this.SmtpAuthentication == SmtpAuthenticationMode.Ntlm)
            {
                client.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
            else if (this.SmtpAuthentication == SmtpAuthenticationMode.Basic)
            {
                client.Credentials = new NetworkCredential(this.SmtpUsername.GetFormattedMessage(lastEvent), this.SmtpPassword.GetFormattedMessage(lastEvent));
            }

            client.EnableSsl = this.EnableSsl;
            Internal.InternalLogger.Debug("Sending mail to {0} using {1}", msg.To, this.SmtpServer);
            client.Send(msg);
        }

        private void SetupMailMessage(MailMessage msg, LogEventInfo logEvent)
        {
            msg.From = new MailAddress(this.From.GetFormattedMessage(logEvent));
            foreach (string mail in this.To.GetFormattedMessage(logEvent).Split(';'))
            {
                msg.To.Add(mail);
            }

            if (this.Bcc != null)
            {
                foreach (string mail in this.Bcc.GetFormattedMessage(logEvent).Split(';'))
                {
                    msg.Bcc.Add(mail);
                }
            }

            if (this.CC != null)
            {
                foreach (string mail in this.CC.GetFormattedMessage(logEvent).Split(';'))
                {
                    msg.CC.Add(mail);
                }
            }

            msg.Subject = this.Subject.GetFormattedMessage(logEvent);
            msg.BodyEncoding = this.Encoding;
            msg.IsBodyHtml = this.Html;
            msg.Priority = MailPriority.Normal;
        }
    }
}

#endif
