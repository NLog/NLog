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

namespace NLog.Internal
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using NLog.Common;

    /// <summary>
    /// Stream helpers
    /// </summary>
    internal static class StreamHelpers
    {
        /// <summary>
        /// UTF-8 Byte Order Mark (BOM) bytes: EF BB BF (239, 187, 191).
        /// </summary>
        private static readonly byte[] Utf8BOM = Encoding.UTF8.GetPreamble();

        /// <summary>
        /// Copy to output stream and skip BOM if encoding is UTF8
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="encoding"></param>
        public static void CopyAndSkipBom(this Stream input, Stream output, Encoding encoding)
        {
            var bomSize = Utf8BOM.Length;
            var bomBuffer = new byte[bomSize];
            var posBefore = input.Position;
            int bytesRead = input.Read(bomBuffer, 0, bomSize);

            //TODO support other BOMs, like UTF16
            if (bytesRead == bomSize && bomBuffer.SequenceEqual(Utf8BOM))
            {
                InternalLogger.Debug("input has UTF8 BOM");
                //already skipped due to read
            }
            else
            {
                InternalLogger.Debug("input hasn't a UTF8 BOM");
                //reset position
                input.Position = posBefore;
            }

            Copy(input, output);
        }


        /// <summary>
        /// Copy stream input to output. Skip the first bytes
        /// </summary>
        /// <param name="input">stream to read from</param>
        /// <param name="output">stream to write to</param>
        /// <remarks>.net35 doesn't have a .copyto</remarks>
        public static void Copy(this Stream input, Stream output)
        {
            CopyWithOffset(input, output, 0);
        }

        /// <summary>
        /// Copy stream input to output. Skip the first bytes
        /// </summary>
        /// <param name="input">stream to read from</param>
        /// <param name="output">stream to write to</param>
        /// <param name="offset">first bytes to skip (optional)</param>
        public static void CopyWithOffset(this Stream input, Stream output, int offset)
        {
            if (offset < 0)
            {
                throw new ArgumentException("cannot be negative", nameof(offset));
            }

            if (offset > 0)
            {
                //skip offset
                input.Seek(offset, SeekOrigin.Current);
            }

            byte[] buffer = new byte[4096];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }

        /// <summary>
        /// Copies the contents of the StringBuilder to the MemoryStream using the specified encoding (Without BOM/Preamble)
        /// </summary>
        /// <param name="builder">StringBuilder source</param>
        /// <param name="ms">MemoryStream destination</param>
        /// <param name="encoding">Encoding used for converter string into byte-stream</param>
        /// <param name="transformBuffer">Helper char-buffer to minimize memory allocations</param>
        public static void CopyToStream(this StringBuilder builder, MemoryStream ms, Encoding encoding, char[] transformBuffer)
        {
            int charCount;
            int byteCount = encoding.GetMaxByteCount(builder.Length);
            long position = ms.Position;
            ms.SetLength(position + byteCount);
            for (int i = 0; i < builder.Length; i += transformBuffer.Length)
            {
                charCount = Math.Min(builder.Length - i, transformBuffer.Length);
                builder.CopyTo(i, transformBuffer, 0, charCount);
                byteCount = encoding.GetBytes(transformBuffer, 0, charCount, ms.GetBuffer(), (int)position);
                position += byteCount;
            }
            ms.Position = position;
            if (position != ms.Length)
            {
                ms.SetLength(position);
            }
        }
    }
}
