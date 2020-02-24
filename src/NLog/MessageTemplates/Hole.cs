// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.MessageTemplates
{
    /// <summary>
    /// A hole that will be replaced with a value
    /// </summary>
    internal struct Hole
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Hole(string name, string format, CaptureType captureType, short position, short alignment)
        {
            Name = name;
            Format = format;
            CaptureType = captureType;
            Index = position;
            Alignment = alignment;
        }

        /// <summary>Parameter name sent to structured loggers.</summary>
        /// <remarks>This is everything between "{" and the first of ",:}". 
        /// Including surrounding spaces and names that are numbers.</remarks>
        public readonly string Name;
        /// <summary>Format to render the parameter.</summary>
        /// <remarks>This is everything between ":" and the first unescaped "}"</remarks>
        public readonly string Format;
        /// <summary>
        /// Type
        /// </summary>
        public readonly CaptureType CaptureType;
        /// <summary>When the template is positional, this is the parsed name of this parameter.</summary>
        /// <remarks>For named templates, the value of Index is undefined.</remarks>
        public readonly short Index;
        /// <summary>Alignment to render the parameter, by default 0.</summary>
        /// <remarks>This is the parsed value between "," and the first of ":}"</remarks>
        public readonly short Alignment;
    }
}