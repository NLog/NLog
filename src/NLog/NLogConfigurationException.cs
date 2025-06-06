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

namespace NLog
{
    using System;
    using System.ComponentModel;
    using JetBrains.Annotations;

    /// <summary>
    /// Exception thrown during NLog configuration.
    /// </summary>
#if NETFRAMEWORK
    [Serializable]
#endif
    public class NLogConfigurationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NLogConfigurationException" /> class.
        /// </summary>
        public NLogConfigurationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogConfigurationException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public NLogConfigurationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Obsolete and replaced by using normal string-interpolation with <see cref="NLogConfigurationException(string)"/> in NLog v5.
        /// Initializes a new instance of the <see cref="NLogConfigurationException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageParameters">Parameters for the message</param>
        [Obsolete("Instead use string interpolation. Marked obsolete with NLog 5.0")]
        [StringFormatMethod("message")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public NLogConfigurationException(string message, params object?[] messageParameters)
            : base(string.Format(message, messageParameters))
        {
        }

        /// <summary>
        /// Obsolete and replaced by using normal string-interpolation with <see cref="NLogConfigurationException(string)"/> in NLog v5.
        /// Initializes a new instance of the <see cref="NLogConfigurationException" /> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageParameters">Parameters for the message</param>
        [Obsolete("Instead use string interpolation. Marked obsolete with NLog 5.0")]
        [StringFormatMethod("message")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public NLogConfigurationException(Exception? innerException, string message, params object?[] messageParameters)
            : base(string.Format(message, messageParameters), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogConfigurationException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public NLogConfigurationException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }

#if NETFRAMEWORK
        /// <summary>
        /// Initializes a new instance of the <see cref="NLogConfigurationException" /> class.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="info"/> parameter is null.
        /// </exception>
        /// <exception cref="System.Runtime.Serialization.SerializationException">
        /// The class name is null or <see cref="System.Exception.HResult"/> is zero (0).
        /// </exception>
        protected NLogConfigurationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
