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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog.Config;

namespace NLog.Internal
{
    internal static class EncodingHelpers
    {
        private const string IncludeBomSuffix = "-bom";
        private const string ExcludeBomSuffix = "-no-bom";

        private static Dictionary<string, EncodingInfo> bomEncodings;

        /// <summary>
        /// UTF-8 BOM 239, 187, 191
        /// </summary>
        internal static readonly byte[] Utf8BOM = { 0xEF, 0xBB, 0xBF };

        static EncodingHelpers()
        {
            EncodingInfo UTF8 = new EncodingInfoDefaultNoBom(new UTF8Encoding(true), new UTF8Encoding(false));
            EncodingInfo UTF16LE = new EncodingInfoDefaultBom(new UnicodeEncoding(false, true), new UnicodeEncoding(false, false));
            EncodingInfo UTF16BE = new EncodingInfoDefaultBom(new UnicodeEncoding(true, true), new UnicodeEncoding(true, false));
            EncodingInfo UTF32LE = new EncodingInfoDefaultBom(new UTF32Encoding(false, true), new UTF32Encoding(false, false));
            EncodingInfo UTF32BE = new EncodingInfoDefaultBom(new UTF32Encoding(true, true), new UTF32Encoding(true, false));

            // NOTE: Encodings that do not specify a byte-order are defaulted to little endian, which matches
            //       the values returned by Encoding.GetEncoding(). It may be beneficial to at some point
            //       use System.BitConverter.IsLittleEndian to choose the appropriate default based on the 
            //       runtime's native byte order.

            bomEncodings = new Dictionary<string, EncodingInfo>(StringComparer.OrdinalIgnoreCase)
            {
                // UTF-8 encoding names..
                {"unicode-1-1-utf-8", UTF8},
                {"unicode-2-0-utf-8", UTF8},
                {"utf-8", UTF8},
                {"x-unicode-1-1-utf-8", UTF8},
                {"x-unicode-2-0-utf-8", UTF8},

                // UTF-16BE encoding names
                {"unicodeFFFE", UTF16BE},
                {"UTF-16BE", UTF16BE},

                // UTF-16LE encoding names
                {"ISO-10646-UCS-2", UTF16LE},
                {"ucs-2", UTF16LE},
                {"unicode", UTF16LE},
                {"utf-16", UTF16LE},
                {"UTF-16LE", UTF16LE},

                // UTF-32BE encoding names
                {"UTF-32BE", UTF32BE},

                // UTF-32LE encoding names
                {"utf-32", UTF32LE},
                {"UTF-32LE", UTF32LE},
            };
        }

        internal static Encoding GetEncoding(string encodingName, DefaultByteOrderMarkAttribute bomAttribute = null)
        {
            if (encodingName == null)
                throw new ArgumentNullException("encodingName");

            bool? hasBomSuffix;
            EncodingInfo encodingInfo;

            // find the EncodingInfo class associated with the encodingName parameter. If we don't have one, then
            // return the result of Encoding.GetEncoding().

            if (!bomEncodings.TryGetValue(GetEncodingBaseName(encodingName, out hasBomSuffix), out encodingInfo))
                return Encoding.GetEncoding(encodingName);

            // get the correct encoding based on the BOM suffix and defaultToBom parameter. The
            // BOM suffix always has priority, if not set then defaultToBom is used, if 
            // defaultToBom is null, then we use whatever the EncodingInfo instance indicates
            // the default should be.

            return encodingInfo.GetEncoding(hasBomSuffix, bomAttribute);
        }

        private static string GetEncodingBaseName(string encodingName, out bool? hasBomSuffix)
        {
            // NOTE: The order of the checks are important here, the longer of Include/ExcludeBomSuffix
            //       must be checked first..

            if (encodingName.EndsWith(ExcludeBomSuffix, StringComparison.OrdinalIgnoreCase))
            {
                hasBomSuffix = false;
                return encodingName.Substring(0, encodingName.Length - ExcludeBomSuffix.Length);
            }
            else if (encodingName.EndsWith(IncludeBomSuffix, StringComparison.OrdinalIgnoreCase))
            {
                hasBomSuffix = true;
                return encodingName.Substring(0, encodingName.Length - IncludeBomSuffix.Length);
            }
            else
            {
                hasBomSuffix = null;
                return encodingName;
            }
        }

        private abstract class EncodingInfo
        {
            public readonly Encoding BOM;
            public readonly Encoding NoBOM;

            protected EncodingInfo(Encoding bomEncoding, Encoding noBomEncoding)
            {
                if ((bomEncoding == null) || (noBomEncoding == null))
                    throw new ArgumentNullException((bomEncoding == null) ? "bomEncoding" : "noBomEncoding");

                BOM = bomEncoding;
                NoBOM = noBomEncoding;
            }

            public Encoding GetEncoding(bool? hasBom, DefaultByteOrderMarkAttribute bomAttribute = null)
            {
                return (hasBom ?? (bomAttribute ?? DefaultByteOrderMarkAttribute.Default).GetCodepageState(BOM.CodePage) ?? DefaultToBom) ? BOM : NoBOM;
            }

            protected abstract bool DefaultToBom { get; }
        }

        private sealed class EncodingInfoDefaultBom : EncodingInfo
        {
            public EncodingInfoDefaultBom(Encoding bomEncoding, Encoding noBomEncoding)
                : base(bomEncoding, noBomEncoding)
            {
            }

            protected override bool DefaultToBom
            {
                get { return true; }
            }
        }

        private sealed class EncodingInfoDefaultNoBom : EncodingInfo
        {
            public EncodingInfoDefaultNoBom(Encoding bomEncoding, Encoding noBomEncoding)
                : base(bomEncoding, noBomEncoding)
            {
            }

            protected override bool DefaultToBom
            {
                get { return false; }
            }
        }
    }
}
