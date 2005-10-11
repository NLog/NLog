// 
// Copyright (c) 2004,2005 Jaroslaw Kowalski <jkowalski@users.sourceforge.net>
// 
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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

#if !NETCF
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Diagnostics;
using System.Reflection;

using System.Web.Mail;

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
    /// <xml src="examples/targets/Mail/MailTarget.nlog" />
    /// <p>
    /// This assumes just one target and a single rule. More configuration
    /// options are described <a href="config.html">here</a>.
    /// </p>
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <cs src="examples/targets/Mail/MailTarget.cs" />
    /// <p>
    /// Mail target works best when used with BufferingWrapper target
    /// which lets you send multiple logging messages in single mail
    /// </p>
    /// <p>
    /// To set up the buffered mail target in the <a href="config.html">configuration file</a>, 
    /// use the following syntax:
    /// </p>
    /// <xml src="examples/targets/Mail/BufferedMailTarget.nlog" />
    /// <p>
    /// To set up the buffered mail target programmatically use code like this:
    /// </p>
    /// <cs src="examples/targets/Mail/BufferedMailTarget.cs" />
    /// </example>
    [Target("Mail",IgnoresLayout=true)]
    public class MailTarget: Target
    {
        private Layout _from;
        private Layout _to;
        private Layout _cc;
        private Layout _bcc;
        private Layout _subject;
        private Layout _header;
        private Layout _footer;
        private Layout _body = new Layout("${message}");
        private Encoding _encoding = System.Text.Encoding.UTF8;
        private string _smtpServer;
        private string _smtpUsername;
        private string _smtpPassword;
        private string _smtpAuthentication = "none";
        private int _smtpPort = 25;
        private bool _isHtml = false;

        /// <summary>
        /// Creates a new instance of <see cref="MailTarget"/>.
        /// </summary>
        public MailTarget()
        {
        }

        /// <summary>
        /// Sender's email address (e.g. joe@domain.com)
        /// </summary>
        public string From
        {
            get { return _from.Text; }
            set { _from = new Layout(value); }
        }

        /// <summary>
        /// Recipients' email addresses separated by semicolons (e.g. john@domain.com;jane@domain.com)
        /// </summary>
        public string To
        {
            get { return _to.Text; }
            set { _to = new Layout(value); }
        }

        /// <summary>
        /// CC email addresses separated by semicolons (e.g. john@domain.com;jane@domain.com)
        /// </summary>
        public string CC
        {
            get { return _cc.Text; }
            set { _cc = new Layout(value); }
        }

        /// <summary>
        /// BCC email addresses separated by semicolons (e.g. john@domain.com;jane@domain.com)
        /// </summary>
        public string BCC
        {
            get { return _bcc.Text; }
            set { _bcc = new Layout(value); }
        }

        /// <summary>
        /// Mail subject.
        /// </summary>
        public string Subject
        {
            get { return _subject.Text; }
            set { _subject = new Layout(value); }
        }

        /// <summary>
        /// Header of the mail message body.
        /// </summary>
        public string Header
        {
            get { return _header.Text; }
            set { _header = new Layout(value); }
        }

        /// <summary>
        /// Footer of the mail message body.
        /// </summary>
        public string Footer
        {
            get { return _footer.Text; }
            set { _footer = new Layout(value); }
        }

        /// <summary>
        /// Mail message body (repeated for each log message send in one mail)
        /// </summary>
        [System.ComponentModel.DefaultValue("${message}")]
        public string Body
        {
            get { return _body.Text; }
            set { _body = new Layout(value); }
        }

        /// <summary>
        /// Encoding to be used for sending e-mail.
        /// </summary>
        [System.ComponentModel.DefaultValue("UTF8")]
        public string Encoding
        {
            get { return _encoding.EncodingName; }
            set { _encoding = System.Text.Encoding.GetEncoding(value); }
        }

        /// <summary>
        /// Send message as HTML instead of plain text.
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool HTML
        {
            get { return _isHtml; }
            set { _isHtml = value; }
        }

        /// <summary>
        /// SMTP Server to be used for sending.
        /// </summary>
        public string SMTPServer
        {
            get { return _smtpServer; }
            set { _smtpServer = value; }
        }

        /// <summary>
        /// SMTP Authentication mode.
        /// </summary>
        /// <remarks>
        /// Possible values are:
        /// <ul>
        /// <li><b>none</b> - no authentication</li>
        /// <li><b>basic</b> - SMTPUsername and SMTPPassword are used to provide the credentials</li>
        /// <li><b>ntlm</b> - integrated Windows authentication</li>
        /// </ul>
        /// </remarks>
        [System.ComponentModel.DefaultValue("none")]
        public string SMTPAuthentication
        {
            get { return _smtpAuthentication; }
            set { 
                AssertFieldsSupport("SMTPAuthentication");
                string n = value.ToLower();
                if (n == "none" || n == "basic" || n == "ntlm")
                    _smtpAuthentication = n;
                else
                    throw new ArgumentException("Invalid argument to SMTPAuthentication. Allowed values are none,basic or ntlm");
            }
        }

        /// <summary>
        /// The username used to connect to SMTP server (used when SMTPAuthentication is set to "basic").
        /// </summary>
        public string SMTPUsername
        {
            get { return _smtpUsername; }
            set { 
                AssertFieldsSupport("SMTPUsername");
                _smtpUsername = value;
            }
        }

        /// <summary>
        /// The password used to authenticate against SMTP server (used when SMTPAuthentication is set to "basic").
        /// </summary>
        public string SMTPPassword
        {
            get { return _smtpPassword; }
            set { 
                AssertFieldsSupport("SMTPPassword");
                _smtpPassword = value;
            }
        }

        /// <summary>
        /// The port that SMTP Server is listening on.
        /// </summary>
        [System.ComponentModel.DefaultValue(25)]
        public int SMTPPort
        {
            get { return _smtpPort; }
            set
            { 
                if (value != 25)
                    AssertFieldsSupport("SMTPPort");
                _smtpPort = value; 
            }
        }

        /// <summary>
        /// Adds all layouts used by this target to the specified collection.
        /// </summary>
        /// <param name="layouts">The collection to add layouts to.</param>
        public override void PopulateLayouts(LayoutCollection layouts)
        {
            base.PopulateLayouts (layouts);
            if (_from != null) layouts.Add(_from);
            if (_to != null) layouts.Add(_to);
            if (_cc != null) layouts.Add(_cc);
            if (_bcc != null) layouts.Add(_bcc);
            if (_subject != null) layouts.Add(_subject);
            if (_header != null) layouts.Add(_header);
            if (_footer != null) layouts.Add(_footer);
            if (_body != null) layouts.Add(_body);
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
                return;
            if (events.Length == 0)
                return;

            if (_from == null || _to == null || _subject == null)
                return;

            LogEventInfo lastEvent = events[events.Length - 1];
            string bodyText;

            // unbuffered case, create a local buffer, append header, body and footer
            StringBuilder bodyBuffer = new StringBuilder();
            if (_header != null)
                bodyBuffer.Append(_header.GetFormattedMessage(lastEvent));
            if (_body != null)
            {
                for (int i = 0; i < events.Length; ++i)
                {
                    bodyBuffer.Append(_body.GetFormattedMessage(events[i]));
                }
            }
            if (_footer != null)
                bodyBuffer.Append(_footer.GetFormattedMessage(lastEvent));

            bodyText = bodyBuffer.ToString();

            MailMessage msg = new MailMessage();
            SetupMailMessage(msg, lastEvent);
            msg.Body = bodyText;
            SendMessage(msg);
        }

        private void SetupMailMessage(MailMessage msg, LogEventInfo logEvent)
        {
            msg.From = _from.GetFormattedMessage(logEvent);
            msg.To = _to.GetFormattedMessage(logEvent);
            if (_bcc != null)
                msg.Bcc = _bcc.GetFormattedMessage(logEvent);

            if (_cc != null)
                msg.Cc = _cc.GetFormattedMessage(logEvent);
            msg.Subject = _subject.GetFormattedMessage(logEvent);
            msg.BodyEncoding = System.Text.Encoding.UTF8;
            msg.BodyFormat = HTML ? MailFormat.Html : MailFormat.Text;
            msg.Priority = MailPriority.Normal;

            IDictionary fields = GetFieldsDictionary(msg);
            if (fields != null)
            {
                if (SMTPPort != 25)
                {
                    fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserverport", SMTPPort);
                }
                if (SMTPAuthentication == "basic")
                {
                    fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1");	
                    fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", SMTPUsername);
                    fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", SMTPPassword);
                }
                if (SMTPAuthentication == "ntlm")
                {
                    fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "2");	
                }
            }
        }

        private void SendMessage(MailMessage msg)
        {
            SmtpMail.SmtpServer = _smtpServer;
            Internal.InternalLogger.Debug("Sending mail to {0} using {1}", msg.To, _smtpServer);

            SmtpMail.Send(msg);
        }

        // .NET 1.0 doesn't support "MailMessage.Fields". We want to be portable so 
        // we detect this at runtime.

        private PropertyInfo _fieldsProperty = typeof(MailMessage).GetProperty("Fields");

        private void AssertFieldsSupport(string fieldName)
        {
            if (_fieldsProperty == null)
                throw new ArgumentException("Parameter " + fieldName + " isn't supported on this runtime version.");
        }

        private IDictionary GetFieldsDictionary(MailMessage m)
        {
            if (_fieldsProperty == null)
                return null;

            return (IDictionary)_fieldsProperty.GetValue(m, null);
        }
    }
}

#endif
