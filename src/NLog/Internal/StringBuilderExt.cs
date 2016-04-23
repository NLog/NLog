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

using System;
using System.IO;
using System.Text;
using NLog.Config;
using NLog.Internal.FileAppenders;

using NLog.Internal.Pooling;
using NLog.Internal.Pooling.Pools;

namespace NLog.Internal
{
    /// <summary>
    /// Helpers for <see cref="StringBuilder"/>, which is used in e.g. layout renderers.
    /// </summary>
    internal static class StringBuilderExt
    {
        /// <summary>
        /// Append a value and use formatProvider of <paramref name="logEvent"/> or <paramref name="configuration"/> to convert to string.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="o">value to append.</param>
        /// <param name="logEvent">current logEvent for FormatProvider.</param>
        /// <param name="configuration">Configuration for DefaultCultureInfo</param>
        public static void Append(this StringBuilder builder, object o, LogEventInfo logEvent, LoggingConfiguration configuration)
        {
            var formatProvider = logEvent.FormatProvider;
            if (formatProvider == null && configuration != null)
            {
                formatProvider = configuration.DefaultCultureInfo;
            }
            builder.Append(Convert.ToString(o, formatProvider));
        }


        public static void WriteTo(this StringBuilder builder, BaseFileAppender appender, LoggingConfiguration configuration, Encoding encoding)
        {
#if NET4_5
            char[] array;
            MemoryStream ms;

            if (configuration != null && configuration.PoolConfiguration.Enabled)
            {
                array = configuration.PoolFactory.Get<CharArrayPool, char[]>().Get(builder.Length);
                // We need at most 8 times the amount of chars to store each character, probably less, 
                // unless we are writing korean chinese or other multi byte language
                ms = configuration.PoolFactory.Get<MemoryStreamPool, MemoryStream>().Get();
            }
            else
            {
                array = new char[builder.Length];
                ms = new MemoryStream(builder.Length * 8);
            }
            ms.Capacity = builder.Length * 8;

            // Copy contents of string builder to array
            builder.CopyTo(0, array, 0, builder.Length);

            var bytes = ms.GetBuffer();

            int bytesWritten = encoding.GetBytes(array, 0, builder.Length, bytes, 0);

            appender.Write(bytes, 0, bytesWritten);

            configuration.PutBack(array);
            configuration.PutBack(ms);

#else
            var str = builder.ToString();
            byte[] bytes = encoding.GetBytes(str);
            appender.Write(bytes, 0, bytes.Length);
#endif
        }

        public static void WriteTo(this StringBuilder builder, Stream stream, LoggingConfiguration configuration, Encoding encoding)
        {
#if NET4_5
            char[] array;
            MemoryStream ms;

            if (configuration.PoolingEnabled())
            {
                array = configuration.PoolFactory.Get<CharArrayPool, char[]>().Get(builder.Length);
                // We need at most 8 times the amount of chars to store each character, probably less, 
                // unless we are writing korean chinese or other multi byte language
                ms = configuration.PoolFactory.Get<MemoryStreamPool, MemoryStream>().Get();
            }
            else
            {
                array = new char[builder.Length];
                ms = new MemoryStream(builder.Length * 8);
            }
            ms.Capacity = builder.Length * 8;

            // Copy contents of string builder to array
            builder.CopyTo(0, array, 0, builder.Length);

            var bytes = ms.GetBuffer();
            int bytesWritten = encoding.GetBytes(array, 0, builder.Length, bytes, 0);

            stream.Write(bytes, 0, bytesWritten);

            configuration.PutBack(array);
            configuration.PutBack(ms);

#else
            var str = builder.ToString();
            byte[] bytes = encoding.GetBytes(str);
            stream.Write(bytes, 0, bytes.Length);
#endif
        }
    }
}
