// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !SILVERLIGHT

namespace NLog.Internal
{
    using System;
    using System.Net;
    using System.Net.Mail;

    /// <summary>
    /// Supports mocking of SMTP Client code.
    /// </summary>
    internal interface ISmtpClient : IDisposable
    {
        /// <summary>
        /// Specifies how outgoing email messages will be handled.
        /// </summary>
        SmtpDeliveryMethod DeliveryMethod { get; set; }

        /// <summary>
        /// Gets or sets the name or IP address of the host used for SMTP transactions.
        /// </summary>
        string Host { get; set; }

        /// <summary>
        /// Gets or sets the port used for SMTP transactions.
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies the amount of time after which a synchronous <see cref="Send">Send</see> call times out.
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// Gets or sets the credentials used to authenticate the sender.
        /// </summary>
        ICredentialsByHost Credentials { get; set; }

        bool EnableSsl { get; set; }

        /// <summary>
        /// Sends an e-mail message to an SMTP server for delivery. These methods block while the message is being transmitted.
        /// </summary>
        /// <param name="msg">
        ///   <typeparam>System.Net.Mail.MailMessage
        ///     <name>MailMessage</name>
        /// </typeparam> A <see cref="MailMessage">MailMessage</see> that contains the message to send.</param>
        void Send(MailMessage msg);

        /// <summary>
        /// Gets or sets the folder where applications save mail messages to be processed by the local SMTP server.
        /// </summary>
        string PickupDirectoryLocation { get; set; }
    }
}

#endif